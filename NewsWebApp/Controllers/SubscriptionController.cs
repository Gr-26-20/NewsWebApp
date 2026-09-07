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
    //[Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(ISubscriptionService subscriptionService, UserManager<ApplicationUser> userManager)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

                return await _subscriptionService.SubscriptionAsync(user.Email) switch
            {

                SubscribeResult.Success => RedirectToAction("Confirmation"),
                _ => RedirectToAction("Index", "Home")
            };

        }

        public IActionResult Confirmation()
        {
            return View();
        }

        
    }
}
