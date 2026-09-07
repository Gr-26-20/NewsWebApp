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
                AnyAsync(s => s.UserId.Email == userEmail
                && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow);
        }

        public async Task<Subscriptions?> GetActiveSubscriptionAsync(string userEmail)
        {
            
            return await _db.Subscriptions. Include(s => s.UserId)
                .Where(s => s.UserId.Email == userEmail 
                    && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<SubscribeResult> SubscriptionAsync(string userId)
        {
            if (await HasActiveSubscriptionAsync(userId))
            {
                return SubscribeResult.AlreadySubscribed;
            }
            var user = await _db.Users.FindAsync(userId);

            if(user == null)
            {
                return SubscribeResult.UserNotFound;
            }

            _db.Subscriptions.Add(new Subscriptions
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Just an Attractive Subscription",
                Price = Price,
                DurationInDays = DurationInDays,
                BoundingTimeInDays = 0,
                CreatedAt = DateTime.UtcNow,
                UserId = user
            });

            
            await _db.SaveChangesAsync();
            return SubscribeResult.Success;
        }
    }

    public enum SubscribeResult
    {
        Success,
        AlreadySubscribed,
        UserNotFound
    }
}
