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


                var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                //List of all newsletters
                List<NewsLetter> newsletters = await dbContext.NewsLetters.ToListAsync();



                //Lists for categorized newsletters
                List < NewsLetter > newsNewsletters = new List<NewsLetter>();
                List < NewsLetter > worldNewsletters = new List<NewsLetter>();
                List < NewsLetter > swedenNewsletters = new List<NewsLetter>();
                List < NewsLetter > weatherNewsletters = new List<NewsLetter>();
                List < NewsLetter > sportsNewsletters = new List<NewsLetter>();



                //Loop to add each newsletter of each category in their separate lists
                foreach (var item in newsletters)
                    {
                    //Adding the different categorized newsletters to their respective
                    //    lists
                    if (item.Category == "News")
                        newsNewsletters.Add(item);
                    else if (item.Category == "World")
                        worldNewsletters.Add(item);
                    else if (item.Category == "Sweden")
                        swedenNewsletters.Add(item);
                    else if (item.Category == "Weather")
                        weatherNewsletters.Add(item);
                    else if (item.Category == "Sports")
                        sportsNewsletters.Add(item);
                }


                //Chunks of messages for each category of newsletter, grabbing the latest newsletter for each category
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, smtpFrom));
                message.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                message.Subject = "CLICKBAIT newsletter";

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = newsletters[newsletters.Count - 1].Body ?? "No newsletter content available."
                };


                var newsMessage = new MimeMessage();
                if (newsNewsletters.Count > 0)
                {

                    newsMessage.From.Add(new MailboxAddress(senderName, smtpFrom));
                    newsMessage.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                    newsMessage.Subject = "CLICKBAIT newsletter";
                    newsMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = newsNewsletters[newsNewsletters.Count - 1].Body ?? "No newsletter content available."
                    };
                }

                var worldMessage = new MimeMessage();
                if (worldNewsletters.Count > 0)
                {
                    worldMessage.From.Add(new MailboxAddress(senderName, smtpFrom));
                    worldMessage.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                    worldMessage.Subject = "CLICKBAIT world newsletter";
                    worldMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = worldNewsletters[worldNewsletters.Count - 1].Body ?? "No newsletter content available."
                    };
                }


                var swedenMessage = new MimeMessage();
                if (swedenNewsletters.Count > 0)
                {

                    swedenMessage.From.Add(new MailboxAddress(senderName, smtpFrom));
                    swedenMessage.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                    swedenMessage.Subject = "CLICKBAIT Sweden newsletter";
                    swedenMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = swedenNewsletters[swedenNewsletters.Count - 1].Body ?? "No newsletter content available."
                    };
                }

                var weatherMessage = new MimeMessage();
                if (weatherNewsletters.Count > 0)
                {

                    weatherMessage.From.Add(new MailboxAddress(senderName, smtpFrom));
                    weatherMessage.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                    weatherMessage.Subject = "CLICKBAIT weather newsletter";
                    weatherMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = weatherNewsletters[weatherNewsletters.Count - 1].Body ?? "No newsletter content available."
                    };
                }


                var sportsMessage = new MimeMessage();
                if (sportsNewsletters.Count > 0)
                {
                    sportsMessage.From.Add(new MailboxAddress(senderName, smtpFrom));
                    sportsMessage.To.Add(MailboxAddress.Parse(queuedEmail.EmailAddress));
                    sportsMessage.Subject = "CLICKBAIT sports newsletter";
                    sportsMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = sportsNewsletters[sportsNewsletters.Count - 1].Body ?? "No newsletter content available."
                    };

                }

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
                await emailClient.SendAsync(message);
                if (newsNewsletters.Count > 0 && queuedEmail.IsSubscribedToNewsNewsletter)
                {
                    await emailClient.SendAsync(newsMessage);
                }
                if (worldNewsletters.Count > 0 && queuedEmail.IsSubscribedToWorldNewsletter)
                {
                    await emailClient.SendAsync(worldMessage);
                }
                if (swedenNewsletters.Count > 0 && queuedEmail.IsSubscribedToSwedenNewsletter)
                {
                    await emailClient.SendAsync(swedenMessage);
                }
                if (weatherNewsletters.Count > 0 && queuedEmail.IsSubscribedToWeatherNewsletter)
                {
                    await emailClient.SendAsync(weatherMessage);
                }

                if (sportsNewsletters.Count > 0 && queuedEmail.IsSubscribedToSportsNewsletter)
                {
                    await emailClient.SendAsync(sportsMessage);
                }

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
                _logger.LogWarning(
                    "Queue message could not be deserialized as a newsletter message.");
            }

            throw new InvalidOperationException(
                "The queue message is not a valid newsletter message.");

            //return new QueueEmailMessage(queueMessage.Trim());
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

        //Extending the class to hold subscription status bools
        private sealed record QueueEmailMessage(
            string EmailAddress,
            bool IsSubscribedToNewsletter,
            bool IsSubscribedToNewsNewsletter,
            bool IsSubscribedToWorldNewsletter,
            bool IsSubscribedToSwedenNewsletter,
            bool IsSubscribedToWeatherNewsletter,
            bool IsSubscribedToSportsNewsletter
            );
    }

}

