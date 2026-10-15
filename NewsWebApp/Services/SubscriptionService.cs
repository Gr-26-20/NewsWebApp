using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Services;

public sealed class SubscriptionService(ApplicationDbContext db) : ISubscriptionService
{
    public Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return db.Subscriptions.AnyAsync(s => s.UserId == userId &&
            (s.StripeStatus != null
                ? s.StripeStatus == "active" && s.StripeInvoicePaid && s.CurrentPeriodEndUtc > now
                : s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > now));
    }
    public Task<Subscriptions?> GetActiveSubscriptionAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return db.Subscriptions.Include(s => s.User).Where(s => s.UserId == userId &&
            (s.StripeStatus != null
                ? s.StripeStatus == "active" && s.StripeInvoicePaid && s.CurrentPeriodEndUtc > now
                : s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > now))
            .OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
    }
}
