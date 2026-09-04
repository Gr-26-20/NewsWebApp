using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface IArticleService
    {
        Task<List<Articles>> GetAllArticlesAsync();
    }
}
