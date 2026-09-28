using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using System.Formats.Tar;


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
        public async Task <IActionResult> Create(Feedback feedback, int Id)
        {
            if (ModelState.IsValid)
            {
                var article = _dbContext.Articles.Find(Id);


                feedback = new Feedback
                {
                    Feedbackstring = feedback.Feedbackstring,
                    CreatedAt = feedback.CreatedAt,
                    UpdatedAt = feedback.UpdatedAt,
                    ArticleId = Id,
                    
                };
                article.Feedback.Add(feedback);

                _dbContext.Feedback.Add(feedback);
                    article.Feedback.Add(feedback);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Articles", "Writer");
            }
            return View();
        }



        public async Task<IActionResult> Edit(int id)
        {
            var feedback = await _dbContext.Feedback.FindAsync(id);
            return View(feedback);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Update(feedback);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Articles", "Writer");
            }
            return View(feedback);
        }

        public async Task<IActionResult> Delete(int id)
        {
            
            if (ModelState.IsValid)
            {
                var feedback = await _dbContext.Feedback.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound();
                }
                _dbContext.Feedback.Remove(feedback);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Details", "Editor");
            }
            return RedirectToAction("Details", "Editor");
        }

        //public async Task<IActionResult> Delete(int id)
        //{

        //    var feedback = await _dbContext.Feedback.FindAsync(id);
        //    if (feedback == null)
        //    {
        //        return NotFound();
        //    }
        //    //var allFeedback = _articleService.DeleteAllFeedbackForArticle(id);
        //    _dbContext.Feedback.Remove(feedback);
        //    await _dbContext.SaveChangesAsync();
        //    return View("Details", "Editor");
        //}

        public async Task<IActionResult> Details(int id)
        {
            var feedback = await _articleService.GetAllFeedbackForArticle(id);
            //var feedback = await _articleService.GetAllFeedback();
            if (ModelState.IsValid)
            {
                return View(feedback);
            }
            return RedirectToAction("Articles", "Writer");
        }


        public async Task<IActionResult> GetAllFeedback()
        {
            var feedback = _articleService.GetAllFeedback();
            if (ModelState.IsValid)
            {
                FeedbackVM feedbackVM = new FeedbackVM
                {
                    feedbackList = await feedback
                    
                };
                return View(feedbackVM);
            }
            return View(feedback);
        }

        public async Task<IActionResult> Approve(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (ModelState.IsValid)
            {
                if(article == null)
                {
                    return NotFound();
                }
                article.articleStatus = Articles.Status.Approved;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Articles", "Writer");
            }
            return RedirectToAction("Articles", "Writer");
        }

        public async Task<IActionResult> Reject(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (ModelState.IsValid)
            {
                if (article == null)
                {
                    return NotFound();
                }
                article.articleStatus = Articles.Status.Rejected;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Articles", "Writer");
            }
            return RedirectToAction("Articles", "Writer");
        }


        public async Task<IActionResult> Archive(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (ModelState.IsValid)
            {
                if (article == null)
                {
                    return NotFound();
                }
                article.articleStatus = Articles.Status.Archived;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Articles", "Writer");
            }
            return RedirectToAction("Articles", "Writer");
        }
    }
}
