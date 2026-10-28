using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SendNewsletterFromQueue;

public class SendNewsletter(ILogger<SendNewsletter> logger,
    IDbContextFactory<FuncDbContext> dbContextFactory, IConfiguration configuration)
{
    [Function("SendNewsletter")]
    public async Task RunAsync(
        // Keep Saturday's schedule, but never resend just because the host restarts.
        [TimerTrigger("0 0 7 * * 6", RunOnStartup = false)] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var storage = configuration["AzureWebJobsStorage"]
            ?? throw new InvalidOperationException("Missing AzureWebJobsStorage setting.");
        var queueName = configuration["QueueName"];
        if (string.IsNullOrWhiteSpace(queueName)) throw new InvalidOperationException("Missing QueueName setting.");
        var queue = new QueueClient(storage, queueName,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var users = await db.AspNetUsers.AsNoTracking().Where(u => u.Email != null && u.Email != ""
            && (u.IsSubscribedToNewsletter || u.IsSubscribedToNewsNewsletter || u.IsSubscribedToWorldNewsletter
                || u.IsSubscribedToSwedenNewsletter || u.IsSubscribedToWeatherNewsletter || u.IsSubscribedToSportsNewsletter))
            .ToListAsync(cancellationToken);
        int count = 0;
        foreach (var request in NewsletterPreferences.BuildRequests(users))
        {
            await queue.SendMessageAsync(JsonSerializer.Serialize(request), cancellationToken);
            count++;
        }
        logger.LogInformation("Queued {Count} newsletter deliveries in {QueueName}.", count, queueName);
    }
}
