using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Services;
using Stripe;

namespace NewsWebApp.Controllers;

[AllowAnonymous]
[Route("stripe/webhook")]
public class StripeWebhookController(
    IConfiguration config, StripeBillingService billing, ILogger<StripeWebhookController> logger) : ControllerBase
{
    [HttpPost, IgnoreAntiforgeryToken]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        // Retrieve the Stripe webhook secret from configuration
        var secret = config["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            logger.LogError("Stripe:WebhookSecret is not configured.");
            return StatusCode(503);
        }
        var json = await new StreamReader(Request.Body).ReadToEndAsync(cancellationToken);
        Event stripeEvent;
        try { stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"].ToString(), secret); }
        catch (StripeException) { return BadRequest(); }
        if (stripeEvent.Livemode) return BadRequest("This integration is configured for test-mode subscriptions.");
        string? subscriptionId = stripeEvent.Type
            // Determine the subscription ID based on the event type and data
            switch
        {
            "checkout.session.completed" or "checkout.session.async_payment_succeeded" =>
                (stripeEvent.Data.Object as Stripe.Checkout.Session)?.SubscriptionId,
            "customer.subscription.created" or "customer.subscription.updated" or "customer.subscription.deleted" =>
                (stripeEvent.Data.Object as Subscription)?.Id,
            "invoice.paid" or "invoice.payment_failed" or "invoice.payment_action_required" =>
                (stripeEvent.Data.Object as Invoice)?.Parent?.SubscriptionDetails?.SubscriptionId,
            _ => null
        };
        if (subscriptionId is null) return Ok();
        try
        {
            
            await billing.SynchronizeAsync(subscriptionId, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe event {EventId} could not be processed; Stripe should retry.", stripeEvent.Id);
            return StatusCode(500);
        }
    }
}
