using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private readonly IFileService _fileService;

        public WriterController(ApplicationDbContext context, IArticleService articleService, IFileService fileService)
        {
            _context = context;
            _articleService = articleService;
            _fileService = fileService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upload()
        {
            
            return View();
        }
       

        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create(Articles article, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = article.ImageUrl;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    imageUrl = await _fileService.UploadImageAsync(ImageFile);
                }

                article = new Articles
                {
                    Slug = article.Slug,
                    Title = article.Title,
                    Author = article.Author,
                    Summary = article.Summary,
                    Content = article.Content,
                    ImageUrl = imageUrl,
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

        [Authorize(Roles = "Writer")]
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
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> EditArticle(int id, Articles article, IFormFile? ImageFile)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    article.ImageUrl = await _fileService.UploadImageAsync(ImageFile);
                }

                _context.Update(article);
                await _context.SaveChangesAsync();
                return RedirectToAction("Articles");
            }
            return View(article);
        }

        //[Authorize(Roles = "WRITER")]
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

        //[Authorize(Roles = "WRITER")]
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

        public async Task<IActionResult> Search(string searchString)
        {
            var articles = await _context.Articles
                .Where(a => a.Title.Contains(searchString) || a.Title.ToUpper().Contains(searchString) || a.Summary.Contains(searchString) || a.Summary.ToUpper().Contains(searchString) || a.Category.Contains(searchString) || a.Category.ToUpper().Contains(searchString))
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByCategory(string category)
        {
            var articles = await _context.Articles
                .Where(a => a.Category == category)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByDate(DateTime startDate, DateTime endDate)
        {
            var articles = await _context.Articles
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByViews(int minViews, int maxViews)
        {
            var articles = await _context.Articles
                .Where(a => a.Views >= minViews && a.Views <= maxViews)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByLikes(int minLikes, int maxLikes)
        {
            var articles = await _context.Articles
                .Where(a => a.Likes >= minLikes && a.Likes <= maxLikes)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public Task<IActionResult> FilterByAuthor(string author)
        {
            var articles = _context.Articles
                .Where(a => a.Author == author)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles.Result
            };
            return Task.FromResult<IActionResult>(View("Index", articlesVM));
        }

        public Task<IActionResult> FilterBySlug(string slug)
        {
            var articles = _context.Articles
                .Where(a => a.Slug == slug)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles.Result
            };
            return Task.FromResult<IActionResult>(View("Index", articlesVM));
        }

        public async Task<IActionResult> FilterByArchived(bool isArchived)
        {
            var articles = await _context.Articles
                .Where(a => a.IsArchived == isArchived)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByUpdatedAt(DateTime startDate, DateTime endDate)
        {
            var articles = await _context.Articles
                .Where(a => a.UpdatedAt >= startDate && a.UpdatedAt <= endDate)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> FilterByCreatedAt(DateTime startDate, DateTime endDate)
        {
            var articles = await _context.Articles
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .ToListAsync();
            ArticlesVM articlesVM = new ArticlesVM
            {
                Articles = articles
            };
            return View("Index", articlesVM);
        }

        public async Task<IActionResult> ViewArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }
    }
}
