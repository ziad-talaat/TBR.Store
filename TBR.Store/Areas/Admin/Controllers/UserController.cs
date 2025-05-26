using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    [Authorize(Roles =Roles.Role_Admin)]
    public class UserController : Controller
    {

        private readonly IUnitOfWork _UnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _UnitOfWork = unitOfWork;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
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
        public async Task<IActionResult> LockUnLock([FromBody]string id)
        {
            var user = await _UnitOfWork.User.GetSpecific(x => x.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while locking/UnLocking" });
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(15);
            }
            await _UnitOfWork.CompleteAsync();

                return Json(new { success = true, message = "operation successfull" });
        }


        [NonAction]
        private async  Task<bool> isAdmin(ApplicationUser user)
        {
            var roles = await  _userManager.GetRolesAsync(user);
            if (roles.Any(x=>x.Contains("Admin")))
            {
                return false;
            }
            return true;
        }


        public async Task< IActionResult> GetAlll()
        {
            IEnumerable<ApplicationUser> users = await  _UnitOfWork.User.GetAll(isAdmin, true, new[] 
            {nameof(ApplicationUser.Company)} );

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault(Roles.Role_Customer);
            }


            return Json(new {data=users});
        }

    }
}
