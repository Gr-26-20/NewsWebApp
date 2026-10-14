# Simple daily test newsletter

Logged-in users choose **Yes** or **No** on `/Categories/Newsletter`. Everyone starts at No. Their choice is saved in `AspNetUsers.NewsletterOptIn`.

`SendDailyTestNewsletter` runs once a day at **08:00 UTC**, reads users with Yes, and sends the same short test email. It does not choose saved newsletters, use an address allowlist, check age, or use a queue. `LastNewsletterAttemptUtc` prevents two hosts or repeat invocations from sending twice on the same UTC day.

## Setup

1. Apply the website's `AddNewsletterPreference` SQL migration before running either app. It adds two columns without changing existing preferences; existing users default to No. The ready-to-run script is `NewsWebApp/Data/Migrations/AddNewsletterPreference.sql` (for a database already at `AddMainImagePosition`).
2. Deploy the website and this Function App against the same SQL database.
3. Configure the Function App settings below. Nested .NET settings use double underscores in Azure:

| Setting | Purpose |
|---|---|
| SqlConnectionString | Connection to the website's SQL database |
| AzureWebJobsStorage | Standard Azure Functions timer storage |
| EmailSettings__SmtpServer | SMTP host (e.g. smtp.gmail.com) |
| EmailSettings__SmtpPort | 465 for implicit TLS, or a STARTTLS port |
| EmailSettings__SenderEmail | Sender email address |
| EmailSettings__SenderName | Optional sender name |
| EmailSettings__SmtpUsername | SMTP login |
| EmailSettings__SmtpPassword | New SMTP/app password |

No QueueName, campaign ID, recipient list or delivery table is needed. The old `CheckAdults` and `EmailFromQueue` functions have been removed, so queued messages are not processed by this version. Replace the old exposed password; restoring the branch does not revoke credentials.

The timer uses UTC: 08:00 UTC is 10:00 in Warsaw during summer time and 09:00 during winter time. No emails are sent when saving Yes/No; the next daily run picks up the choice.

For a live check, apply the migration, deploy, choose Yes for a test account, and verify the next daily email. Choose No to stop future daily sends. Avoid starting a local Function host against production SQL, because it also runs the daily timer.

For an immediate test, run the built Function assembly with `--test-email your-address@example.com`. It checks that address has chosen Yes and sends one test without starting the timer. Supply SQL and SMTP settings through environment variables or the `Values` section of `local.settings.json` in the current folder. A manual test does not change the daily-send timestamp. Success means the SMTP server accepted the message; check the inbox/spam folder to confirm receipt.

Connection/authentication errors are logged and release the daily reservation for a manual retry. If SMTP submission has started, a timeout is ambiguous; that user is not retried again that day, which avoids duplicates but may mean a missed test email. Other opted-in users are still processed. SMTP acceptance is not proof that the message reached the inbox.

## Automated tests (no real email)

```powershell
dotnet run --project SendNewsletterFromQueue.Tests/SendNewsletterFromQueue.Tests.csproj
```

Tests use a temporary SQLite database, a fake email sender and a controllable clock. They exercise actual database filtering and daily reservations plus the website's preference action.
The test runner is kept outside the main solution and uses its own output folder, so a test process cannot block the website build or Update-Database.
