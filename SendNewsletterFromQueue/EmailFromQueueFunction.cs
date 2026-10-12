using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.Json;

namespace SendNewsletterFromQueue
{
    public sealed class EmailFromQueue
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailFromQueue> _logger;

        public EmailFromQueue(
            IConfiguration configuration,
            ILogger<EmailFromQueue> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [Function(nameof(EmailFromQueue))]
        public async Task RunAsync(
            [QueueTrigger("email", Connection = "AzureWebJobsStorage")]
        string queueMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                QueueEmailMessage queuedEmail = ReadQueueMessage(queueMessage);

                if (string.IsNullOrWhiteSpace(queuedEmail.EmailAddress))
                {
                    throw new InvalidOperationException(
                        "The queue message does not contain an email address.");
                }

                string smtpHost = GetRequiredSetting("EmailSettings:SmtpServer");
                int smtpPort = GetRequiredIntSetting("EmailSettings:SmtpPort");
                string smtpFrom = GetRequiredSetting("EmailSettings:SenderEmail");
                string smtpUser = GetRequiredSetting("EmailSettings:SmtpUsername");
                string smtpPassword = GetRequiredSetting("EmailSettings:SmtpPassword");
                string senderName = _configuration["EmailSettings:SenderName"] ?? string.Empty;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, smtpFrom));
                message.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                message.Subject = "Message from FunctionApp3";
                message.Body = new TextPart("plain")
                {
                    Text = "This is a message from the FunctionApp3 application."
                };

                SecureSocketOptions socketOptions = smtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                using var emailClient = new SmtpClient();

                emailClient.Connect(
                    smtpHost,
                    smtpPort,
                    socketOptions,
                    cancellationToken);

                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(
                    smtpUser,
                    smtpPassword.Replace(" ", string.Empty));
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Send(message);
                emailClient.Disconnect(true);

                _logger.LogInformation(
                    "Email sent to {EmailAddress}.",
                    queuedEmail.EmailAddress);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "EmailFromQueue failed while processing a queue message.");

                throw;
            }
        }

        private QueueEmailMessage ReadQueueMessage(string queueMessage)
        {
            try
            {
                QueueEmailMessage? message = JsonSerializer.Deserialize<QueueEmailMessage>(
                    queueMessage,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));

                if (message is not null)
                {
                    return message;
                }
            }
            catch (JsonException)
            {
                // Support messages containing only the email address.
            }

            return new QueueEmailMessage(queueMessage.Trim());
        }

        private string GetRequiredSetting(string settingName)
        {
            return _configuration[settingName]
                ?? throw new InvalidOperationException(
                    $"The '{settingName}' application setting is not configured.");
        }

        private int GetRequiredIntSetting(string settingName)
        {
            string value = GetRequiredSetting(settingName);

            return int.TryParse(value, out int result)
                ? result
                : throw new InvalidOperationException(
                    $"The '{settingName}' application setting must be an integer.");
        }

        private sealed record QueueEmailMessage(string EmailAddress);
    }

}

