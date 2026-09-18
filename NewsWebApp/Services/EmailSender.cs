using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using System.Security.Authentication;

namespace NewsWebApp.Services
{
    public class EmailSender : IEmailSender
    {
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string email, string subject, string content)
    {
        string response = "";
        var message = new MimeMessage();
        message.Sender = MailboxAddress.Parse(_configuration["EmailSettings:SenderEmail"]!); //Who is sending
        message.Sender.Name = _configuration["EmailSettings:SenderName"];
        message.To.Add(MailboxAddress.Parse(email));
        message.From.Add(message.Sender);
        message.Subject = subject; //We will say we are sending HTML. But there are options for plaintext etc.
                                   //message.Body = new TextPart(TextFormat.Html) { Text = content }; 

        //We will say we are sending HTML. But there are options for plaintext etc.
        message.Body = new TextPart(TextFormat.Html) { Text = content };
        //Be careful that the SmtpClient class is the one from Mailkit not the framework! 
        using (var emailClient = new SmtpClient())
        {
            try
            {
                //The last parameter here is to use SSL (Which you should!) 
                emailClient.Connect(_configuration["EmailSettings:SmtpServer"]!, Convert.ToInt32(_configuration["EmailSettings:SmtpPort"]), true);
            }
            catch (SmtpCommandException ex)
            {
                response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
            }
            catch (SmtpProtocolException ex)
            {
                response = "Protocol error while trying to connect:" + ex.Message;
                return Task.FromResult(response);
            }
            catch(AuthenticationException ex)
                {
                    response = "Authentication error while trying to connect:" + ex.Message;
                    return Task.FromResult(response);
                }
                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
            emailClient.Authenticate(_configuration["EmailSettings:SmtpUsername"]!, _configuration["EmailSettings:SmtpPassword"]!); //Need to authenticate with my username and password, before i can send emails.

            try
            {
                emailClient.Send(message);
            }
            catch (SmtpCommandException ex)
            {
                response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;
                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        response += " Recipient not accepted: " + ex.Mailbox;
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        response += " Sender not accepted: " + ex.Mailbox;
                        Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        response += " Message not accepted.";
                        break;
                }
            }
            emailClient.Disconnect(true);
        }
        return Task.CompletedTask;
    }
}
}