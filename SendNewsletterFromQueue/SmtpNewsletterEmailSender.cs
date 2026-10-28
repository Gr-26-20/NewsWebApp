using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace SendNewsletterFromQueue;

public sealed class SmtpNewsletterEmailSender(IConfiguration configuration) : INewsletterEmailSender
{
    public async Task SendAsync(NewsletterRequest request, NewsLetter newsletter, CancellationToken cancellationToken)
    {
        //  Azure configuration instead of source code.
        string Required(string key) => !string.IsNullOrWhiteSpace(configuration[key])
            ? configuration[key]! : throw new InvalidOperationException($"Missing setting: {key}");

        var port = int.Parse(Required("EmailSettings:SmtpPort"));

        using var message = NewsletterMessage.Create(request, newsletter,
            configuration["EmailSettings:SenderName"]
            ?? "CLICKBAIT News", Required("EmailSettings:SenderEmail"));

        using var client = new SmtpClient();

        await client.ConnectAsync(Required("EmailSettings:SmtpServer"), port,
            port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, cancellationToken);

        client.AuthenticationMechanisms.Remove("XOAUTH2");

        await client.AuthenticateAsync(Required("EmailSettings:SmtpUsername"),
            Required("EmailSettings:SmtpPassword").Replace(" ", ""), cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}
