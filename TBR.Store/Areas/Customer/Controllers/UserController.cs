using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TBL.Core.Contracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHost;
        public UserController(IUnitOfWork unitOfWork, IWebHostEnvironment webHost, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork=unitOfWork;
            _webHost=webHost;
            _userManager=userManager;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity =(ClaimsIdentity) User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("user Not authenticated");

            ApplicationUser? user=await _unitOfWork.User.GetOneAsync(userId);
            UserPageVM userPageVM = new UserPageVM()
            {
                ImageUrl = user?.ImageUrl,
                UserName = user?.UserName ?? "No Name Exist",
                Email = user?.Email ?? "No email Exist",
                PhoneNumber = user?.PhoneNumber,
                Address=user.Address,
            };
            return View(userPageVM);
        }   

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeImage(IFormFile file)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("user Not authenticated");

            ApplicationUser? user = await _unitOfWork.User.GetOneAsync(userId);
            if (file != null)
            {
                if (!string.IsNullOrEmpty(user.ImageUrl))
               {
                   var oldimagePath = Path.Combine(_webHost.WebRootPath, user.ImageUrl.TrimStart('/'));

                           if (System.IO.File.Exists(oldimagePath))
                           {
                               System.IO.File.Delete(oldimagePath);
                           }
                }
                string wwwRootPath = _webHost.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                   string userPath = Path.Combine(wwwRootPath, @"Images\Users");
                using (var fileStream = new FileStream(Path.Combine(userPath, fileName), FileMode.Create))
               {
                   await file.CopyToAsync(fileStream);
               }
               user.ImageUrl = @"/Images/Users/" + fileName;
                _unitOfWork.User.Update(user);
               await  _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("user Not authenticated");

            ApplicationUser? user = await _unitOfWork.User.GetOneAsync(userId);

          bool result=  await _userManager.CheckPasswordAsync(user, oldPassword);

            if (result == true)
            {
                HttpContext.Session.SetString("AllowCanChange", "true");
                HttpContext.Session.SetString("oldPass", oldPassword);
                return RedirectToAction("CanChange");
            }

            TempData["Error"] = "the password isn't right";
            return RedirectToAction("Index");   
        }

        [HttpGet]
        public IActionResult CanChange()
        {
            var canChange = HttpContext.Session.GetString("AllowCanChange");
            if (canChange != "true")
            {
                return Unauthorized("You cannot access this directly.");
            }

            return View();
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CanChange(string password)
        {

            string oldPass=HttpContext.Session.GetString("oldPass");

            var canChange = HttpContext.Session.GetString("AllowCanChange");
            if (canChange != "true")
            {
                return Unauthorized("You cannot access this directly.");
            }
            HttpContext.Session.Remove("AllowCanChange");
            HttpContext.Session.Remove("oldPass");


            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("user Not authenticated");

            ApplicationUser? user = await _unitOfWork.User.GetOneAsync(userId);

            await  _userManager.ChangePasswordAsync(user, oldPass, password);
            
            TempData["success"] = "the password updated";

            return RedirectToAction("Index");   
        }



    }
}
