using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;
using NewsWebApp.Services;

namespace NewsWebApp.Components
{
    public class FeedbackViewcomponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        private readonly ArticleService _articleService;
        public FeedbackViewcomponent(ApplicationDbContext context, ArticleService articleService)
        {
            _context = context;
            _articleService = articleService;
        }


        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            var feedback = await _articleService.GetAllFeedbackForArticle(id);
            return View(feedback);
        }
    }
}
