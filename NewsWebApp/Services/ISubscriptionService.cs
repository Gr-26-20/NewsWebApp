using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface ISubscriptionService 
    {
        Task<bool> HasActiveSubscriptionAsync(int userId);
        Task<Subscriptions?> GetActiveSubscriptionAsync(int userId);
        Task<SubscribeResult> SubscriptionAsync(int userId);

    }
}
