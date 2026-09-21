using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Articles>> GetAllArticlesAsync()
        {
            return await _context.Articles.ToListAsync();
        }

        public async Task<List<Articles>> GetArticlesByCategoryAsync(string category)
        {
            return await _context.Articles
                .Where(a => a.Category == category && !a.IsArchived)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Articles>> GetArchivedArticlesAsync()
        {
            return await _context.Articles
                .Where(a => a.IsArchived)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Articles>> GetEditorsChoiceArticlesAsync()
        {
            return await _context.Articles
                .Where(a => a.EditorChoice)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Articles>> GetApprovedArticlesAsync()
        {
           return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.New)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Articles>> GetApprovedArticlesByCategoryAsync(string category)
        {
            return await _context.Articles
               .AsAsyncEnumerable()
               .Where(a => a.Category == category && !a.IsArchived && a.articleStatus == Articles.Status.Approved)
               .OrderByDescending(a => a.CreatedAt)
               .ToListAsync();
        }

        public async Task<List<Articles>> GetArticlesNewAsync()
        {
            return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.New)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
                
        }

        public async Task<List<Articles>> GetArticlesPendingAsync()
        {
            return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        }

        public async Task<List<Articles>> GetArticlesApprovedAsync()
        {
            return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        }

        public async Task<List<Articles>> GetArticlesRejectedAsync()
        {
            return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        }

        public async Task<List<Articles>> GetArticlesArchivedAsync()
        {
            return await _context.Articles
                .Where(a => a.articleStatus == Articles.Status.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        }

        public List<Feedback> GetAllFeedbackForArticle(Articles article)
        {
            //var article = _context.Articles.FindAsync(id);
            return article.Feedback;
        }
    }
}