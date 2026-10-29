using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsWebApp.Data;
using NewsWebApp.Data.Migrations;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using static System.Net.Mime.MediaTypeNames;

namespace NewsWebApp.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        private readonly ApplicationDbContext _applicationDbContext;

        private readonly IEmailSender _emailSender;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUserAndRoleService _userAndRoleService;

        public NewsletterController(INewsletterService newsletterService, ApplicationDbContext applicationDbContext, IEmailSender emailSender, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IUserAndRoleService userAndRoleService)
        {
            _newsletterService = newsletterService;
            _applicationDbContext = applicationDbContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userAndRoleService = userAndRoleService;
        }
        public async Task<IActionResult> ManageNewsletters()
        {
            var newsletters = await _newsletterService.GetAllNewslettersAsync();
            if (ModelState.IsValid)
            {
                NewsletterVM newsletterVM = new NewsletterVM
                {
                    NewsLetters = newsletters
                };
                return View(newsletterVM);
            }
            return View(newsletters);
        }

        public async Task<IActionResult> EditNewsLetter(int id)
        {
            var newsletter = await _newsletterService.GetNewsletterByIdAsync(id);
            if (newsletter == null)
            {
                return NotFound();
            }
            return View(newsletter);
        }

        [HttpPost]
        public async Task<IActionResult> EditNewsletter(NewsLetter newsLetter)
        {

            if (ModelState.IsValid)
            {
                
                _applicationDbContext.Update(newsLetter);
                await _applicationDbContext.SaveChangesAsync();
                newsLetter.Body = "";
                newsLetter.Body = $"<img style='height:100px; width:200px;' src='{newsLetter.Logo}' alt='logo'/></div></div><h1>{newsLetter.Subject}</h1><p>{newsLetter.Body}</p>";
                if (!string.IsNullOrEmpty(newsLetter.imageUrl))
                {
                    newsLetter.Body += $"<div style='display: flex;'><div><img src='{newsLetter.imageUrl}' alt='Image' /></div>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title))
                {
                    newsLetter.Body += $"<div><div style='padding-left: 2%;'><h2>{newsLetter.Title}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link}' target='_blank'>Read more</a></p></div></div></div></div>";
                }
                if (!string.IsNullOrEmpty(newsLetter.imageUrl2))
                {
                    newsLetter.Body += $"<div style='display:flex;'><div><img src='{newsLetter.imageUrl2}' alt='Image' /></div>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title2))
                {
                    newsLetter.Body += $"<div><div style='padding-left: 2%;'><h2>{newsLetter.Title2}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description2))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description2}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link2))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link2}' target='_blank'>Read more</a></p></div></div></div></div>";
                }




                if (!string.IsNullOrEmpty(newsLetter.imageUrl3))
                {
                    newsLetter.Body += $"<div style='display:flex'><div><img src='{newsLetter.imageUrl3}' alt='Image' /></div>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title3))
                {
                    newsLetter.Body += $"<div><div style='padding-left: 2%;'><h2>{newsLetter.Title3}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description3))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description3}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link3))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link3}' target='_blank'>Read more</a></p></div></div></div></div>";
                }
                //Append the new changes to the body of the message
                //UpdateNewsletterBody(newsLetter);
                //Save changes to db
                 _applicationDbContext.Update(newsLetter);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(ManageNewsletters));
            }
            return View(newsLetter);
        }

        public async Task<IActionResult> DeleteNewsLetter(int id)
        {
            var newsletter = await _newsletterService.GetNewsletterByIdAsync(id);
            if (newsletter == null)
            {
                return NotFound();
            }
            await _newsletterService.DeleteNewsLetter(newsletter);
            return RedirectToAction(nameof(ManageNewsletters));
        }

        public async Task<IActionResult> SendNewsletter(int id)
        {
            var newsletter = await _newsletterService.GetNewsletterByIdAsync(id);
            await _emailSender.SendEmailAsync(newsletter.Email, newsletter.Subject, newsletter.Body);
            return RedirectToAction(nameof(ManageNewsletters));
        }

        //public async Task<IActionResult> Subscribe(string userId) 
        //{

        //    //var currentUser = User;
        //    //var user = await _userManager.GetUserAsync(currentUser);
        //    var user = await _userAndRoleService.GetCurrentUser();
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    if(!user.IsSubscribedToNewsletter)
        //    {
        //        user.IsSubscribedToNewsletter = true;
        //    }
        //    else
        //    {
        //        user.IsSubscribedToNewsletter = false;
        //    }

        //    await _applicationDbContext.SaveChangesAsync();
        //    //return RedirectToAction(nameof(ManageNewsletters));
        //    return Json(new
        //    {
        //        subscribed = user.IsSubscribedToNewsNewsletter
        //    });
        //}

        [HttpPost]
        public async Task<IActionResult>SubscribeToNewsCategory(string userId)
        {
            var user = await _userAndRoleService.GetCurrentUser();
            if(user == null)
            {
                return NotFound();
            }
            if (!user.IsSubscribedToNewsNewsletter)
            {
                user.IsSubscribedToNewsNewsletter = true;
            }
            else
            {
                user.IsSubscribedToNewsNewsletter = false;
            }
            await _applicationDbContext.SaveChangesAsync();
            //return RedirectToAction(nameof(ManageNewsletters));
            return Json(new
            {
                subscribed = user.IsSubscribedToNewsNewsletter
            });
        }
        [HttpPost]
        public async Task<IActionResult> SubscribeToWorldCategory()
        {
            var user = await _userAndRoleService.GetCurrentUser();
            if (user == null)
            {
                return NotFound();
            }
            if (!user.IsSubscribedToWorldNewsletter)
            {
                user.IsSubscribedToWorldNewsletter = true;
            }
            else
            {
                user.IsSubscribedToWorldNewsletter = false;
            }



            await _applicationDbContext.SaveChangesAsync();
            //return RedirectToAction(nameof(ManageNewsletters));
            return Json(new
            {
                success = true,
                subscribed = user.IsSubscribedToWorldNewsletter
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubscribeToSwedenCategory(string userId)
        {
            var user = await _userAndRoleService.GetCurrentUser();
            if (user == null)
            {
                return NotFound();
            }
            if (!user.IsSubscribedToSwedenNewsletter)
            {
                user.IsSubscribedToSwedenNewsletter = true;
            }
            else
            {
                user.IsSubscribedToSwedenNewsletter = false;
            }
            await _applicationDbContext.SaveChangesAsync();
            //return RedirectToAction(nameof(ManageNewsletters));
            return Json(new
            {
                subscribed = user.IsSubscribedToSwedenNewsletter
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubscribeToWeatherCategory(string userId)
        {
            var user = await _userAndRoleService.GetCurrentUser();
            if (user == null)
            {
                return NotFound();
            }
            if (!user.IsSubscribedToWeatherNewsletter)
            {
                user.IsSubscribedToWeatherNewsletter = true;
            }
            else
            {
                user.IsSubscribedToWeatherNewsletter = false;
            }
            await _applicationDbContext.SaveChangesAsync();
            //return RedirectToAction(nameof(ManageNewsletters));
            return Json(new
            {
                subscribed = user.IsSubscribedToWeatherNewsletter
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubscribeToSportsCategory(string userId)
        {
            var user = await _userAndRoleService.GetCurrentUser();
            if (user == null)
            {
                return NotFound();
            }
            if (!user.IsSubscribedToSportsNewsletter)
            {
                user.IsSubscribedToSportsNewsletter = true;
            }
            else
            {
                user.IsSubscribedToSportsNewsletter = false;
            }
            await _applicationDbContext.SaveChangesAsync();
            //return RedirectToAction(nameof(ManageNewsletters));
            return Json(new
            {
                
                subscribed = user.IsSubscribedToSportsNewsletter
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetNewsSubscriptionStatus()
        {
            var user = await _userAndRoleService.GetCurrentUser();

            if (user == null)
                return Unauthorized();

            return Json(new
            {
                subscribed = user.IsSubscribedToNewsNewsletter
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWorldSubscriptionStatus(string category)
        {
            var user = await _userAndRoleService.GetCurrentUser();

            if (user == null)
                return Unauthorized();

            return Json(new
            {
                subscribed = user.IsSubscribedToWorldNewsletter
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSwedenSubscriptionStatus()
        {
            var user = await _userAndRoleService.GetCurrentUser();

            if (user == null)
                return Unauthorized();

            return Json(new
            {
                subscribed = user.IsSubscribedToSwedenNewsletter
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWeatherSubscriptionStatus()
        {
            var user = await _userAndRoleService.GetCurrentUser();

            if (user == null)
                return Unauthorized();

            return Json(new
            {
                subscribed = user.IsSubscribedToWeatherNewsletter
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSportsSubscriptionStatus()
        {
            var user = await _userAndRoleService.GetCurrentUser();

            if (user == null)
                return Unauthorized();

            return Json(new
            {
                subscribed = user.IsSubscribedToSportsNewsletter
            });
        }

    }
}
