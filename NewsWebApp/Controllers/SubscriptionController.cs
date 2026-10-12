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
                return View("Pay", model);
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
        public async Task<IActionResult> MySubscription()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(user.Email);
            
            return View(subscription);
        }

        public async Task<IActionResult> Pay()
        { 
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if(await _subscriptionService.HasActiveSubscriptionAsync(user.Email))
            {
                return RedirectToAction("AlreadySubscribed");
            }
            ViewBag.PublishableKey = _configuration["Stripe:PublishableKey"];
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession()
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

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                UiMode = "hosted_page",
                Mode = "subscription",
                BillingAddressCollection = "auto",
                PhoneNumberCollection = new Stripe.Checkout.SessionPhoneNumberCollectionOptions
                {
                    Enabled = false,
                },
                AutomaticTax = new Stripe.Checkout.SessionAutomaticTaxOptions
                {
                    Enabled = false,
                },
                AllowPromotionCodes = false,
                PaymentMethodCollection = "always",
                SubmitType = "auto",
                CustomerEmail = user.Email,
                SuccessUrl = "https://example.com/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://example.com/cancel",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        Price = _configuration["Stripe:PriceId"],
                        Quantity = 1,
                    },
                },
                Metadata = new Dictionary<string, string>
                {
                    { "integration_identifier", "hosted_mobile_app_0002" },
                    { "origin_context", "mobile_app" },
                },
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }
    }
}
