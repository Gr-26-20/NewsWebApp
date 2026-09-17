using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models.ViewModels;

namespace NewsWebApp.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.Message = "Thank you. Your message has been received.";

            return View();
        }
    }
}