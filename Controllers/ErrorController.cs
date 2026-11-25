
using Microsoft.AspNetCore.Mvc;

namespace Golbet.Controllers;

// This is an MVC error controller, not an API controller.
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    // Handles HTTP status codes such as 404 and 403.
    [Route("Error/{statusCode:int}")]
    public IActionResult Handle(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound");
        }

        return View("General");
    }

    // Handles unhandled exceptions (500).
    [Route("Error/General")]
    public IActionResult General()
    {
        return View("General");
    }
}
