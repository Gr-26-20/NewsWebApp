BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [CancelAtPeriodEnd] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [CurrentPeriodEndUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [StripeCustomerId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [StripeInvoicePaid] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [StripePriceId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD [StripeStatus] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [StripeCheckoutSessionId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [StripeCustomerId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015092100_AddStripeBilling'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261015092100_AddStripeBilling', N'10.0.11');
END;

COMMIT;
GO

