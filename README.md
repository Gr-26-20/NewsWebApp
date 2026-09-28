# NewsWebApp

NewsWebApp is an ASP.NET Core 10 web application for managing and distributing news articles. It supports Admin, Writer, and Subscriber roles, subscriptions, weather data, electricity prices, and newsletters. It uses Azure Blob Storage for images and Stripe for payments.

## Technologies

- ASP.NET Core 10
- Entity Framework Core 10 with SQL Server
- ASP.NET Identity (Authentication & Authorization)
- Azure Blob Storage
- Stripe Payment Processing
- MailKit & MimeKit (Email)
- SMHI Weather API Integration


## Directory structure

```text
NewsWebApp/ 
├── Areas/                            # ASP.NET Identity UI (Auto-generated) 
│   └── Identity/Pages/Account/      # Login, Register, Password Recovery, 2FA, Profile Management  
│       ├── Login.cshtml.cs   
│       ├── Register.cshtml.cs   
│       ├── Manage/                   # User account management pages  
│       └── ...  
│ 
├── Controllers/                     # MVC Controllers - Business Logic & HTTP Handling  
│   ├── HomeController.cs            # Home page, weather integration  
│   ├── WriterController.cs          # Article creation/editing (Writer role) 
│   ├── ArticlesController.cs        # Article details page  
│   ├── CategoriesController.cs      # Category management  
│   ├── SubscriptionController.cs    # Subscription & payment flow  
│   ├── NewsletterController.cs      # Newsletter management 
│   ├── FileController.cs            #  File upload to blob storage 
│   ├── ForecastController.cs        # Weather forecast 
│   ├── EmailController.cs           # Email operations   
│   └── ContactController.cs         # Contact form 
│ 
├── Models/                          # Data Models & ViewModels 
│   ├── Articles.cs                  # Article entity model 
│   ├── Category.cs                  # Article categories  
│   ├── Subscriptions.cs             # User subscription data 
│   ├── NewsLetter.cs                # Newsletter data 
│   ├── Weather.cs                   # Weather entity 
│   ├── Users.cs                     # Custom user properties 
│   ├── ContactViewModel.cs          # Contact form model 
│   ├── ErrorViewModel.cs            # Error page model  
│   ├── FileUploadModel.cs           # File upload model 
│   └── ViewModels/                  # Display & input models 
│       ├── HomeViewModel.cs         # Home page data  
│       ├── ArticlesVM.cs            # Articles list  
│       ├── CreateArticleViewModel.cs  # Article creation form  
│       ├── SubscribeViewModel.cs    # Subscription form 
│       ├── NewsletterVM.cs          # Newsletter data 
│       ├── WeatherForecast.cs       # Weather display model 
│       └── ...  
│ 
├── Services/                        # Business Logic & External Integrations 
│   ├── IArticleService.cs           # Interface 
│   ├── ArticleService.cs            # Article CRUD operations 
│   ├── IFileService.cs              # Interface 
│   ├── FileService.cs               # Azure Blob Storage operations 
│   ├── ISubscriptionService.cs      # Interface 
│   ├── SubscriptionService.cs       # Subscription management 
│   ├── INewsletterService.cs        # Interface 
│   ├── NewsletterService.cs         # Newsletter sending 
│   ├── IUserAndRoleService.cs       # Interface   
│   ├── UserAndRoleService.cs        # User & role management 
│   ├── WeatherService.cs            # Weather API integration 
│   ├── SmhiWeatherService.cs        # SMHI specific weather data  
│   ├── EmailSender.cs               # Email sending (MailKit) 
│   ├── IEmailService.cs             # Interface 
│   └── RoleSeeder.cs                # Initialize default roles (Admin, Writer)  
| 
├── Data/                            # Entity Framework & Database
│   ├── ApplicationDbContext.cs      # EF Core DbContext
│   ├── ApplicationUser.cs           # Identity user extended properties
│   └── Migrations/                  # Database migration history
│       ├── [timestamp]_CreateIdentitySchema.cs  
│       ├── [timestamp]_AddArticleContent.cs 
│       ├── [timestamp]_Newsletter.cs 
│       ├── [timestamp]_AddedLogoToNewsletter.cs  
│       ├── [timestamp]_PriceTypeFixAndIsSubcribedUsersInArticle.cs 
│       └── ... 
│ 
├── Views/                          # Razor Views (UI Templates) 
│   ├── Writer/ 
│   │   ├── Create.cshtml           # Article creation form
│   │   ├── EditArticle.cshtml      # Article editing 
│   │   ├── Articles.cshtml         # Writer's articles list
│   │   ├── ViewArticle.cshtml      # Article detail view  
│   │   └── Index.cshtml            # Writer dashboard
│   ├── Articles/ 
│   │   └── Details.cshtml          # Article detail page 
│   ├── Categories/ 
│   │   └── Index.cshtml            # Category listing
│   ├── Subscription/
│   │   ├── Index.cshtml            # Subscription plans
│   │   ├── Confirmation.cshtml     # Payment success
│   │   ├── PaymentFailed.cshtml    # Payment failure
│   │   └── AlreadySubscribed.cshtml # Already subscribed message
│   ├── Home/
│   │   ├── Index.cshtml            # Home page
│   │   └── Privacy.cshtml          # Privacy policy
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Master layout
│   │   ├── _Layout.cshtml.css      # Layout styling
│   │   ├── _LoginPartial.cshtml    # Login/logout UI
│   │   ├── _CookieConsentPartial.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml            # Error page
│   └── Contact/
│       └── Index.cshtml.cs         # Contact form
│
├── Pages/                          # Razor Pages (Alternative to MVC views)
│   ├── Profile.cshtml              # User profile page
│   ├── Admin/
│   │   └── Users.cshtml            # Admin users management
│   └── _ViewStart.cshtml           # Razor page startup
│
├── wwwroot/                        # Static Assets
│   └── images/                     # Image storage directory (using Azure Blob)
```

## Key features

### Article management

-  Create, read, update, delete articles
-  Category filtering
-  Archive functionality
-  Editor's choice highlights
-  Subscriber-only content
-  Image uploads to Azure Blob Storage
-  Like/view counters

### Authentication and authorization

-  Email/password registration & login
-  Email confirmation
-  Password reset
-  Two-factor authentication (2FA)
-  Role-based access control
-  Lockout protection

### Subscriptions and payments

-  Stripe integration
-  Multiple subscription tiers
-  Subscription status tracking
-  Payment webhook handling
-  Subscriber-only articles

### Weather integration

-  Real-time weather data
- Multiple municipalities (Sweden)
-  Weather forecasts
-  SMHI API integration
-  Caching for performance

### Newsletter

-  Newsletter creation & sending
-  Email distribution
-  Newsletter branding (logo)
-  Subscriber tracking

## Development configuration

Create `appsettings.Development.json` in the application project directory, next to `appsettings.json`.
Ask Angelin for the actual key values.

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_keyvaluexxxxxxxxxxxx",
    "SecretKey": "sk_test_keyvaluexxxxxxxxxxxxxxxx"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
## Test subscription payments

Use these card details to test subscription payments:

| Scenario | Card number | Expiry | CVV |
| --- | --- | --- | --- |
| ✅ Successful payment | `4242 4242 4242 4242` | `12/26` | `123` |
| ❌ Declined payment | `4000 0000 0000 0002` | `12/26` | `123` |
