using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            [TimerTrigger("0 0 7  * * 6", RunOnStartup = false)] TimerInfo timer,
            CancellationToken cancellationToken)
        {
            string storageConnectionString = GetRequiredSetting("AzureWebJobsStorage");
            string queueName = _configuration["QueueName"] ?? "";

            var queueClient = new QueueClient(storageConnectionString, queueName,
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
            await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            int latestBirthYearForAdults = DateTime.UtcNow.Year - 18;

            // The context is disposed automatically when this function invocation completes.
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            List<string> emailAddresses = await dbContext.AspNetUsers
                .AsNoTracking()
                .Where(user =>
                    user.IsSubscribedToNewsletter &&
                    user.Email != null &&
                    user.Email != string.Empty)
                .Select(user => user.Email!)
                .ToListAsync(cancellationToken);

            foreach (string emailAddress in emailAddresses)
            {
                string message = JsonSerializer.Serialize(new
                {
                    EmailAddress = emailAddress
                });

                await queueClient.SendMessageAsync(message, cancellationToken);
            }

            _logger.LogInformation(
                "Queued {UserCount} user email addresses in {QueueName}.",
                emailAddresses.Count,
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
