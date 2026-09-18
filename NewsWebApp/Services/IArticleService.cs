using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface IArticleService
    {
        Task<List<Articles>> GetAllArticlesAsync();

        Task<List<Articles>> GetArticlesByCategoryAsync(string category);
        Task<List<Articles>> GetArchivedArticlesAsync();
        Task<List<Articles>> GetEditorsChoiceArticlesAsync();
    }
}