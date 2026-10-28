using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;
using SendNewsletterFromQueue;
using Xunit;

namespace NewsletterTests;

public sealed class NewsletterTests
{
    [Fact]
    public void GeneralAndCategoryOptIns_CreateOnlyOneRequestPerCategory()
    {
        var user = new AspNetUser { Id = "reader", Email = "reader@example.com",
            IsSubscribedToNewsletter = true, IsSubscribedToNewsNewsletter = true };
        var requests = NewsletterPreferences.BuildRequests([user]).ToList();
        Assert.Equal(5, requests.Count);
        Assert.Equal(5, requests.Select(r => r.Category).Distinct().Count());
    }

    [Fact]
    public void IndividualChoices_QueueOnlyThoseCategories()
    {
        var user = new AspNetUser { Id = "reader", Email = "reader@example.com",
            IsSubscribedToNewsNewsletter = true, IsSubscribedToWeatherNewsletter = true };
        Assert.Equal(new[] { "News", "Weather" },
            NewsletterPreferences.BuildRequests([user]).Select(r => r.Category));
    }

    [Fact]
    public void UnsubscribedOrMissingEmail_QueuesNothing()
    {
        Assert.Empty(NewsletterPreferences.BuildRequests([
            new AspNetUser { Id = "no-consent", Email = "reader@example.com" },
            new AspNetUser { Id = "no-email", IsSubscribedToNewsletter = true },
            new AspNetUser { Id = "blank-email", Email = "  ", IsSubscribedToNewsletter = true }
        ]));
    }

    [Theory] //for each category, the queue handler should send only the newest newsletter in that category.
    [InlineData("News")]
    [InlineData("World")]
    [InlineData("Sweden")]
    [InlineData("Weather")]
    [InlineData("Sports")]
    public async Task QueueHandler_SendsOnlyNewestNewsletterInRequestedCategory(string category)
    {
        using var fixture = new Fixture();
        // There are ten newsletters altogether but only two in each category.
        // This reproduces the data shape that broke the old Weather/Sports list indexing.
        foreach (var name in NewsletterPreferences.Categories)
        {
            fixture.Db.NewsLetters.Add(new NewsLetter { Category = name, Body = $"old {name}" });
            fixture.Db.NewsLetters.Add(new NewsLetter { Category = name, Body = $"new {name}", Subject = $"Latest {name}" });
        }
        await fixture.Db.SaveChangesAsync();

        await fixture.Run(category);

        var message = Assert.Single(fixture.Sender.Messages);
        Assert.Equal($"new {category}", message.HtmlBody);
        Assert.Equal($"Latest {category}", message.Subject);
        Assert.Equal("reader@example.com", Assert.Single(message.To.Mailboxes).Address);
    }

    [Fact]
    public async Task FiveCategories_SendFiveEmailsWithoutDuplicates()
    {
        using var fixture = new Fixture();
        foreach (var category in NewsletterPreferences.Categories)
            fixture.Db.NewsLetters.Add(new NewsLetter { Category = category, Body = category });
        await fixture.Db.SaveChangesAsync();
        foreach (var request in NewsletterPreferences.BuildRequests([fixture.User]))
            await fixture.Handler.RunAsync(JsonSerializer.Serialize(request), default);
        Assert.Equal(5, fixture.Sender.Messages.Count);
        Assert.Equal(5, fixture.Sender.Messages.Select(m => m.HtmlBody).Distinct().Count());
    }

    [Fact]
    public async Task EmptyCategory_IsSkippedWithoutCrashing()
    {
        using var fixture = new Fixture();
        await fixture.Run("Weather");
        Assert.Empty(fixture.Sender.Messages);
    }

    [Fact]
    public async Task UserUnsubscribesAfterQueueing_EmailIsNotSent()
    {
        using var fixture = new Fixture();
        fixture.Db.NewsLetters.Add(new NewsLetter { Category = "News", Body = "News" });
        fixture.User.IsSubscribedToNewsletter = false;
        await fixture.Db.SaveChangesAsync();
        await fixture.Run("News");
        Assert.Empty(fixture.Sender.Messages);
    }

    [Fact]
    public async Task ChangedEmail_IsNotSentToOldAddress()
    {
        using var fixture = new Fixture();
        fixture.Db.NewsLetters.Add(new NewsLetter { Category = "News", Body = "News" });
        fixture.User.Email = "new@example.com";
        await fixture.Db.SaveChangesAsync();
        await fixture.Run("News");
        Assert.Empty(fixture.Sender.Messages);
    }

    [Fact]
    public async Task OldEmailOnlyQueueMessage_IsRejectedWithoutSending()
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.RunAsync(
            "{\"EmailAddress\":\"reader@example.com\"}", default));
        Assert.Empty(fixture.Sender.Messages);
    }

    [Fact]
    public async Task SmtpFailure_IsPropagatedForQueueRetry()
    {
        using var fixture = new Fixture();
        fixture.Db.NewsLetters.Add(new NewsLetter { Category = "News", Body = "News" });
        await fixture.Db.SaveChangesAsync();
        fixture.Sender.Fail = true;
        await Assert.ThrowsAsync<IOException>(() => fixture.Run("News"));
    }

    private sealed class RecordingSender : INewsletterEmailSender
    {
        public List<MimeMessage> Messages { get; } = [];
        public bool Fail { get; set; }
        public Task SendAsync(NewsletterRequest request, NewsLetter newsletter, CancellationToken cancellationToken)
        {
            if (Fail) throw new IOException("Simulated SMTP failure");
            // Build the actual MIME message, but record it instead of opening an SMTP connection.
            Messages.Add(NewsletterMessage.Create(request, newsletter, "Test", "sender@example.com"));
            return Task.CompletedTask;
        }
    }

    private sealed class Fixture : IDisposable, IDbContextFactory<FuncDbContext>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly DbContextOptions<FuncDbContext> options;
        public FuncDbContext Db { get; }
        public AspNetUser User { get; } = new() { Id = "reader", Email = "reader@example.com", IsSubscribedToNewsletter = true };
        public RecordingSender Sender { get; } = new();
        public EmailFromQueue Handler { get; }
        public Fixture()
        {
            connection.Open();
            options = new DbContextOptionsBuilder<FuncDbContext>().UseSqlite(connection).Options;
            Db = CreateDbContext();
            Db.Database.EnsureCreated();
            Db.AspNetUsers.Add(User);
            Db.SaveChanges();
            Handler = new EmailFromQueue(NullLogger<EmailFromQueue>.Instance, this, new NewsletterDelivery(Sender));
        }
        public FuncDbContext CreateDbContext() => new(options);
        public Task Run(string category) => Handler.RunAsync(JsonSerializer.Serialize(
            new NewsletterRequest("reader", "reader@example.com", category)), default);
        public void Dispose()
        {
            foreach (var message in Sender.Messages) message.Dispose();
            Db.Dispose();
            connection.Dispose();
        }
    }
}
