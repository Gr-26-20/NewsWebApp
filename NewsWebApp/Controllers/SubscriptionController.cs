using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace NewsWebApp.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task<IActionResult> Index()
        {
           if( await _subscriptionService.HasActiveSubscriptionAsync(GetCurrentUserId()))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(new SubscribeViewModel());
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(SubscribeViewModel model)
        {
            if (!ModelState.IsValid) {
                return View("Index", model);
            }

            return await _subscriptionService.SubscriptionAsync(GetCurrentUserId()) switch
            {

                SubscribeResult.Success => RedirectToAction("Confirmation"),
                _ => RedirectToAction("Index", "Home")
            };

        }

        public IActionResult Confirmation()
        {
            return View();
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
