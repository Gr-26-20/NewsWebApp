using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Services;
using Microsoft.AspNetCore.Identity;
using NewsWebApp.Data;

namespace NewsWebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly UserManager<ApplicationUser> _users;

        public CategoriesController(IArticleService articleService, UserManager<ApplicationUser> users)
        {
            _articleService = articleService;
            _users = users;
        }

        [Route("Categories/{category?}")]
        public async Task<IActionResult> Index(string? category)
        {
            var selectedCategory = category ?? "Sweden";

            ViewBag.Category = selectedCategory.ToUpper();
            if (string.Equals(selectedCategory, "Newsletter", StringComparison.OrdinalIgnoreCase)
                && User.Identity?.IsAuthenticated == true)
            {
                var user = await _users.GetUserAsync(User);
                ViewBag.NewsletterOptIn = user?.NewsletterOptIn ?? false;
            }

            var articles = await _articleService
                .GetArticlesByCategoryAsync(selectedCategory);

            return View(articles);
        }
    }
}
