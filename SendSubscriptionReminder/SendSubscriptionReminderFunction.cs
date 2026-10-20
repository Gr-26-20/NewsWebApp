using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SendSubscriptionReminder
{
    public class SendSubscriptionReminder
    {
        private readonly ILogger<SendSubscriptionReminder   > _logger;
        private readonly IDbContextFactory<FunctionDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;

        public SendSubscriptionReminder(
            ILogger<SendSubscriptionReminder> logger,
            IDbContextFactory<FunctionDbContext> dbContextFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        [Function("SendSubscriptionReminder")]
        public async Task RunAsync(
            [TimerTrigger("0 0 6  * * *", RunOnStartup = false)] TimerInfo timer,
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

            //var users = await dbContext.AspNetUsers
            //    .AsNoTracking()
            //    .Where(user => user.Email != null && user.Email != string.Empty)
            //    .ToListAsync(cancellationToken);

            //var subscriptions = await dbContext.Subscriptions
            //    .AsNoTracking()
            //    .Where(subscription => subscription.CurrentPeriodEndUtc.HasValue)
            //    .ToListAsync(cancellationToken);

            //var query = from user in users
            //            join subscription in subscriptions on user.Id equals subscription.UserId
            //            select new { User = user, Subscription = subscription };


            //foreach(var joined in query)
            //{
            //    string emailAddress = joined.User.Email!;
            //    string subscriptionEndDate = joined.Subscription.CurrentPeriodEndUtc?.ToString("yyyy-MM-dd") ?? "N/A";
            //    TimeSpan daysRemaining = joined.Subscription.CurrentPeriodEndUtc?.Subtract(DateTime.UtcNow) ?? TimeSpan.Zero;
            //    string message = JsonSerializer.Serialize(new
            //    {
            //        EmailAddress = emailAddress
            //    });

            //var emailAddresses = dbContext.AspNetUsers.Join(
            //dbContext.Subscriptions,
            //user => user.Id,
            //subscription => subscription.UserId,
            //(user, subscription) => new { User = user, Subscription = subscription })
            //.Where(joined => joined.User.Email != null &&
            //                 joined.User.Email != string.Empty &&
            //                 joined.Subscription.CurrentPeriodEndUtc.HasValue && (
            //                 joined.Subscription.CurrentPeriodEndUtc.Value - DateTime.UtcNow).TotalDays <= 7)
            //                 //joined.Subscription.CurrentPeriodEndUtc)
            //.Select(joined => joined.User.Email!).ToList();



            var now = DateTime.UtcNow;
            var oneWeekFromNow = now.AddDays(7);

            List<string> emailAddresses = await dbContext.AspNetUsers
                .Join(
                    dbContext.Subscriptions,
                    user => user.Id,
                    subscription => subscription.UserId,
                    (user, subscription) => new
                    {
                        User = user,
                        Subscription = subscription
                    })
                .Where(joined =>
                    !string.IsNullOrEmpty(joined.User.Email) &&
                    joined.Subscription.CurrentPeriodEndUtc.HasValue &&
                    joined.Subscription.CurrentPeriodEndUtc.Value >= now &&
                    joined.Subscription.CurrentPeriodEndUtc.Value <= oneWeekFromNow
                )
                .Select(joined => joined.User.Email!)
                .Distinct()
                .ToListAsync();

            foreach (string emailAddress in emailAddresses)
            {
                string message = JsonSerializer.Serialize(new
                {
                    EmailAddress = emailAddress
                });

                await queueClient.SendMessageAsync(message, cancellationToken);
            }




            //_logger.LogInformation(
            //    "Queued {UserCount} user email addresses in {QueueName}.",
            //    emailAddresses.Count,
            //    queueName);


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
}
