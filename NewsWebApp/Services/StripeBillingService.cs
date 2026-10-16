using System.Data;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Services;

public sealed class StripeBillingService(ApplicationDbContext db, IStripeBillingGateway stripe, IConfiguration config)
{
    public async Task<string> StartCheckoutAsync(string userId, CancellationToken ct)
    {
        await stripe.GetPriceAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var userQuery = db.Database.IsSqlServer()
            ? db.Users.FromSqlInterpolated($"SELECT * FROM [AspNetUsers] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {userId}")
            : db.Users.Where(u => u.Id == userId);
        var user = await userQuery.SingleAsync(ct);
        if (string.IsNullOrWhiteSpace(user.Email)) throw new InvalidOperationException("Your account needs an email address.");
        if (user.StripeCustomerId is null)
            user.StripeCustomerId = (await stripe.CreateCustomerAsync(user, ct)).Id;
        var subscriptions = await stripe.GetCustomerSubscriptionsAsync(user.StripeCustomerId, ct);
        if (subscriptions.Any(s => s.Metadata.GetValueOrDefault("AppUserId") == userId
            && s.Status is "active" or "trialing" or "past_due" or "unpaid" or "incomplete" or "paused"))
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return await stripe.CreatePortalAsync(user.StripeCustomerId, ct);
        }
        if (user.StripeCheckoutSessionId is not null)
        {
            var previous = await stripe.GetCheckoutAsync(user.StripeCheckoutSessionId, ct);
            if (previous.Status == "open" && previous.LineItems?.Data.SingleOrDefault()?.Price?.Id == config["Stripe:PriceId"])
            {
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return previous.Url;
            }
            if (previous.Status == "open") await stripe.ExpireCheckoutAsync(previous.Id, ct);
        }
        var checkout = await stripe.CreateCheckoutAsync(userId, user.StripeCustomerId, ct);
        user.StripeCheckoutSessionId = checkout.Id;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return checkout.Url;
    }
    public async Task<bool> ConfirmCheckoutAsync(string sessionId, string userId, CancellationToken ct)
    {
        var session = await stripe.GetCheckoutAsync(sessionId, ct);
        if (session.ClientReferenceId != userId) return false;
        if (session.Status == "complete" && session.PaymentStatus == "paid" && session.SubscriptionId is not null)
            await SynchronizeAsync(session.SubscriptionId, ct);
        return true;
    }
    public async Task SynchronizeAsync(string subscriptionId, CancellationToken ct)
    {
        // Read canonical Stripe state inside the transaction; replayed events cannot restore old state.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var rowQuery = db.Database.IsSqlServer()
            ? db.Subscriptions.FromSqlInterpolated($"SELECT * FROM [Subscriptions] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {subscriptionId}")
            : db.Subscriptions.Where(s => s.Id == subscriptionId);
        var row = await rowQuery.SingleOrDefaultAsync(ct);
        var subscription = await stripe.GetSubscriptionAsync(subscriptionId, ct);
        if (subscription.Livemode) throw new InvalidOperationException("Live subscriptions are not enabled.");
        var userId = subscription.Metadata.GetValueOrDefault("AppUserId");
        if (string.IsNullOrEmpty(userId)) return;
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null || user.StripeCustomerId != subscription.CustomerId) return;
        // Failed/incomplete first payments must not create a successful-subscription record.
        if (row is null && (subscription.Status != "active" || subscription.LatestInvoice?.Status != "paid")) return;
        if (row is not null && row.UserId != userId)
            throw new InvalidOperationException("Subscription ownership mismatch.");
        if (row is null)
        {
            row = new Subscriptions
            {
                Id = subscriptionId, UserId = userId, User = user,
                Name = "CLICKBAIT News Premium", CreatedAt = subscription.Created
            };
            db.Subscriptions.Add(row);
        }
        var item = subscription.Items?.Data.Count == 1 ? subscription.Items.Data[0] : null;
        var expectedPlan = item?.Price?.Id == config["Stripe:PriceId"] && item?.Quantity == 1;
        row.StripeCustomerId = subscription.CustomerId;
        row.StripeStatus = expectedPlan ? subscription.Status : "unsupported_plan";
        row.StripePriceId = item?.Price?.Id;
        row.StripeInvoicePaid = subscription.LatestInvoice?.Status == "paid";
        row.CurrentPeriodEndUtc = item?.CurrentPeriodEnd;
        row.CancelAtPeriodEnd = subscription.CancelAtPeriodEnd;
        row.Price = 99.99m;
        row.DurationInDays = 0; // Use the Stripe period, not an assumed 30 days.
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
    public async Task<string> OpenPortalAsync(string userId, CancellationToken ct)
    {
        var customer = await db.Users.Where(u => u.Id == userId).Select(u => u.StripeCustomerId).SingleAsync(ct);
        if (customer is null) throw new InvalidOperationException("No Stripe billing account exists for this user.");
        return await stripe.CreatePortalAsync(customer, ct);
    }
}
