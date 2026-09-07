using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IArticleService _articleService;

        public CategoriesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [Route("Categories/{category?}")]
        public async Task<IActionResult> Index(string? category)
        {
            var selectedCategory = category ?? "Sweden";

            ViewBag.Category = selectedCategory.ToUpper();

            var articles = await _articleService
                .GetArticlesByCategoryAsync(selectedCategory);

            return View(articles);
        }
    }
}