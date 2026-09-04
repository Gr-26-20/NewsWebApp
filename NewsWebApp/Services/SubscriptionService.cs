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

        public async Task <bool> HasActiveSubscriptionAsync(int userId)
        {
            
            
            return await _db.Subscriptions.
                AnyAsync(s => s.UserId.Id == userId 
                && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow);
        }

        public async Task<Subscriptions?> GetActiveSubscriptionAsync(int userId)
        {
            
            return await _db.Subscriptions. Include(s => s.UserId)
                .Where(s => s.UserId.Id == userId
                    && s.CreatedAt.HasValue && s.CreatedAt.Value.AddDays(s.DurationInDays) > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<SubscribeResult> SubscriptionAsync(int userId)
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

                Name = "One and only Attractive Subscription",
                Price = Price,
                DurationInDays = DurationInDays,
                BoundingTimeInDays = 7,
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
