using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface ISubscriptionService 
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task<Subscriptions?> GetActiveSubscriptionAsync(string userId);
        Task<SubscribeResult> SubscriptionAsync(string userId);

    }
}
