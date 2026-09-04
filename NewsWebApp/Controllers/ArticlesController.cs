using Microsoft.AspNetCore.Mvc;

namespace NewsWebApp.Controllers
{
    public class ArticlesController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
    }
}
