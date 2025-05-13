using Microsoft.AspNetCore.Mvc;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
