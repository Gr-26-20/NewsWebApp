using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using System.Text.Json;

namespace SendNewsletterFromQueue
{
    public sealed class EmailFromQueue
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailFromQueue> _logger;

        private readonly IDbContextFactory<FuncDbContext> _dbContextFactory;

        public EmailFromQueue(
            IConfiguration configuration,
            ILogger<EmailFromQueue> logger,
            IDbContextFactory<FuncDbContext> dbContextFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
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

                //Hardcoded SMTP settings for testing purposes(because the configuration is not working in the Azure Function)
                string smtpHost = "smtp.gmail.com";
                string smtpPort = "465";
                string smtpFrom = "danielturunen1@gmail.com";
                string smtpUser = "danielturunen1@gmail.com";
                string smtpPassword = "covt esen vmcv ngzk";
                string senderName = "Clickbait News";

                //string smtpHost = _configuration["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("The 'EmailSettings:SmtpServer' application setting is not configured.");
                //int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? throw new InvalidOperationException("The 'EmailSettings:SmtpPort' application setting is not configured."));
                //string smtpFrom = _configuration["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("The 'EmailSettings:SenderEmail' application setting is not configured.");
                //string smtpUser = _configuration["EmailSettings:SmtpUsername"] ?? throw new InvalidOperationException("The 'EmailSettings:SmtpUsername' application setting is not configured.");
                //string smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("The 'EmailSettings:SmtpPassword' application setting is not configured.");
                //string senderName = _configuration["EmailSettings:SenderName"] ?? string.Empty;

                //string smtpHost = GetRequiredSetting("EmailSettings:SmtpServer");
                //int smtpPort = GetRequiredIntSetting("EmailSettings:SmtpPort");
                //string smtpFrom = GetRequiredSetting("EmailSettings:SenderEmail");
                //string smtpUser = GetRequiredSetting("EmailSettings:SmtpUsername");
                //string smtpPassword = GetRequiredSetting("EmailSettings:SmtpPassword");
                //string senderName = _configuration["EmailSettings:SenderName"] ?? string.Empty;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, smtpFrom));
                message.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                message.Subject = "CLICKBAIT newsletter";
                var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                List<NewsLetter> newsletters = await dbContext.NewsLetters.ToListAsync();
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = newsletters[newsletters.Count - 1].Body ?? "No newsletter content available."
                };

                SecureSocketOptions socketOptions = int.Parse(smtpPort) == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                using var emailClient = new SmtpClient();

                emailClient.Connect(
                    smtpHost,
                    int.Parse(smtpPort),
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

