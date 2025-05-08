using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork UnitOfWork)
        {
            _unitOfWork = UnitOfWork;   
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products =await  _unitOfWork.Products.GetProductWithProjectionToName();
            return View(products);
        }
    
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Category.GetAllAsync(false);
            var categoriesListItems = categories.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
            });
            ViewBag.Categories = categoriesListItems;
            return View();
        }
           
    
         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
               var categories = await _unitOfWork.Category.GetAllAsync(false);
               var categoriesListItems = categories.Select(x => new SelectListItem
               {
                   Value = x.Id.ToString(),
                   Text = x.Name,
               });
               ViewBag.Categories = categoriesListItems;
                return View(product);
            }
           await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            TempData["success"] = "product added successfuly";

            return RedirectToAction(nameof(ProductController.Index));
        }
    
         [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Product? product = await _unitOfWork.Products.GetOneAsync<int>(id);
            if(product == null)
                return View("Error");
            var categories = await _unitOfWork.Category.GetAllAsync(false);
            var categoriesListItems = categories.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
            });
            ViewBag.Categories = categoriesListItems;

            return View(product);
        }
    
         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _unitOfWork.Category.GetAllAsync(false);
                var categoriesListItems = categories.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                });
                ViewBag.Categories = categoriesListItems;
                return View(product);
            }
            try
            {

                _unitOfWork.Products.Update(product);
                 await _unitOfWork.CompleteAsync();
                TempData["success"] = "product Updated successfuly";
            }
            catch(DbUpdateException ex)
            {
                TempData["Error"] = "Unable to Update the product.";
            }

            return RedirectToAction(nameof(ProductController.Index));
    
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Product? product = await _unitOfWork.Products.GetOneAsync<int>(id);
            if (product == null)
                return View("Error");

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletee(int  id)
        {
            Product? product = await _unitOfWork.Products.GetOneAsync<int>(id);
            if (product == null)
                return View("Error");

            try
            {

                _unitOfWork.Products.Remove(product);
                await _unitOfWork.CompleteAsync();
                TempData["success"] = "product Deleted successfuly";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to Delete the product.";
            }

            return RedirectToAction(nameof(ProductController.Index));

        }



    }
}
