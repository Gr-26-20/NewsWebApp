
extern alias IdentitySdk;
using Azure.Core;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsWebApp.Data;
using NewsWebApp.Services;
using OpenTelemetry;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(
    Environment.GetEnvironmentVariable(
        "APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddHttpClient();

builder.Services.AddSingleton<Azure.Core.TokenCredential>(services =>
    services.GetRequiredService<IHostEnvironment>().IsDevelopment()
        ? new IdentitySdk::Azure.Identity.DefaultAzureCredential()
        : new IdentitySdk::Azure.Identity.ManagedIdentityCredential());

builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<SmhiWeatherService>();
builder.Services.AddHttpClient<SmhiWeatherWarningService>();
builder.Services.AddHttpClient<WeatherEditorialAiService>();
builder.Services.AddHttpClient<WeatherImageAiService>();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<TableStorageService>();
builder.Services.AddScoped<WeatherArticleGenerationService>();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Build().Run();
