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
    }
}