using Microsoft.AspNetCore.Mvc;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public new IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("~/Views/Shared/NotFound.cshtml");
        }
    }
}
