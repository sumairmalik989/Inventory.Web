using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

public sealed class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCodePage(int code)
    {
        ViewData["StatusCode"] = code;
        return View("StatusCode");
    }
}