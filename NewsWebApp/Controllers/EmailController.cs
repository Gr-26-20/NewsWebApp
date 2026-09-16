using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailSender _emailSender;

        private readonly ApplicationDbContext _context;
        public EmailController(IEmailSender emailSender, ApplicationDbContext context)
        {
            _emailSender = emailSender;
            _context = context;
        }
       

        public async Task<IActionResult>SendNewsLetter(string toEmail, string subject, string body) 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendNewsLetter(NewsLetter newsLetter)
        {
            var category = new List<SelectListItem>
            {
            new SelectListItem { Value = "News", Text = "News" },
            new SelectListItem { Value = "World", Text = "World" },
            new SelectListItem { Value = "Sweden", Text = "Sweden" },
            new SelectListItem { Value = "Sports", Text = "Sports" },
            new SelectListItem { Value = "Weather", Text = "Weather" },
            };
            ViewBag.Categories = new SelectList(category, "Value", "Text");
            if (ModelState.IsValid)
            {
                
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
                    newsLetter.Body += $"<p>{newsLetter.Description}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link3))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link3}' target='_blank'>Read more</a></p></div></div></div></div>";
                }
                _context.Add(newsLetter);
                _context.SaveChanges();
                await _emailSender.SendEmailAsync(newsLetter.Email, newsLetter.Subject, newsLetter.Body);
                return RedirectToAction("Index", "Home");
            }
            return View(newsLetter);

        }


       


        [HttpPost]
        public async Task<IActionResult> CreateNewsLetter(NewsLetter newsLetter)
        {
            var category = new List<SelectListItem>
            {
            new SelectListItem { Value = "News", Text = "News" },
            new SelectListItem { Value = "World", Text = "World" },
            new SelectListItem { Value = "Sweden", Text = "Sweden" },
            new SelectListItem { Value = "Sports", Text = "Sports" },
            new SelectListItem { Value = "Weather", Text = "Weather" },
            };
            ViewBag.Categories = new SelectList(category, "Value", "Text");


            if (ModelState.IsValid)
            {
                newsLetter.Body = $"<img style='height:100px; width:200px;' src='{newsLetter.Logo}' alt='logo'/></div></div><h1>{newsLetter.Subject}</h1><p>{newsLetter.Body}</p>";
                if (!string.IsNullOrEmpty(newsLetter.imageUrl))
                {
                    newsLetter.Body += $"<img src='{newsLetter.imageUrl}' alt='Image' />";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title))
                {
                    newsLetter.Body += $"<h2>{newsLetter.Title}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link}' target='_blank'>Read more</a></p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.imageUrl2))
                {
                    newsLetter.Body += $"<img src='{newsLetter.imageUrl}' alt='Image' />";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title2))
                {
                    newsLetter.Body += $"<h2>{newsLetter.Title2}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description2))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description2}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link2))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link2}' target='_blank'>Read more</a></p>";
                }




                if (!string.IsNullOrEmpty(newsLetter.imageUrl3))
                {
                    newsLetter.Body += $"<img src='{newsLetter.imageUrl3}' alt='Image' />";
                }
                if (!string.IsNullOrEmpty(newsLetter.Title3))
                {
                    newsLetter.Body += $"<h2>{newsLetter.Title3}</h2>";
                }
                if (!string.IsNullOrEmpty(newsLetter.Description3))
                {
                    newsLetter.Body += $"<p>{newsLetter.Description}</p>";
                }
                if (!string.IsNullOrEmpty(newsLetter.link3))
                {
                    newsLetter.Body += $"<p><a href='{newsLetter.link3}' target='_blank'>Read more</a></p>";
                }

                newsLetter = new NewsLetter
                {
                    
                    Email = newsLetter.Email,
                    Subject = newsLetter.Subject,
                    Category = newsLetter.Category,
                    Body = newsLetter.Body,
                    imageUrl = newsLetter.imageUrl,
                    Title = newsLetter.Title,
                    Description = newsLetter.Description,
                    link = newsLetter.link,
                    //imageUrl2 = newsLetter.imageUrl2,
                    //Title2 = newsLetter.Title2,
                    //Description2 = newsLetter.Description2,
                    //link2 = newsLetter.link2,
                    //imageUrl3 = newsLetter.imageUrl3,
                    //Title3 = newsLetter.Title3,
                    //Description3 = newsLetter.Description3,
                    //link3 = newsLetter.link3
                };

                _context.NewsLetters.Add(newsLetter);
                await _context.SaveChangesAsync();

                //await _emailSender.SendEmailAsync(newsLetter.Email, newsLetter.Subject, newsLetter.Body);
                return RedirectToAction("Index", "Home");
                //return Ok("Newsletter sent successfully.");
            }
            return View(newsLetter);

        }
    }
}
