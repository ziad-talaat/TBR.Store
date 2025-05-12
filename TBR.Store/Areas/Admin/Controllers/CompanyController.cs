using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    [Authorize(Roles =Roles.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _UnitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Company> objCategoryList = await _UnitOfWork.Company.GetAllAsync(false);
            return View(objCategoryList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Company obj)
        {
            if (ModelState.IsValid)
            {
                

                await _UnitOfWork.Company.AddAsync(obj);
                await _UnitOfWork.CompleteAsync();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || id == 0)
                return NotFound("invalid entry");
            Company? company = await _UnitOfWork.Company.GetOneAsync(id);
            if (company != null)
                return View(company);

            return NotFound("No such company");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Company obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                        _UnitOfWork.Company.Update(obj);
                        await _UnitOfWork.CompleteAsync();
                        TempData["success"] = "company Updated Successfully";

                        return RedirectToAction(nameof(Index));
                }


                catch (DbUpdateException ex)
                {
                    TempData["Error"] = "Unable to delete the company. It may be used in other data (e.g., foreign key constraint).";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(obj);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Company? cat = await _UnitOfWork.Company.GetOneAsync(id);

            if (cat != null)
            {
                return View(cat);
            }
            return NotFound("No such Cmpany");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            Company? company = await _UnitOfWork.Company.GetOneAsync(id);
            if (company == null)
                return NotFound("No such Company");

            try
            {
                _UnitOfWork.Company.Remove(company);
                await _UnitOfWork.CompleteAsync();
                TempData["success"] = "Company Deleted Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to delete the Comapny. It may be used in other data (e.g., foreign key constraint).";
                return RedirectToAction(nameof(Index));
            }

        }


    }
}
