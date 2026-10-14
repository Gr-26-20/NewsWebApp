BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014095708_AddNewsletterPreference'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastNewsletterAttemptUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014095708_AddNewsletterPreference'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [NewsletterOptIn] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014095708_AddNewsletterPreference'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014095708_AddNewsletterPreference', N'10.0.11');
END;

COMMIT;
GO

