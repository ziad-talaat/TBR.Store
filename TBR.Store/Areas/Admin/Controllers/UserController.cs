using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.Core.ViewModel;
using TBL.EF.Repositories;

namespace TBR.Store.Areas.Admin.Controllers
{
    [Area(nameof(Areas.Admin))]
    [Authorize(Roles =Roles.Role_Admin)]
    public class UserController : Controller
    {

        private readonly IUnitOfWork _UnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _UnitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager; 
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


        [HttpGet]
        public async Task<IActionResult> RoleManagment(string userId)
        {
            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                AppUser = await _UnitOfWork.User.GetSpecific(u => u.Id == userId,true ,new[] {nameof(ApplicationUser.Company)} ),
               RolesList = _roleManager.Roles.Select(i => new SelectListItem
               {
                   Text = i.Name,
                   Value = i.Name
               }),
                CompanyList =  _UnitOfWork.Company.GetAllAsync().GetAwaiter().GetResult().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };


            var user = await _UnitOfWork.User.GetSpecific(u => u.Id == userId);

            var roles = await _userManager.GetRolesAsync(user);

            RoleVM.AppUser.Role = roles.FirstOrDefault(Roles.Role_Customer);

            return View(RoleVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleManagment(RoleManagmentVM roleVM)
        {
            
            string oldRole = _userManager.GetRolesAsync(await _UnitOfWork.User.GetSpecific(u => u.Id == roleVM.AppUser.Id))
                              .GetAwaiter().GetResult().FirstOrDefault(Roles.Role_Customer);

            ApplicationUser applicationUser = await _UnitOfWork.User.GetSpecific(u => u.Id == roleVM.AppUser.Id);


            if (!(roleVM.AppUser.Role == oldRole))
            {
                //a role was updated
                if (roleVM.AppUser.Role == Roles.Role_Company)
                {
                    applicationUser.CompanyId = roleVM.AppUser.CompanyId;
                }
                if (oldRole == Roles.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _UnitOfWork.User.Update(applicationUser);
               await  _UnitOfWork.CompleteAsync();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleVM.AppUser.Role).GetAwaiter().GetResult();

            }
            else
            {
                if (oldRole == Roles.Role_Company && applicationUser.CompanyId != roleVM.AppUser.CompanyId)
                {
                    applicationUser.CompanyId = roleVM.AppUser.CompanyId;
                    _UnitOfWork.User.Update(applicationUser);
                    await _UnitOfWork.CompleteAsync();
                }
            }

            return RedirectToAction("Index");
        }


           

    }
}
