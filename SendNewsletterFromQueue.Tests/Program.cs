using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NewsWebApp.Controllers;
using NewsWebApp.Data;
using SendNewsletterFromQueue;

var passed = 0;
async Task Test(string name, Func<Task> action)
{
    await action(); Console.WriteLine($"PASS: {name}"); passed++;
}
void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
async Task Fails(Func<Task> action)
{
    try { await action(); } catch { return; }
    throw new Exception("Expected failure");
}
var testPath = Path.Combine(Path.GetTempPath(), $"newsletter-{Guid.NewGuid():N}.db");
var factory = new Factory(new DbContextOptionsBuilder<FuncDbContext>().UseSqlite($"Data Source={testPath}").Options);
var clock = new Clock();
var sender = new Sender();
var daily = new DailyNewsletter(factory, sender, clock, NullLogger<DailyNewsletter>.Instance);
async Task Reset()
{
    sender.Reset();
    await using var db = factory.CreateDbContext();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    db.Users.AddRange(
        new NewsletterUser { Id = "yes", Email = "yes@example.test", NewsletterOptIn = true },
        new NewsletterUser { Id = "no", Email = "no@example.test" },
        new NewsletterUser { Id = "empty", Email = "", NewsletterOptIn = true });
    await db.SaveChangesAsync();
}
try
{
    await Test("Only users who chose Yes receive mail", async () =>
    {
        await Reset(); await daily.SendAsync(default);
        Check(sender.Addresses.SequenceEqual(["yes@example.test"]));
    });
    await Test("Repeating the daily run does not send twice", async () =>
    {
        await Reset(); await daily.SendAsync(default); await daily.SendAsync(default);
        Check(sender.Addresses.Count == 1);
    });
    await Test("Users who chose Yes get another email the next day", async () =>
    {
        await Reset(); await daily.SendAsync(default); clock.Now = clock.Now.AddDays(1);
        await daily.SendAsync(default); Check(sender.Addresses.Count == 2);
    });
    await Test("Choosing No stops future daily emails", async () =>
    {
        await Reset(); await daily.SendAsync(default);
        await using var db = factory.CreateDbContext();
        await db.Users.Where(u => u.Id == "yes").ExecuteUpdateAsync(s => s.SetProperty(u => u.NewsletterOptIn, false));
        clock.Now = clock.Now.AddDays(1); await daily.SendAsync(default);
        Check(sender.Addresses.Count == 1);
    });
    await Test("Authentication failure can be safely retried", async () =>
    {
        await Reset(); sender.FailBefore = true; await daily.SendAsync(default);
        await using var db = factory.CreateDbContext();
        Check((await db.Users.SingleAsync(u => u.Id == "yes")).LastNewsletterAttemptUtc is null);
        sender.FailBefore = false; await daily.SendAsync(default); Check(sender.Addresses.Count == 1);
    });
    await Test("An uncertain SMTP result is not retried today", async () =>
    {
        await Reset(); sender.FailDuring = true; await daily.SendAsync(default); await daily.SendAsync(default);
        Check(sender.Addresses.Count == 1);
    });
    await Test("Two overlapping hosts cannot send twice", async () =>
    {
        await Reset(); sender.Pause = true;
        var first = daily.SendAsync(default);
        await sender.Started.Task;
        await daily.SendAsync(default);
        sender.Continue.TrySetResult(); await first;
        Check(sender.Addresses.Count == 1);
    });
    await Test("Failure for one user does not stop other users", async () =>
    {
        await Reset();
        await using var db = factory.CreateDbContext();
        db.Users.Add(new NewsletterUser { Id = "other", Email = "other@example.test", NewsletterOptIn = true });
        await db.SaveChangesAsync();
        sender.FailAddress = "yes@example.test";
        await daily.SendAsync(default); Check(sender.Addresses.SequenceEqual(["other@example.test"]));
    });
    await Test("Cancellation prevents any send", async () =>
    {
        await Reset();
        await Fails(() => daily.SendAsync(new CancellationToken(true)));
        Check(sender.Addresses.Count == 0);
    });
    await Test("SMTP settings are required before submission", async () =>
    {
        var submitted = false;
        await Fails(() => new TestNewsletterSender(new ConfigurationBuilder().Build()).SendAsync("yes@example.test",
            () => { submitted = true; return Task.CompletedTask; }, default));
        Check(!submitted);
    });
    await Test("Test message explains how to unsubscribe", () =>
    {
        Check(TestNewsletterSender.Subject.Contains("test newsletter") && TestNewsletterSender.Body.Contains("No newsletters"));
        return Task.CompletedTask;
    });

    var account = new ApplicationUser { Id = "signed-in", UserName = "reader", NewsletterOptIn = false };
    var userStore = new UserStore(account);
    using var users = new UserManager<ApplicationUser>(userStore, Options.Create(new IdentityOptions()),
        new PasswordHasher<ApplicationUser>(), [], [], new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(),
        null!, NullLogger<UserManager<ApplicationUser>>.Instance);
    var http = new DefaultHttpContext
    {
        User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, account.Id)], "Test"))
    };
    var controller = new NewsletterPreferenceController(users)
    {
        ControllerContext = new ControllerContext { HttpContext = http },
        TempData = new TempDataDictionary(http, new TempProvider())
    };
    await Test("Yes saves the logged-in account's preference", async () =>
    {
        var result = await controller.Set(true);
        Check(account.NewsletterOptIn && userStore.Updates == 1 && result is RedirectToActionResult);
    });
    await Test("No saves the logged-in account's preference", async () =>
    {
        await controller.Set(false); Check(!account.NewsletterOptIn && userStore.Updates == 2);
    });
    await Test("Missing signed-in account cannot update anyone", async () =>
    {
        http.User = new ClaimsPrincipal(new ClaimsIdentity());
        Check(await controller.Set(true) is ChallengeResult && userStore.Updates == 2);
    });
    await Test("Preference endpoint requires login, POST and antiforgery", () =>
    {
        Check(Attribute.IsDefined(typeof(NewsletterPreferenceController), typeof(AuthorizeAttribute)));
        var action = typeof(NewsletterPreferenceController).GetMethod("Set")!;
        Check(Attribute.IsDefined(action, typeof(HttpPostAttribute)) && Attribute.IsDefined(action, typeof(ValidateAntiForgeryTokenAttribute)));
        return Task.CompletedTask;
    });
    Console.WriteLine($"All {passed} tests passed. No real emails or production databases were used.");
}
finally
{
    await using var db = factory.CreateDbContext();
    await db.Database.EnsureDeletedAsync();
}

