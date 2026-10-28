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



var builder = FunctionsApplication.CreateBuilder(args);


builder.ConfigureFunctionsWebApplication();

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

// Tests replace the SMTP sender with a recorder, so they never send real email.
builder.Services.AddTransient<INewsletterEmailSender, SmtpNewsletterEmailSender>();
builder.Services.AddTransient<NewsletterDelivery>();

builder.Build().Run();
