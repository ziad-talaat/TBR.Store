using Application.EF.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TBL.Core.Converter;
using TBL.Core.Models;

namespace TBR.Store.Controllers
{
    public class CategoryController : Controller
    {

        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context=context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Category> objCategoryList = await _context.Category.AsNoTracking().ToListAsync();
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
              await   _context.Category.AddAsync(obj);
              await   _context.SaveChangesAsync();
              TempData["success"] = "Category Created Successfully";
              return RedirectToAction(nameof(CategoryController.Index));
            }

            return View(obj);
       }

          
          
       [HttpGet]
       public async Task<IActionResult> Edit(int id)
       {
            if (id == null || id == 0)
                return NotFound("No such Category");
            Category ?category = await _context.Category.FindAsync(id);
            if(category!=null)
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
                    Category? category = await _context.Category.FindAsync(obj.Id);
                    if (category != null)
                    {
                        Converter.ConvertToCategoryDTO(category, obj);

                        _context.Category.Update(category);
                        await _context.SaveChangesAsync();
                        TempData["success"] = "Category Updated Successfully";

                        return RedirectToAction(nameof(CategoryController.Index));

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
            Category? cat = await _context.Category.FindAsync(id);

            if(cat!=null)
            {
                return View(cat);
            }
            return NotFound("No such category");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletee(int id)
        {
            Category? cat = await _context.Category.FindAsync(id);
            if(cat==null)
            return NotFound("No such category");

            try
            {
                _context.Category.Remove(cat);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category Deleted Successfully";

                return RedirectToAction(nameof(CategoryController.Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to delete the category. It may be used in other data (e.g., foreign key constraint).";
                return RedirectToAction(nameof(Index));
            }
            
        }

    
    }
}
