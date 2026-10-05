using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
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

        private readonly ISessionHelper _sessionHelper;

        public EditorController(IArticleService articleService, ApplicationDbContext dbContext, ISessionHelper sessionHelper)
        {
            _articleService = articleService;
            _dbContext = dbContext;
            _sessionHelper = sessionHelper;
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
                _dbContext.Feedback.Add(feedback);
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
        public async Task<IActionResult> Edit(int id,Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.ArticleId = Convert.ToInt32(TempData["ArticleId"]);
                _dbContext.Update(feedback);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Details", "Editor");
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


       

        

        public async Task<IActionResult> Details(int id)
        {
            if (id != 0)
            {
                //Tempdata to store the id of the article if the feedback gets deleted.
                int feedbackidTemp = id;
                TempData["FeedbackId"] = feedbackidTemp.ToString();
                TempData.Keep("FeedbackId");
                //Tempdata to store the article id if the feedback gets modified.
                int articleidTemp = id;
                TempData["ArticleId"] = articleidTemp.ToString();
                TempData.Keep("ArticleId");
            }
            if (id == 0 && TempData["ArticleId"] != null)
            {
                //Tempdata to store the id of the article if the feedback gets deleted.
                int feedbackidTemp = Convert.ToInt32(TempData["FeedbackId"]);
                id = feedbackidTemp;
                TempData.Keep("FeedbackId");
                //Tempdata to store the article id if the feedback gets modified.
                int articleidTemp = id;
                TempData["ArticleId"] = articleidTemp.ToString();
                TempData.Keep("ArticleId");
            }

            var feedback = await _articleService.GetAllFeedbackForArticle(id);
            //var feedbackIds = _sessionHelper.Get<List<int>>(SessionKeys.Feedback) ?? new List<int>();
            //var article = await _dbContext.Articles.FindAsync(id);
            //if (article != null)
            //{
            //    foreach (var feedbackItem in feedback)
            //    {
            //        if (!article.Feedback.Any(f => f.Id == feedbackItem.Id))
            //        {
            //            {
            //                article.Feedback.Add(feedbackItem);
            //            }
            //        }
            //        await _dbContext.SaveChangesAsync();
            //    }


                //var feedbacks = feedbackIds
                //    .Select(id => _articleService.GetFeedbackById(id))
                //    .Where(p => p != null)
                //    .ToList();
                //foreach (var feedbackItem in feedback)
                //{
                //    if (!feedbackIds.Contains(feedbackItem.Id))
                //    {
                //        feedbackIds.Add(feedbackItem.Id);
                //    }
                //}
                //_sessionHelper.Set(SessionKeys.Feedback, feedbackIds);
                //List<Feedback> feedbackList = new List<Feedback>();
                //foreach (var feedbackId in feedbackIds)
                //{
                //    var feedbackItem = await _dbContext.Feedback.FindAsync(feedbackId);
                //    if (feedbackItem != null)
                //    {
                //        feedbackList.Add(feedbackItem);
                //    }
                //}


                if (ModelState.IsValid)
                {
                    //if (article != null)
                    //{
                        ViewBag.FeedbackList = feedback;
                    //}
                    return View(ViewBag.FeedbackList);

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
        [Authorize(Roles = "Editor, Admin")]
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

        [Authorize(Roles = "Editor, Admin")]
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

        [Authorize(Roles = "Editor, Admin")]
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
