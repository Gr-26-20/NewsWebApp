using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NewsWebApp.Controllers;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Services;
using Stripe;
using Stripe.Checkout;

const string priceId = "price_1UPiJoG2MKBLu1fVtNjMyxVj";
const string signingSecret = "whsec_unit_test_only";
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Stripe:PriceId"] = priceId, ["Stripe:WebhookSecret"] = signingSecret
}).Build();
await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
await db.Database.EnsureCreatedAsync();
db.Users.Add(new ApplicationUser { Id = "user1", UserName = "reader", Email = "reader@example.test" });
await db.SaveChangesAsync();
var gateway = new FakeStripe();
var billing = new StripeBillingService(db, gateway, config);
var access = new NewsWebApp.Services.SubscriptionService(db);
var passed = 0;
void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
async Task Test(string name, Func<Task> action)
{
    await action(); Console.WriteLine($"PASS: {name}"); passed++;
}
void PaidSubscription(string status = "active", string invoiceStatus = "paid")
{
    gateway.Subscription = new Subscription
    {
        Id = "sub_test", CustomerId = "cus_test", Status = status,
        Created = DateTime.UtcNow, Metadata = new() { ["AppUserId"] = "user1" },
        LatestInvoice = new Invoice { Id = "in_test", Status = invoiceStatus },
        Items = new StripeList<SubscriptionItem> { Data =
        [
            new SubscriptionItem { Id = "si_test", Price = new Price { Id = priceId }, Quantity = 1,
                CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) }
        ] }
    };
}
async Task<IActionResult> Webhook(string type, object data, bool valid = true, bool live = false, long? timestamp = null)
{
    var json = JsonSerializer.Serialize(new
    {
        id = "evt_test", @object = "event", api_version = StripeConfiguration.ApiVersion,
        type, livemode = live, data = new { @object = data }
    });
    var time = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var hash = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingSecret),
        Encoding.UTF8.GetBytes($"{time}.{json}"))).ToLowerInvariant();
    var http = new DefaultHttpContext();
    http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
    http.Request.Headers["Stripe-Signature"] = $"t={time},v1={(valid ? hash : "bad")}";
    var controller = new StripeWebhookController(config, billing, NullLogger<StripeWebhookController>.Instance)
    { ControllerContext = new ControllerContext { HttpContext = http } };
    return await controller.Receive(default);
}

await Test("Checkout links the account to a Stripe customer", async () =>
{
    var url = await billing.StartCheckoutAsync("user1", default);
    Check(url == "https://checkout.stripe.test/session" && gateway.CreatedUserId == "user1");
    Check((await db.Users.SingleAsync()).StripeCustomerId == "cus_test");
});
await Test("Repeated clicks reuse the same open checkout", async () =>
{
    await billing.StartCheckoutAsync("user1", default);
    Check(gateway.CheckoutsCreated == 1 && gateway.CustomersCreated == 1);
});
await Test("Starting checkout does not grant access", async () => Check(!await access.HasActiveSubscriptionAsync("user1")));
await Test("A different user cannot confirm someone else's session", async () =>
    Check(!await billing.ConfirmCheckoutAsync("cs_test", "other-user", default)));
