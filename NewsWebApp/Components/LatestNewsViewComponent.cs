using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Services;

namespace NewsWebApp.Components
{
    public class LatestNewsViewComponent : ViewComponent
    {
        private readonly IArticleService _articleService;
        private readonly ApplicationDbContext _dbContext;

        private readonly ISessionHelper _sessionHelper;



        public LatestNewsViewComponent(IArticleService articleService, ApplicationDbContext dbContext, ISessionHelper sessionHelper)

        {

            _articleService = articleService;
            _dbContext = dbContext;
            _sessionHelper = sessionHelper;

        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            if (id != 0)
            {
                int feedbackidTemp = id;
                TempData["FeedbackId"] = feedbackidTemp.ToString();
                TempData.Keep("FeedbackId");
            }
            if (id == 0 && TempData["FeedbackId"] != null)
            {
                int feedbackidTemp = Convert.ToInt32(TempData["FeedbackId"]);
                id = feedbackidTemp;
                TempData.Keep("FeedbackId");
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
            return View();
        }


        private IEnumerable<string> GetLatestNews()
        {
            return new List<string> { "News1", "News2", "News3" };
        }
    }
}
