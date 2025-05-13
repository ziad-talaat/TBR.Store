using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using TBL.Core.Contracts;
using TBL.Core.Converter;
using TBL.Core.Models;
using TBL.Core.ViewModel;
using Microsoft.CodeAnalysis.CSharp;

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
        public async Task<IActionResult> Index(string ?searchBy,string?searchValue,string ?sortBy,string ?value ,bool isAssending=true,int pageNumber=1)
        {
            Pagination<Product> pageDetails=  _unitOfWork.Products.GetAllSortedAndFilterdInPage(searchBy,searchValue,sortBy, value, isAssending,pageNumber);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchValue = searchValue;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentOrder = isAssending;
            ViewBag.CurrentCategory = value;
            ViewBag.SearchItems = new List<SelectListItem>
            {
              new SelectListItem { Value = nameof(Product.Title), Text = "Title" , Selected = (searchBy == nameof(Product.Title))},
              new SelectListItem { Value = nameof(Product.ISBN), Text = "ISBN" ,Selected = (searchBy == nameof(Product.ISBN))},
              new SelectListItem { Value = nameof(Product.Author), Text = "Author",Selected = (searchBy == nameof(Product.Author)) },
              new SelectListItem { Value = nameof(Product.Price), Text = "Price",Selected = (searchBy == nameof(Product.Price)) },
            };

           var categoriesNames=await _unitOfWork.Category.GetCategoriesName();

            ViewBag.FilterCategories = categoriesNames.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (value == x)
            }).ToList();


            
            return View(pageDetails);
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
