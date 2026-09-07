using Microsoft.AspNetCore.Mvc;

namespace NewsWebApp.Controllers
{
    public class CategoriesController : Controller
    {
        [Route("Categories/{category?}")]
        public IActionResult Index(string? category)
        {
            ViewBag.Category = (category ?? "Sweden").ToUpper();

            return View();
        }
    }
}
