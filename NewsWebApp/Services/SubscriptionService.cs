using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;
namespace NewsWebApp.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _db;
        private readonly decimal Price = 99.99m;
        private readonly int DurationInDays = 30;

        public SubscriptionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task <bool> HasActiveSubscriptionAsync(string userEmail)
        {
            
            
            return await _db.Subscriptions.
                AnyAsync(s => s.User.Email == userEmail
                && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow);
        }

        public async Task<Subscriptions?> GetActiveSubscriptionAsync(string userEmail)
        {
            
            return await _db.Subscriptions. Include(s => s.User)
                .Where(s => s.User.Email == userEmail 
                    && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<SubscribeResult> SubscriptionAsync(string userEmail, string stripeToken)
        {
            if (await HasActiveSubscriptionAsync(userEmail))
            {
                return SubscribeResult.AlreadySubscribed;
            }
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if(user == null)
            {
                return SubscribeResult.UserNotFound;
            }

            var chargeOptions = new Stripe.ChargeCreateOptions
            {
                Amount = (long)(Price * 100), 
                Currency = "sek",
                Description = "ClickBait News - Irresistible Subscription",
                Source = stripeToken,
            };

            var chargeService = new Stripe.ChargeService();
            try
            {
                var charge = await chargeService.CreateAsync(chargeOptions);
                if (charge.Status != "succeeded")
                {
                    return SubscribeResult.PaymentFailed;
                }

            }
            catch (Stripe.StripeException)
            {
                return SubscribeResult.PaymentFailed;
            }

            _db.Subscriptions.Add(new Subscriptions
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Just an Attractive Subscription",
                Price = Price,
                DurationInDays = DurationInDays,
                BoundingTimeInDays = 0,
                CreatedAt = DateTime.UtcNow,
                User = user
            });

            await _db.SaveChangesAsync();
            return SubscribeResult.Success;
        }
    }

    public enum SubscribeResult
    {
        Success,
        AlreadySubscribed,

        PaymentFailed,
        UserNotFound
    }
}
