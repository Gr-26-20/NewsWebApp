using Azure.Monitor.OpenTelemetry.Exporter;
using SendNewsletterFromQueue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using Microsoft.Extensions.Configuration;



// A manual test sends to one opted-in account without starting the daily timer.
if (args.Length == 2 && args[0] == "--test-email")
{
    var localSettings = new ConfigurationBuilder()
        .AddJsonFile("local.settings.json", optional: true).Build();
    var testConfiguration = new ConfigurationBuilder()
        .AddInMemoryCollection(localSettings.GetSection("Values").GetChildren()
            .Select(setting => new KeyValuePair<string, string?>(setting.Key, setting.Value)))
        .AddEnvironmentVariables().Build();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    try
    {
        var options = new DbContextOptionsBuilder<FuncDbContext>().UseSqlServer(
            testConfiguration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SqlConnectionString is required for the manual test.")).Options;
        await using var db = new FuncDbContext(options);
        var optedIn = await db.Users.AnyAsync(u => u.Email == args[1] && u.NewsletterOptIn, timeout.Token);
        if (!optedIn)
            throw new InvalidOperationException("That email address has not chosen Yes for newsletters.");
        await new TestNewsletterSender(testConfiguration).SendAsync(args[1], () => Task.CompletedTask, timeout.Token);
        Console.WriteLine($"Test newsletter accepted by the mail server for {args[1]}.");
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Test newsletter failed: {exception.Message}");
        Environment.ExitCode = 1;
    }
    return;
}

var builder = FunctionsApplication.CreateBuilder(args);


builder.ConfigureFunctionsWebApplication();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ITestNewsletterSender, TestNewsletterSender>();
builder.Services.AddScoped<DailyNewsletter>();

builder.Services.AddDbContextFactory<FuncDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SqlConnectionString"]
        ?? throw new InvalidOperationException(
            "The 'SqlConnectionString' application setting is not configured.")));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
