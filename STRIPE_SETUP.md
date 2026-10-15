# Stripe subscriptions — test mode

The app uses product `prod_VQZS4JQb4Hz1TF` and recurring Price `price_1UPiJoG2MKBLu1fVtNjMyxVj`: **99.99 SEK per calendar month**. The price was verified against the Stripe test API. Real payments are deliberately not enabled: checkout validates that the price is test-mode, and live webhook/subscription data is rejected.

## 1. Update the database

Stop the website, select NewsWebApp as the startup project and run in Visual Studio Package Manager Console:

```powershell
Update-Database -Project NewsWebApp -StartupProject NewsWebApp
```

The `AddStripeBilling` migration adds Stripe customer/session IDs to users and billing status/period fields to subscriptions. Existing subscription records are retained and continue to use their original expiry. New Stripe subscriptions use Stripe's actual billing period, not a fixed 30 days. Premium article checks now identify users by their immutable user ID instead of email. An idempotent SQL script is also included at `NewsWebApp/Data/Migrations/AddStripeBilling.sql`.

## 2. Configure keys and the return URL

Configure these in .NET user secrets locally, or App Service environment settings when deployed. Do not commit API/webhook secrets:

| .NET key | App Service setting | Purpose |
|---|---|---|
| Stripe:SecretKey | Stripe__SecretKey | Secret test API key, starting sk_test_ |
| Stripe:PriceId | Stripe__PriceId | price_1UPiJoG2MKBLu1fVtNjMyxVj |
| Stripe:PublicBaseUrl | Stripe__PublicBaseUrl | Actual website origin, e.g. http://localhost:5109 |
| Stripe:WebhookSecret | Stripe__WebhookSecret | whsec_ signing secret from the webhook/listener |

Use the same website origin you open in the browser, including port and HTTPS. The checked-in default is `http://localhost:5109`; if you use the HTTPS launch profile set `https://localhost:7122`. Set the deployed HTTPS website URL in Azure. Hosted Checkout does not need the publishable key or browser-side card-token code.

## 3. Connect Stripe notifications

Create a **test-mode snapshot webhook destination** at:

```text
https://YOUR-WEBSITE/stripe/webhook
```

Select these events:

- checkout.session.completed
- checkout.session.async_payment_succeeded
- customer.subscription.created
- customer.subscription.updated
- customer.subscription.deleted
- invoice.paid
- invoice.payment_failed
- invoice.payment_action_required

Use API version **2026-09-30.endive**, pinned by the installed Stripe.net 53 SDK. The handler verifies the raw body and Stripe-Signature header. Copy the endpoint's signing secret into `Stripe:WebhookSecret`. API keys and signing secrets are different values.

For localhost, use the Stripe CLI in a separate terminal:

```powershell
stripe login
stripe listen --forward-to http://localhost:5109/stripe/webhook
```

Use the **listener's** signing secret for local testing, not the deployed endpoint's secret. Keep the listener running while testing. Official guide: https://docs.stripe.com/webhooks

## 4. Enable the customer portal

In Stripe test-mode billing portal settings, save a configuration that allows customers to update payment methods and cancel subscriptions **at the end of the billing period**. Disable switching products/plans for now: this app supports one fixed Price. The My Subscription page opens this portal for the currently logged-in Stripe customer.

## 5. Test checkout

1. Sign in with an account without active premium access.
2. Open Subscribe and click **Subscribe with Stripe**.
3. Use Stripe's test card `4242 4242 4242 4242`, any future expiry and any three-digit CVC. Never enter a real card for testing.
4. Complete checkout. The app verifies the returned session belongs to that account and retrieves current payment/subscription state from Stripe.
5. Confirm My Subscription shows active access and the actual renewal date.
6. Open Manage billing, cancel at period end, and check that the account retains paid access until the displayed date.
7. Test renewal success/failure with Stripe's subscription test tools. Failed/past-due subscriptions lose premium access; a later paid/active update restores it.

Webhook updates also work when users close the browser before returning. Visiting the confirmation URL by itself never grants access. Stripe redelivery is safe: membership rows are keyed by Stripe subscription ID and synchronization fetches current Stripe state. Repeated checkout clicks reuse an open session. Accounts with an existing active/incomplete/past-due subscription go to the portal rather than purchasing another.

Legacy pre-Checkout subscriptions stay usable until their stored expiry but do not automatically renew or receive a portal customer. They have no associated Stripe recurring subscription.

## Automated checks

```powershell
dotnet run --project StripeBilling.Tests/StripeBilling.Tests.csproj
```

The standalone checks use SQLite and fake Stripe objects. They cover session reuse, payment verification, account ownership, subscription lifecycle, webhook signatures and replayed events. They do not change production SQL or make real payments. SQL Server uses update/range locks to serialize competing checkout and membership updates; provider-specific locking and a complete browser checkout still need verification in your running environment.
