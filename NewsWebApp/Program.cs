using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Services;



namespace NewsWebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //added to check if the command is "bootstrap-admin" and if the correct number of arguments is provided
            var bootstrapAdmin = args.Length > 0 &&
                string.Equals(args[0], "bootstrap-admin", StringComparison.OrdinalIgnoreCase);

            if (bootstrapAdmin && args.Length != 2)
            {
                Console.Error.WriteLine("Usage: bootstrap-admin <email>");
                Environment.ExitCode = 1;
                return;
            }

            // The command name and email are not ASP.NET configuration arguments.
            var builder = WebApplication.CreateBuilder(bootstrapAdmin ? Array.Empty<string>() : args);

            Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // Add services
            builder.Services.AddScoped<IUserAndRoleService, UserAndRoleService>();

            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<RoleSeeder>();

            //Article service registration
            builder.Services.AddScoped<IArticleService, ArticleService>();

            //Newsletter service registration
            builder.Services.AddScoped<INewsletterService, NewsletterService>();

            // Email service registration
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient<WeatherService>(); // for making HTTP requests
            builder.Services.AddHttpClient<SmhiWeatherService>();
            builder.Services.AddScoped<TableStorageService>();

            //Add session support
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ISessionHelper, SessionHelper>();

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.ConsentCookieValue = "true";
            });

            builder.Services.AddSession();
            var app = builder.Build();

            if (bootstrapAdmin)
            {
                Environment.ExitCode = await AdminBootstrapper.RunAsync(app.Services, args[1]) ? 0 : 1;
                return;
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            // Seed roles on startup (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
                        await roleSeeder.SeedRolesAsync();
                    }
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error seeding roles during startup");
                }
            });

            app.Run();
        }
    }
}
