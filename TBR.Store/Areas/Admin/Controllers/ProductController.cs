using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using TBL.Core.Contracts;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork UnitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = UnitOfWork;   
            _webHostEnvironment=webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products =await  _unitOfWork.Products.GetProductWithCategoryName();
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
        public async Task<IActionResult> Create(Product product,IFormFile? file)
        {
            if (!ModelState.IsValid || file == null)
            {
                if (file == null)
                    ModelState.AddModelError("ImageURL", "should provide an image to the product");

                var categories = await _unitOfWork.Category.GetAllAsync(false);
                var categoriesListItems = categories.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                });
                ViewBag.Categories = categoriesListItems;
                return View(product);
            }
            string wwwRootPath = _webHostEnvironment.WebRootPath;
           
                string fileName=Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"Images\Products"); 

                using(var fileStream =new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                {
                   await  file.CopyToAsync(fileStream);
                }
            product.ImageURL = @"/Images/Products/" + fileName;
            
            try
            {  
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CompleteAsync();
             TempData["success"] = "product added successfuly";
             return RedirectToAction(nameof(ProductController.Index));
            }
                
            catch(DbUpdateException ex)
            {
                TempData["Error"] = "failed to Add";
                return RedirectToAction(nameof(ProductController.Index));
            }
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
        public async Task<IActionResult> Edit(Product product ,IFormFile? file)
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

            if (file != null)
            {
                if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    var oldimagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageURL.TrimStart('/'));

                    if (System.IO.File.Exists(oldimagePath))
                    {
                        System.IO.File.Delete(oldimagePath);
                    }
                }
    

                string wwwRootPath = _webHostEnvironment.WebRootPath;

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"Images\Products");

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                product.ImageURL = @"/Images/Products/" + fileName;
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
            if (!string.IsNullOrEmpty(product.ImageURL))
            {
                var oldimagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageURL.TrimStart('/'));

                if (System.IO.File.Exists(oldimagePath))
                {
                    System.IO.File.Delete(oldimagePath);
                }
            }
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



        #region ApiCalls
        [HttpGet]
        public async Task< IActionResult> GetAll()
        {
            var productsData = await _unitOfWork.Products.GetProductWithCategoryName();
           return Json(new {data= productsData } );
        }
   

        #endregion

    }
}
