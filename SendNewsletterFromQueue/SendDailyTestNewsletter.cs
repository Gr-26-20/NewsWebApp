using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SendNewsletterFromQueue;

public sealed class SendDailyTestNewsletter(DailyNewsletter dailyNewsletter)
{
    [Function(nameof(SendDailyTestNewsletter))]
    public Task RunAsync([TimerTrigger("0 0 8 * * *")] TimerInfo timer, CancellationToken cancellationToken) =>
        dailyNewsletter.SendAsync(cancellationToken);
}

public sealed class DailyNewsletter(
    IDbContextFactory<FuncDbContext> factory,
    ITestNewsletterSender sender,
    TimeProvider clock,
    ILogger<DailyNewsletter> logger)
{
    public async Task SendAsync(CancellationToken ct)
    {
        
        var now = clock.GetUtcNow().UtcDateTime;
        var today = now.Date;
        // Get all users who have opted in for the newsletter, have a valid email, and haven't received a newsletter today.
        await using var db = await factory.CreateDbContextAsync(ct);
        var users = await db.Users.AsNoTracking()
            .Where(u => u.NewsletterOptIn && u.Email != null && u.Email != ""
                && (u.LastNewsletterAttemptUtc == null || u.LastNewsletterAttemptUtc < today))
            .ToListAsync(ct);
        foreach (var user in users)
        {
            ct.ThrowIfCancellationRequested();
            // Reserve this user's daily send atomically, including when two hosts run together.
            var claimed = await db.Users.Where(u => u.Id == user.Id && u.NewsletterOptIn && u.Email == user.Email
                && (u.LastNewsletterAttemptUtc == null || u.LastNewsletterAttemptUtc < today))
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastNewsletterAttemptUtc, now), ct);
            if (claimed == 0) continue;
            var submitting = false;
            try
            {
                
                await sender.SendAsync(user.Email!, () =>
                {
                    submitting = true;
                    return Task.CompletedTask;
                }, ct);
                logger.LogInformation("Daily test newsletter sent to user {UserId}.", user.Id);
            }
            catch (Exception ex)
            {
                // Connection/auth failures can be retried safely. An uncertain SMTP submission
                // keeps today's reservation so a timeout/crash cannot cause a duplicate today.
                if (!submitting)
                    await db.Users.Where(u => u.Id == user.Id && u.LastNewsletterAttemptUtc == now)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastNewsletterAttemptUtc, user.LastNewsletterAttemptUtc),
                            CancellationToken.None);
                logger.LogError(ex, "Daily newsletter failed for user {UserId}; submission started: {Submitting}.", user.Id, submitting);
                if (ct.IsCancellationRequested) throw;
            }
        }
    }
}
