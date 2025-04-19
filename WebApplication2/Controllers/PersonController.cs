using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class PersonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
