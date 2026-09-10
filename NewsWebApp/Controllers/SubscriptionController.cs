using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NewsWebApp.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public SubscriptionController(ISubscriptionService subscriptionService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (await _subscriptionService.HasActiveSubscriptionAsync(user.Email))
            {
                return RedirectToAction("AlreadySubscribed");
            }
            ViewBag.PublishableKey = _configuration["Stripe:PublishableKey"];
            return View(new SubscribeViewModel());


        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(SubscribeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PublishableKey = _configuration["Stripe:PublishableKey"];
                return View("Index", model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return await _subscriptionService.SubscriptionAsync(user.Email, model.StripeToken ?? "") switch
            {

                SubscribeResult.Success => RedirectToAction("Confirmation"),
                SubscribeResult.AlreadySubscribed => RedirectToAction("AlreadySubscribed"),
                SubscribeResult.PaymentFailed => RedirectToAction("PaymentFailed"),
                _ => RedirectToAction("Index", "Home")
            };

        }

        public IActionResult Confirmation()
        {
            return View();
        }

        public IActionResult AlreadySubscribed()
        {
            return View();
        }
        public IActionResult PaymentFailed()
        {
            return View();


        }
    }
}
