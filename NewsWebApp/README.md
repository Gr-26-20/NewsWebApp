
### 2. Create `appsettings.Development.json`
Create this file in the project root (same folder as `appsettings.json`).
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
