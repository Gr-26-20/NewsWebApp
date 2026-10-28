using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SendNewsletterFromQueue;

public sealed class EmailFromQueue(ILogger<EmailFromQueue> logger,
    IDbContextFactory<FuncDbContext> dbContextFactory, NewsletterDelivery delivery)
{
    [Function(nameof(EmailFromQueue))]
    public async Task RunAsync(
        // Producer and consumer must use the same queue setting.
        [QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] string queueMessage,
        CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<NewsletterRequest>(queueMessage,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (request is null || string.IsNullOrWhiteSpace(request.UserId)
            || string.IsNullOrWhiteSpace(request.EmailAddress) || !NewsletterPreferences.Categories.Contains(request.Category))
        {
            // Old email-only messages do not identify a category. Never  send all categories.
            throw new InvalidOperationException("Expected UserId, EmailAddress and Category in the newsletter queue message.");
        }
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sent = await delivery.DeliverAsync(db, request, cancellationToken);
        logger.LogInformation("Newsletter {Category} for user {UserId}: {Outcome}.",
            request.Category, request.UserId, sent ? "sent" : "skipped (no content or no longer subscribed)");
        // Send failures propagate so Azure can retry the failed delivery.
    }
}
