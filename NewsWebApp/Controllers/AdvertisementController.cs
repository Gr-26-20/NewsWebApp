using Microsoft.AspNetCore.Mvc;

namespace NewsWebApp.Controllers;

public class AdvertisementController : Controller
{
    public IActionResult Offer()
    {
        return View();
    }
}