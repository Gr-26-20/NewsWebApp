using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class WriterController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IArticleService _articleService;

        public WriterController(ApplicationDbContext context, IArticleService articleService)
        {
            _context = context;
            _articleService = articleService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> Create(Articles article)
        {
            if (ModelState.IsValid)
            {
                article = new Articles
                {
                    Slug = article.Slug,
                    Title = article.Title,
                    Author = article.Author,
                    Summary = article.Summary,
                    ImageUrl = article.ImageUrl,
                    Views = 0,
                    Likes = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsArchived = false,
                    Category = article.Category
                };
                
                _context.Articles.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction("Articles");
            }
            return View(article);
        }

        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> EditArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> EditArticle(int id, Articles article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(article);
                await _context.SaveChangesAsync();
                return RedirectToAction("Articles");
            }
            return View(article);
        }

        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction("Articles");
        }

        [Authorize(Roles = "WRITER")]
        public async Task<IActionResult> Articles()
        {
            var article = _articleService.GetAllArticlesAsync();
            if (ModelState.IsValid)
            {
                ArticlesVM articlesVM = new ArticlesVM
                {
                    Articles = await article
                };
                return View(articlesVM);
            }
            return View(article);
        }
    }
}
