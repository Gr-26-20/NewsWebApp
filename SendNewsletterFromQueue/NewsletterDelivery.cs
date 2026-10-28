using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;

namespace SendNewsletterFromQueue;

// An email address alone cannot identify which newsletter the recipient requested.
public sealed record NewsletterRequest(string UserId, string EmailAddress, string Category);

public static class NewsletterPreferences
{
    public static readonly string[] Categories = ["News", "World", "Sweden", "Weather", "Sports"];

    public static bool Wants(AspNetUser user, string category) =>
        Categories.Contains(category) && (user.IsSubscribedToNewsletter || (category switch
        {
            "News" => user.IsSubscribedToNewsNewsletter,
            "World" => user.IsSubscribedToWorldNewsletter,
            "Sweden" => user.IsSubscribedToSwedenNewsletter,
            "Weather" => user.IsSubscribedToWeatherNewsletter,
            "Sports" => user.IsSubscribedToSportsNewsletter,
            _ => false
        }));

    public static IEnumerable<NewsletterRequest> BuildRequests(IEnumerable<AspNetUser> users)
    {
        // General subscription means all categories. OR with individual choices prevents duplicate requests.
        foreach (var user in users.Where(u => !string.IsNullOrWhiteSpace(u.Email)))
            foreach (var category in Categories.Where(c => Wants(user, c)))
                yield return new NewsletterRequest(user.Id, user.Email!.Trim(), category);
    }
}

public interface INewsletterEmailSender
{
    Task SendAsync(NewsletterRequest request, NewsLetter newsletter, CancellationToken cancellationToken);
}

public static class NewsletterMessage
{
    public static MimeMessage Create(NewsletterRequest request, NewsLetter newsletter, string senderName, string senderEmail)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(request.EmailAddress));
        message.Subject = string.IsNullOrWhiteSpace(newsletter.Subject)
            ? $"CLICKBAIT {request.Category} newsletter" : newsletter.Subject;
        message.Body = new TextPart(TextFormat.Html) { Text = newsletter.Body };
        return message;
    }
}

public sealed class NewsletterDelivery(INewsletterEmailSender sender)
{
    public async Task<bool> DeliverAsync(FuncDbContext db, NewsletterRequest request, CancellationToken cancellationToken)
    {
        // Recheck consent and email: the user may have unsubscribed since the timer ran.
        var user = await db.AspNetUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null || !NewsletterPreferences.Wants(user, request.Category)
            || !string.Equals(user.Email?.Trim(), request.EmailAddress, StringComparison.OrdinalIgnoreCase))
            return false;

        // NewsLetter has no creation date. Highest identity ID is the newest inserted entry.
        // FirstOrDefault handles empty categories without an out-of-range list index.
        var newsletter = await db.NewsLetters.AsNoTracking()
            .Where(n => n.Category == request.Category)
            .OrderByDescending(n => n.Id).FirstOrDefaultAsync(cancellationToken);
        if (newsletter is null || string.IsNullOrWhiteSpace(newsletter.Body)) return false;

        await sender.SendAsync(request, newsletter, cancellationToken);
        return true;
    }
}
