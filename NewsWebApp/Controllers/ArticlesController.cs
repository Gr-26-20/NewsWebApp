using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IArticleService _articleService;

        public ArticlesController(ApplicationDbContext context, IArticleService articleService)
        {
            _context = context;
            _articleService = articleService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        public async Task<IActionResult> Archived()
        {
            var archivedArticles = await _context.Articles
                .Where(a => a.IsArchived)
                .ToListAsync();

            return View(archivedArticles);
        }

        public async Task<IActionResult> EditorsChoice()
        {
            var editorsChoiceArticles = await _articleService.GetEditorsChoiceArticlesAsync();

            return View(editorsChoiceArticles);
        }
    }
}