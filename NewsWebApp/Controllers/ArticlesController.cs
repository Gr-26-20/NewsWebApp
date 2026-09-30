using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IArticleService _articleService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionService _subscriptionService;

        //public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISubscriptionService subscriptionService, IArticleService articleService)
        //{
        //    _
        //}
        //private readonly UserManager<ApplicationUser> _userManager;
        //private readonly ISubscriptionService _subscriptionService;

        public ArticlesController(
            ApplicationDbContext context,
            IArticleService articleService,
            UserManager<ApplicationUser> userManager,
            ISubscriptionService subscriptionService)
        {
            _context = context;
            _articleService = articleService;
            _userManager = userManager;
            _subscriptionService = subscriptionService;
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
            if (article.IsSubscribedUsers)
            {
                var user = await _userManager.GetUserAsync(User);
                bool hasAccess = user != null &&
                    await _subscriptionService.HasActiveSubscriptionAsync(user.Email);

                if (!hasAccess)
                    return RedirectToAction("Index", "Subscription");
            }
           
            var viewKey = $"viewed_{id}";
            if (HttpContext.Session.GetString(viewKey) == null)
            {
                article.Views++;
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString(viewKey, "true");
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

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            var sessionKey = $"Liked_{id}";
            if(HttpContext.Session.GetString(sessionKey) == null)
            {
                article.Likes++;
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString(sessionKey, "true");
            }
            return Json(new { likes = article.Likes });
        }


        
    }
}