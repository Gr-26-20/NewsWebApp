
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
### To test the subcription use the folloiwng card information
    Scenario	Card Number	            Expiry	CVV
✅ Success	    4242 4242 4242 4242	    12/26	123
❌ Declined  	4000 0000 0000 0002	    12/26	123