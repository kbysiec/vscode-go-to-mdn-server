using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class DataController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}