await Test("Unpaid checkout returns no premium access", async () =>
{
    gateway.Session.Status = "complete"; gateway.Session.PaymentStatus = "unpaid"; gateway.Session.SubscriptionId = "sub_test";
    Check(await billing.ConfirmCheckoutAsync("cs_test", "user1", default));
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Failed first payment is not recorded as a successful subscription", async () =>
{
    PaidSubscription("incomplete", "open"); await billing.SynchronizeAsync("sub_test", default);
    Check(await db.Subscriptions.CountAsync() == 0);
});
await Test("Confirmed paid checkout grants premium access", async () =>
{
    PaidSubscription(); gateway.Session.PaymentStatus = "paid";
    Check(await billing.ConfirmCheckoutAsync("cs_test", "user1", default));
    Check(await access.HasActiveSubscriptionAsync("user1"));
    Check((await access.GetActiveSubscriptionAsync("user1"))?.UserId == "user1");
});
await Test("Replayed updates do not duplicate the subscription", async () =>
{
    await billing.SynchronizeAsync("sub_test", default); await billing.SynchronizeAsync("sub_test", default);
    Check(await db.Subscriptions.CountAsync() == 1);
});
await Test("Other accounts do not inherit premium access", async () =>
    Check(!await access.HasActiveSubscriptionAsync("other-user")));
await Test("Renewal updates the real paid-through date", async () =>
{
    var next = DateTime.UtcNow.AddMonths(2);
    gateway.Subscription!.Items.Data[0].CurrentPeriodEnd = next;
    await billing.SynchronizeAsync("sub_test", default);
    Check((await db.Subscriptions.SingleAsync()).CurrentPeriodEndUtc == next);
});
await Test("Cancellation at period end keeps already-paid access", async () =>
{
    gateway.Subscription!.CancelAtPeriodEnd = true;
    await billing.SynchronizeAsync("sub_test", default);
    Check(await access.HasActiveSubscriptionAsync("user1") && (await db.Subscriptions.SingleAsync()).CancelAtPeriodEnd);
});
await Test("Failed renewal removes access", async () =>
{
    gateway.Subscription!.Status = "past_due"; gateway.Subscription.LatestInvoice.Status = "open";
    await billing.SynchronizeAsync("sub_test", default);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Immediate cancellation removes access", async () =>
{
    PaidSubscription("canceled"); await billing.SynchronizeAsync("sub_test", default);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Expired paid period removes access even without another event", async () =>
{
    PaidSubscription(); gateway.Subscription!.Items.Data[0].CurrentPeriodEnd = DateTime.UtcNow.AddSeconds(-1);
    await billing.SynchronizeAsync("sub_test", default);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("An unrelated Stripe customer cannot alter this membership", async () =>
{
    PaidSubscription(); gateway.Subscription!.CustomerId = "cus_other";
    await billing.SynchronizeAsync("sub_test", default);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Wrong price does not grant access", async () =>
{
    PaidSubscription(); gateway.Subscription!.Items.Data[0].Price.Id = "price_other";
    await billing.SynchronizeAsync("sub_test", default); Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Forged webhook signature is rejected", async () =>
{
    PaidSubscription();
    Check(await Webhook("customer.subscription.updated", new { id = "sub_test", @object = "subscription" }, valid: false) is BadRequestResult);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Old webhook signatures are rejected", async () =>
    Check(await Webhook("customer.subscription.updated", new { id = "sub_test", @object = "subscription" },
        timestamp: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds()) is BadRequestResult));
await Test("Signed paid invoice event activates the subscription", async () =>
{
    Check(await Webhook("invoice.paid", new
    {
        id = "in_test", @object = "invoice",
        parent = new { type = "subscription_details", subscription_details = new { subscription = "sub_test" } }
    }) is OkResult);
    Check(await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Replayed old event uses current canceled Stripe state", async () =>
{
    gateway.Subscription!.Status = "canceled";
    Check(await Webhook("customer.subscription.updated", new { id = "sub_test", @object = "subscription", status = "active" }) is OkResult);
    Check(!await access.HasActiveSubscriptionAsync("user1"));
});
await Test("Live-mode events cannot activate test subscriptions", async () =>
    Check(await Webhook("customer.subscription.updated", new { id = "sub_test", @object = "subscription" }, live: true) is BadRequestObjectResult));
await Test("Unknown signed event is acknowledged safely", async () =>
    Check(await Webhook("customer.updated", new { id = "cus_test", @object = "customer" }) is OkResult));
await Test("Billing portal uses the logged-in account's customer", async () =>
    Check(await billing.OpenPortalAsync("user1", default) == "https://billing.stripe.test/cus_test"));
await Test("An existing unpaid subscription goes to billing rather than creating another charge", async () =>
{
    PaidSubscription("past_due", "open"); gateway.HasExistingSubscription = true;
    Check(await billing.StartCheckoutAsync("user1", default) == "https://billing.stripe.test/cus_test");
    Check(gateway.CheckoutsCreated == 1);
});
Console.WriteLine($"All {passed} checks passed. SQLite and fake Stripe only; no real API calls or payments.");
Console.WriteLine($"Stripe webhook API version: {StripeConfiguration.ApiVersion}");

sealed class FakeStripe : IStripeBillingGateway
{
    public int CustomersCreated, CheckoutsCreated;
    public string? CreatedUserId;
    public bool HasExistingSubscription;
    public Subscription? Subscription;
    public Session Session = new()
    {
        Id = "cs_test", Url = "https://checkout.stripe.test/session", Status = "open", ClientReferenceId = "user1",
        LineItems = new StripeList<LineItem> { Data = [new LineItem { Price = new Price { Id = "price_1UPiJoG2MKBLu1fVtNjMyxVj" } }] }
    };
    public Task<Price> GetPriceAsync(CancellationToken ct) => Task.FromResult(new Price());
    public Task<Customer> CreateCustomerAsync(ApplicationUser user, CancellationToken ct)
    { CustomersCreated++; return Task.FromResult(new Customer { Id = "cus_test" }); }
    public Task<Session> CreateCheckoutAsync(string userId, string customerId, CancellationToken ct)
    { CheckoutsCreated++; CreatedUserId = userId; return Task.FromResult(Session); }
    public Task<Session> GetCheckoutAsync(string id, CancellationToken ct) => Task.FromResult(Session);
    public Task ExpireCheckoutAsync(string id, CancellationToken ct) { Session.Status = "expired"; return Task.CompletedTask; }
    public Task<List<Subscription>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken ct) =>
        Task.FromResult(HasExistingSubscription ? new List<Subscription> { Subscription! } : []);
    public Task<Subscription> GetSubscriptionAsync(string id, CancellationToken ct) => Task.FromResult(Subscription!);
    public Task<string> CreatePortalAsync(string id, CancellationToken ct) => Task.FromResult("https://billing.stripe.test/" + id);
}
