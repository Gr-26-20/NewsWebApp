using NewsWebApp.Models;
using System.Collections.Concurrent;

namespace NewsWebApp.Services
{
    public interface IArticleService
    {
        Task<List<Articles>> GetAllArticlesAsync();

        Task<List<Articles>> GetArticlesByCategoryAsync(string category);
        Task<List<Articles>> GetArchivedArticlesAsync();
        Task<List<Articles>> GetEditorsChoiceArticlesAsync();

        Task<List<Articles>> GetApprovedArticlesAsync();

        Task<List<Articles>> GetApprovedArticlesByCategoryAsync(string category);

        Task<List<Articles>> GetArticlesNewAsync();

        Task<List<Articles>> GetArticlesPendingAsync();

        Task<List<Articles>> GetArticlesApprovedAsync();

        Task<List<Articles>> GetArticlesRejectedAsync();

        Task<List<Articles>> GetArticlesArchivedAsync();

        Task<List<Feedback>> GetAllFeedbackForArticle(int id);

        //Task<List<Feedback>> DeleteAllFeedbackForArticle(int id);

        Task<List<Feedback>> GetAllFeedback();

    }
}