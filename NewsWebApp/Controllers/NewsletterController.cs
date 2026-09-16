using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        private readonly ApplicationDbContext _applicationDbContext;

        private readonly IEmailSender _emailSender;
        public NewsletterController(INewsletterService newsletterService, ApplicationDbContext applicationDbContext, IEmailSender emailSender)
        {
            _newsletterService = newsletterService;
            _applicationDbContext = applicationDbContext;
            _emailSender = emailSender;
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
        public async Task<IActionResult> EditNewsletter(NewsLetter newsletter)
        {
            //    if (ModelState.IsValid)
            //    {
            //        _applicationDbContext.Update(newsletter);
            //        await _applicationDbContext.SaveChanges();
            //        return RedirectToAction(nameof(ManageNewsletters));
            //    }
            //    return View(newsletter);

           

            if (ModelState.IsValid)
            {
                _applicationDbContext.Update(newsletter);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(ManageNewsletters));
            }
            return View(newsletter);


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
    }
}