sealed class Factory(DbContextOptions<FuncDbContext> options) : IDbContextFactory<FuncDbContext>
{
    public FuncDbContext CreateDbContext() => new(options);
}
sealed class Clock : TimeProvider
{
    public DateTimeOffset Now = new(2026, 10, 14, 8, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}
sealed class Sender : ITestNewsletterSender
{
    public List<string> Addresses = [];
    public bool FailBefore, FailDuring, Pause;
    public string? FailAddress;
    public TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Continue = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public void Reset()
    {
        Addresses.Clear(); FailBefore = FailDuring = Pause = false; FailAddress = null;
        Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Continue = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    public async Task SendAsync(string email, Func<Task> beforeSubmit, CancellationToken ct)
    {
        if (FailBefore || email == FailAddress) throw new Exception("Authentication failed");
        await beforeSubmit(); Addresses.Add(email); Started.TrySetResult();
        if (Pause) await Continue.Task;
        if (FailDuring) throw new Exception("SMTP acknowledgement lost");
    }
}
sealed class UserStore(ApplicationUser account) : IUserStore<ApplicationUser>
{
    public int Updates;
    public void Dispose() { }
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id);
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string? name, CancellationToken ct) { user.UserName = name; return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? name, CancellationToken ct) { user.NormalizedUserName = name; return Task.CompletedTask; }
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) { Updates++; return Task.FromResult(IdentityResult.Success); }
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    public Task<ApplicationUser?> FindByIdAsync(string id, CancellationToken ct) => Task.FromResult<ApplicationUser?>(id == account.Id ? account : null);
    public Task<ApplicationUser?> FindByNameAsync(string name, CancellationToken ct) => Task.FromResult<ApplicationUser?>(account);
}
sealed class TempProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
