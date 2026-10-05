using Microsoft.AspNetCore.Mvc;

namespace ClientJavaScript.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View("Error");
}
