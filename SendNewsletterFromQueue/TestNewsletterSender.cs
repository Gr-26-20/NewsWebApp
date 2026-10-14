using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace SendNewsletterFromQueue;

public interface ITestNewsletterSender
{
    Task SendAsync(string email, Func<Task> beforeSubmit, CancellationToken ct);
}

public sealed class TestNewsletterSender(IConfiguration configuration) : ITestNewsletterSender
{
    public const string Subject = "CLICKBAIT News — test newsletter";
    public const string Body = "<h1>Hello from CLICKBAIT News!</h1><p>This is your daily test newsletter. Thanks for signing up!</p>"
        + "<p>To stop receiving emails, choose No newsletters in the Newsletter tab on our website.</p>";
    public async Task SendAsync(string email, Func<Task> beforeSubmit, CancellationToken ct)
    {
        //check configuration values and throw if any are missing or invalid
        string Required(string key) => !string.IsNullOrWhiteSpace(configuration[key])
            ? configuration[key]! : throw new InvalidOperationException($"Missing setting: {key}");
        var host = Required("EmailSettings:SmtpServer");
        if (!int.TryParse(Required("EmailSettings:SmtpPort"), out var port) || port is < 1 or > 65535)
            throw new InvalidOperationException("EmailSettings:SmtpPort must be a valid port.");
        var from = Required("EmailSettings:SenderEmail");
        var username = Required("EmailSettings:SmtpUsername");
        var password = Required("EmailSettings:SmtpPassword");
        // Send the email using MailKit
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(configuration["EmailSettings:SenderName"] ?? "CLICKBAIT News", from));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = Subject;
        message.Body = new TextPart(TextFormat.Html) { Text = Body };
        // Connect to the SMTP server and send the email
        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, ct);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        await client.AuthenticateAsync(username, password, ct);
        await beforeSubmit();
        await client.SendAsync(message, ct);
        try { await client.DisconnectAsync(true, ct); }
        catch { /* The mail server has already accepted the message. */ }
    }
}
