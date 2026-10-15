using NewsWebApp.Data;
using Stripe;
using Stripe.Checkout;

namespace NewsWebApp.Services;

public interface IStripeBillingGateway
{
    Task<Price> GetPriceAsync(CancellationToken ct);
    Task<Customer> CreateCustomerAsync(ApplicationUser user, CancellationToken ct);
    Task<Session> CreateCheckoutAsync(string userId, string customerId, CancellationToken ct);
    Task<Session> GetCheckoutAsync(string sessionId, CancellationToken ct);
    Task ExpireCheckoutAsync(string sessionId, CancellationToken ct);
    Task<List<Subscription>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken ct);
    Task<Subscription> GetSubscriptionAsync(string subscriptionId, CancellationToken ct);
    Task<string> CreatePortalAsync(string customerId, CancellationToken ct);
}

public sealed class StripeBillingGateway(IConfiguration configuration) : IStripeBillingGateway
{
    private StripeClient Client => new(Required("Stripe:SecretKey"));
    private string Required(string key) => !string.IsNullOrWhiteSpace(configuration[key])
        ? configuration[key]! : throw new InvalidOperationException($"Missing setting: {key}");
    private string BaseUrl
    {
        get
        {
            var value = Required("Stripe:PublicBaseUrl").TrimEnd('/');
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
                || (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback))
                || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0)
                throw new InvalidOperationException("Stripe:PublicBaseUrl must be your HTTPS website URL (HTTP is allowed for localhost).");
            return value;
        }
    }
    public async Task<Price> GetPriceAsync(CancellationToken ct)
    {
        
        var price = await new PriceService(Client).GetAsync(Required("Stripe:PriceId"), cancellationToken: ct);
        if (!price.Active || price.Livemode || price.Currency != "sek" || price.Type != "recurring"
            || price.UnitAmount != 9999 || price.Recurring?.Interval != "month"
            || price.Recurring.IntervalCount != 1 || price.Recurring.UsageType != "licensed")
            throw new InvalidOperationException("Configure the active test-mode price for 99.99 SEK per month.");
        return price;
    }
    public Task<Customer> CreateCustomerAsync(ApplicationUser user, CancellationToken ct) =>
        new CustomerService(Client).CreateAsync(new CustomerCreateOptions
        { Email = user.Email, Metadata = new() { ["AppUserId"] = user.Id } },
            new RequestOptions { IdempotencyKey = $"news-customer-{user.Id}" }, ct);
    public Task<Session> CreateCheckoutAsync(string userId, string customerId, CancellationToken ct) =>
        new SessionService(Client).CreateAsync(new SessionCreateOptions
        {
            Mode = "subscription", Customer = customerId, ClientReferenceId = userId,
            LineItems = [new() { Price = Required("Stripe:PriceId"), Quantity = 1 }],
            SuccessUrl = BaseUrl + "/Subscription/Confirmation?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = BaseUrl + "/Subscription/Index?cancelled=true",
            SubscriptionData = new() { Metadata = new() { ["AppUserId"] = userId } },
            Metadata = new() { ["AppUserId"] = userId }
        }, new RequestOptions { IdempotencyKey = $"news-checkout-{userId}-{Guid.NewGuid():N}" }, ct);
    public Task<Session> GetCheckoutAsync(string sessionId, CancellationToken ct) =>
        new SessionService(Client).GetAsync(sessionId, new SessionGetOptions { Expand = ["line_items"] }, cancellationToken: ct);
    public async Task ExpireCheckoutAsync(string sessionId, CancellationToken ct) =>
        await new SessionService(Client).ExpireAsync(sessionId, cancellationToken: ct);
    public async Task<List<Subscription>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken ct)
    {
        var subscriptions = new List<Subscription>();
        await foreach (var subscription in new Stripe.SubscriptionService(Client).ListAutoPagingAsync(
            new SubscriptionListOptions { Customer = customerId, Status = "all", Limit = 100 }, cancellationToken: ct))
            subscriptions.Add(subscription);
        return subscriptions;
    }
    public Task<Subscription> GetSubscriptionAsync(string subscriptionId, CancellationToken ct) =>
        new Stripe.SubscriptionService(Client).GetAsync(subscriptionId,
            new SubscriptionGetOptions { Expand = ["latest_invoice"] }, cancellationToken: ct);
    public async Task<string> CreatePortalAsync(string customerId, CancellationToken ct) =>
        (await new Stripe.BillingPortal.SessionService(Client).CreateAsync(
            new Stripe.BillingPortal.SessionCreateOptions { Customer = customerId, ReturnUrl = BaseUrl + "/Subscription/MySubscription" },
            cancellationToken: ct)).Url;
}
