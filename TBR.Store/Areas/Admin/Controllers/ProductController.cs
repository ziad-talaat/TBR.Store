using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    [Authorize(Roles = Roles.Role_Admin)]
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
        public async Task<IActionResult> Create(Product product,List<IFormFile> files)
        {
            if (!ModelState.IsValid )
            {
                if (files == null)
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

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images/products/product-" + product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if(!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);
                    
                    using (var fileStream=new FileStream( Path.Combine(finalPath, fileName),FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);    
                    }

                    ProductImages image = new()
                    {
                        ImageIrl = @"/" + productPath + @"/" + fileName,
                        ProductId= product.Id,
                    };
                 
                    try
                    {
                        await _unitOfWork.ProductImages.AddAsync(image);
                        await _unitOfWork.CompleteAsync();
                        TempData["success"] = "product added successfuly";
                       
                    }

                    catch (DbUpdateException ex)
                    {
                        TempData["Error"] = "failed to Add";
                       
                    }
                }
            }
            return RedirectToAction(nameof(ProductController.Index));
        }

              
          


    
         [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Product? product = await _unitOfWork.Products.GetSpecific(x => x.Id == id, true, new[] { nameof(Product.ProductImages) });
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
        public async Task<IActionResult> Edit(Product product ,List<IFormFile> files)
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
                string wwwRootPath = _webHostEnvironment.WebRootPath;


          

            if (files.Count() > 0)
            {
                string productFolder = Path.Combine("Images", "Products", $"product-{product.Id}");
                string finalPath = Path.Combine(wwwRootPath, productFolder);

                if(!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);

                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string fullFilePath = Path.Combine(finalPath, fileName);

                    using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    var image = new ProductImages()
                    {
                        ProductId = product.Id,
                        ImageIrl = "/" + Path.Combine(productFolder, fileName).Replace("\\", "/")
                    };
                   product.ProductImages.Add(image);
                }
                
            }
            try
            {
                _unitOfWork.Products.Update(product);
                await _unitOfWork.CompleteAsync();
                TempData["success"] = "Product updated successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Failed to update product.";
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

        
        public async Task<IActionResult> Deletee(int  id)
        {
            ProductImages? image=await _unitOfWork.ProductImages.GetOneAsync(id);

            if (image == null)
                return View("Error");


            var path = image.ImageIrl.TrimStart('/');
                var oldimagePath = Path.Combine(_webHostEnvironment.WebRootPath, path);

                if (System.IO.File.Exists(oldimagePath))
                {
                    System.IO.File.Delete(oldimagePath);
                }
            
            try
            {

                _unitOfWork.ProductImages.Remove(image);
                await _unitOfWork.CompleteAsync();
                TempData["success"] = "product Deleted successfuly";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to Delete the product.";
            }

            return RedirectToAction(nameof(ProductController.Edit), new {id=image.ProductId});

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
