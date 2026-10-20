using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Timers;
using Azure.Storage.Queues;

namespace SubscriptionReminder;

public class SendSubscriptionReminder
{
    private readonly ILogger _logger;

    private readonly IDbContextFactory<FunctionDbContext> _dbContextFactory;
    private readonly IConfiguration _configuration;

    public SendSubscriptionReminder(ILoggerFactory loggerFactory, IConfiguration configuration, IDbContextFactory<FunctionDbContext> dbContextFactory)
    {
        _logger = loggerFactory.CreateLogger<SendSubscriptionReminder>();
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }

    [Function("SendSubscriptionReminder")]
    public async Task RunAsync(
            [TimerTrigger("0 0 7  * * 6", RunOnStartup = true)] TimerInfo timer,
            CancellationToken cancellationToken)
    {
        string storageConnectionString = GetRequiredSetting("AzureWebJobsStorage");
        string queueName = _configuration["QueueName"] ?? "";

        var queueClient = new QueueClient(storageConnectionString, queueName,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);


        // The context is disposed automatically when this function invocation completes.
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        //List<string> emailAddresses = await dbContext.AspNetUsers
        //    .AsNoTracking()
        //    .Where(user =>
        //        user.Email != null &&
        //        user.Email != string.Empty)
        //    .Where(subscription => subscription.SubscriptionEndDate.HasValue &&
        //        subscription.SubscriptionEndDate.Value.Year <= latestBirthYearForAdults)
        //    .Select(user => user.Email!)
        //    .ToListAsync(cancellationToken);

        //var emailAddresses = await dbContext.AspNetUsers
        //    .AsNoTracking()
        //    .Where(user =>
        //        user.Email != null &&
        //        user.Email != string.Empty &&
        //        user.SubscriptionEndDate.HasValue &&
        //        user.SubscriptionEndDate.Value.Year <= latestBirthYearForAdults)
        //    .Select(user => user.Email!)
        //    .ToListAsync(cancellationToken);

        var query = dbContext.AspNetUsers.Join(
            dbContext.Subscriptions,
            user => user.Id,
            subscription => subscription.UserId,
            (user, subscription) => new { User = user, Subscription = subscription })
            .Where(joined => joined.User.Email != null &&
                             joined.User.Email != string.Empty &&
                             joined.Subscription.SubscriptionEndDate.HasValue &&
                             joined.Subscription.SubscriptionEndDate.Value.Day == 25)
            .Select(joined => joined.User.Email!);

        foreach (string emailAddress in query)
        {
            string message = JsonSerializer.Serialize(new
            {
                EmailAddress = emailAddress
            });

            await queueClient.SendMessageAsync(message, cancellationToken);
        }

        _logger.LogInformation(
            "Queued {UserCount} user email addresses in {QueueName}.",
            query.Count(),
            queueName);


        if (timer.ScheduleStatus is not null)
        {
            _logger.LogInformation(
                "Next timer schedule at: {NextSchedule}",
                timer.ScheduleStatus.Next);
        }
    }

    private string GetRequiredSetting(string settingName)
    {
        return _configuration[settingName]
            ?? throw new InvalidOperationException(
                $"The '{settingName}' application setting is not configured.");
    }
}
