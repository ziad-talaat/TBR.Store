using Application.EF.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TBL.Core.Contracts;
using TBL.Core.Converter;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    public class CategoryController : Controller
    {

        private readonly IUnitOfWork _UnitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> objCategoryList = await _UnitOfWork.Category.GetAllAsync(false);
            return View(objCategoryList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                //await   _context.Category.AddAsync(obj);
                //await   _context.SaveChangesAsync();

                await _UnitOfWork.Category.AddAsync(obj);
                await _UnitOfWork.CompleteAsync();
                TempData["success"] = "Category Created Successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || id == 0)
                return NotFound("No such Category");
            Category? category = await _UnitOfWork.Category.GetOneAsync(id);
            if (category != null)
                return View(category);

            return NotFound("No such Category");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Category? category = await _UnitOfWork.Category.GetOneAsync(obj.Id);
                    if (category != null)
                    {
                        category.ConvertToCategoryDTO(obj);

                        _UnitOfWork.Category.Update(category);
                        await _UnitOfWork.CompleteAsync();
                        TempData["success"] = "Category Updated Successfully";

                        return RedirectToAction(nameof(Index));

                    }

                }
                catch (DbUpdateException ex)
                {
                    TempData["Error"] = "Unable to delete the category. It may be used in other data (e.g., foreign key constraint).";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(obj);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Category? cat = await _UnitOfWork.Category.GetOneAsync(id);

            if (cat != null)
            {
                return View(cat);
            }
            return NotFound("No such category");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletee(int id)
        {
            Category? cat = await _UnitOfWork.Category.GetOneAsync(id);
            if (cat == null)
                return NotFound("No such category");

            try
            {
                _UnitOfWork.Category.Remove(cat);
                await _UnitOfWork.CompleteAsync();
                TempData["success"] = "Category Deleted Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to delete the category. It may be used in other data (e.g., foreign key constraint).";
                return RedirectToAction(nameof(Index));
            }

        }


    }
}
