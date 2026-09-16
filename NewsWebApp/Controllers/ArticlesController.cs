using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionService _subscriptionService;

        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISubscriptionService subscriptionService)
        {
            _context = context;
            _userManager = userManager;
            _subscriptionService = subscriptionService;
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
                return View(article);
        }
    }
}