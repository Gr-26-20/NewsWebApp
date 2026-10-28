using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace SendNewsletterFromQueue
{
    public class SendNewsletter
    {
        private readonly ILogger<SendNewsletter> _logger;
        private readonly IDbContextFactory<FuncDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;

        public SendNewsletter(
            ILogger<SendNewsletter> logger,
            IDbContextFactory<FuncDbContext> dbContextFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        [Function("SendNewsletter")]
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

            /* My previous approach does not work, as the queue does not take into account
             * duplicate emails. It only adds the email addresses to the queue,
             * meaning, with the previous approach, the same email address would
             * be added each time, per category subscribed to, to the queue.
             * Hence why we have to add all the email addresses; regardless
             * of which category is subscribed to, only once to the queue.
             */
            var subscriberEmails = await dbContext.AspNetUsers
                .AsNoTracking()
                .Where(user => user.Email != null
                && user.Email != string.Empty
                && (user.IsSubscribedToNewsletter
                || user.IsSubscribedToNewsNewsletter
                || user.IsSubscribedToSportsNewsletter
                || user.IsSubscribedToSwedenNewsletter
                || user.IsSubscribedToWeatherNewsletter
                || user.IsSubscribedToWorldNewsletter))
                .Select(user => new{
                    user.Email,
                    user.IsSubscribedToNewsNewsletter,
                    user.IsSubscribedToSportsNewsletter,
                    user.IsSubscribedToSwedenNewsletter,
                    user.IsSubscribedToWeatherNewsletter,
                    user.IsSubscribedToWorldNewsletter
                })
                .ToListAsync(cancellationToken);

            foreach (var subscriber in subscriberEmails)
            {
                var message = JsonSerializer.Serialize(new
                {
                    EmailAddress = subscriber.Email,
                    //IsSubscribedToNewsletter =
                    //    subscriber.IsSubscribedToNewsletter,
                    IsSubscribedToNewsNewsletter =
                        subscriber.IsSubscribedToNewsNewsletter,
                    IsSubscribedToWorldNewsletter =
                        subscriber.IsSubscribedToWorldNewsletter,
                    IsSubscribedToSwedenNewsletter =
                        subscriber.IsSubscribedToSwedenNewsletter,
                    IsSubscribedToWeatherNewsletter =
                        subscriber.IsSubscribedToWeatherNewsletter,
                    IsSubscribedToSportsNewsletter =
                        subscriber.IsSubscribedToSportsNewsletter
                });

                await queueClient.SendMessageAsync(message, cancellationToken);
            }


                _logger.LogInformation(
                "Queued {UserCount} user email addresses in {QueueName}.",
                subscriberEmails.Count,
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
}
