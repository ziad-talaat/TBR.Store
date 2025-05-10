using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TBL.Core.Contracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;   
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork= unitOfWork;
        }

        [HttpGet]
        public IActionResult Index(string ?searchBy,string?searchValue,string ?sortBy,bool isAssending=true,int pageNumber=1)
        {
            var pageDetails=  _unitOfWork.Products.GetAllSortedAndFilterdInPage(searchBy,searchValue,sortBy,isAssending,pageNumber);
            IEnumerable<Product> products = pageDetails.Items;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ProductWithCategoryNameVM? product = await _unitOfWork.Products.GetProductWithCategoryName(id);
            if (product!= null) 
            {
                return View(product);
            }
            else
            {
                ViewBag.Error = "No Product Details";
                return RedirectToAction(nameof(HomeController.Index));
            }
        }


        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
