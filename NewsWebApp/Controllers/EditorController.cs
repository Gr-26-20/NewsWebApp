using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class EditorController : Controller
    {
        private readonly IArticleService _articleService;

        private readonly ApplicationDbContext _dbContext;

        public EditorController(IArticleService articleService, ApplicationDbContext dbContext)
        {
            _articleService = articleService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Feedback feedback, int Id)
        {
            if (ModelState.IsValid)
            {
                var article = await _dbContext.Articles.FindAsync(Id);
                
                if (article == null)
                {
                    return NotFound();
                }
                feedback = new Feedback
                {
                    Feedbackstring = feedback.Feedbackstring,
                    CreatedAt = feedback.CreatedAt,
                    UpdatedAt = feedback.UpdatedAt
                };
                if(article != null)
                {
                    _dbContext.Feedback.Add(feedback);
                    article.Feedback.Add(feedback);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("Articles", "Writer");
                } 
            }
            return View();
        }

        public async Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Feedback feedback)
        {
            _dbContext.Update(feedback);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Details", "Articles");
        }

        public async Task<IActionResult> Delete(Feedback feedback)
        {
            _dbContext.Feedback.Remove(feedback);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Details", "Articles");
        }

        public async Task<IActionResult> Details(Feedback feedback, int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (ModelState.IsValid)
            {
                //Why is this empty?
                var test = article.Feedback;
                return View(test);
            }
            return RedirectToAction("Articles", "Writer");
        }
    }
}
