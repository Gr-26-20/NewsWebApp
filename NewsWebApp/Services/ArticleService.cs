using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;
using System.Collections.Concurrent;

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

        public async Task<List<Feedback>> GetAllFeedbackForArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            var feedbackList = _context.Feedback
                .Where(f => f.ArticleId == id).AsNoTracking()
                .ToListAsync();
            return await feedbackList;
        }

        public async Task<Feedback> GetFeedbackById(int id)
        {
            var feedback = await _context.Feedback.FirstOrDefaultAsync(f => f.Id == id);
            return feedback;
        }

        //public async Task<List<Feedback>> DeleteAllFeedbackForArticle(int id)
        //{
        //    var article = _context.Articles.Find(id);
        //    var allFeedback = _context.Feedback
        //        .Where(f => f.ArticleId == id)
        //        .ToList();
        //    for (int i = 0; i < allFeedback.Count; i++)
        //    {
        //        _context.Feedback.Remove(allFeedback[i]);
        //        await _context.SaveChangesAsync();
        //    }

        //    return await _context.Feedback
        //        .Where(f => f.ArticleId == id)
        //        .ToListAsync();
        //}

        public async Task<List<Feedback>> GetAllFeedback()
        {
            return await _context.Feedback.AsNoTracking().ToListAsync();
        }

    }
}