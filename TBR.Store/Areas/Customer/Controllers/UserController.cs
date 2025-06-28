using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stripe;
using Stripe.Climate;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Enums;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHost;
        public UserController(IUnitOfWork unitOfWork, IWebHostEnvironment webHost, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork=unitOfWork;
            _webHost=webHost;
            _userManager=userManager;
            _signInManager = signInManager;
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
        public async Task<IActionResult> ChangePassword(ChangePasswordVM pass)
        {
            
            if (ModelState.IsValid)
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized("user Not authenticated");

                var user = await _userManager.GetUserAsync(User);

                var result = await _userManager.ChangePasswordAsync(user, pass.OldPassword, pass.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["success"] = "password Changed Successfully";
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(pass);   


        }

        //[HttpGet]
        //public IActionResult CanChange()
        //{
        //    var canChange = HttpContext.Session.GetString("AllowCanChange");
        //    if (canChange != "true")
        //    {
        //        return Unauthorized("You cannot access this directly.");
        //    }

        //    return View();
        //}

        // [HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CanChange(string password)
        //{

        //    string oldPass=HttpContext.Session.GetString("oldPass");

        //    var canChange = HttpContext.Session.GetString("AllowCanChange");
        //    if (canChange != "true")
        //    {
        //        return Unauthorized("You cannot access this directly.");
        //    }
        //    HttpContext.Session.Remove("AllowCanChange");
        //    HttpContext.Session.Remove("oldPass");


        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (userId == null)
        //        return Unauthorized("user Not authenticated");

        //    ApplicationUser? user = await _unitOfWork.User.GetOneAsync(userId);

        //    await  _userManager.ChangePasswordAsync(user, oldPass, password);
            
        //    TempData["success"] = "the password updated";

        //    return RedirectToAction("Index");   
        //}

        [HttpGet]
        public ActionResult<string?> GetUserImage()
        {
          
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Redirect("/images/Users/cat.jpg"); // fallback image

            string? imageUrl = _unitOfWork.User.GetUserImageUrl(userId);

            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "/images/Users/cat.jpg";

            return Redirect(imageUrl);

        }

        [HttpGet]
        public async Task<IActionResult> EditUSerInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
               return Unauthorized();
            ApplicationUser? user = await _unitOfWork.User.GetSpecific(x => x.Id == userId, true);

            EditUserVM userToEdit=new EditUserVM();

            EditUserVM.MapToEditUser(user, userToEdit);
            return View(userToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUSerInfo(EditUserVM userVM)
        {
            if (ModelState.IsValid) {
               

                string phone = userVM.PhoneNumber;
                if (!Regex.IsMatch(phone, @"^01[0-2,5][0-9]{8}$"))
                {
                    ModelState.AddModelError("phone", "phone is not valid");
                    return View(userVM);
                }


                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);

                var existingEmailUser = await _userManager.FindByEmailAsync(userVM.Email);
                if (existingEmailUser != null && existingEmailUser.Id != currentUser.Id)
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(userVM);
                }
                var existingNameUser = await _userManager.FindByNameAsync(userVM.UserName);
                if (existingNameUser != null && existingNameUser.Id != currentUser.Id)
                {
                    ModelState.AddModelError("Name", "Name is already in use.");
                    return View(userVM);
                }



                 EditUserVM.MapToUser(userVM, currentUser);
                _unitOfWork.User.Update(currentUser);
                await _unitOfWork.CompleteAsync();
                TempData["success"] = "user Update successfully";
                return RedirectToAction("Index");

            }
            return View(userVM);
                
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {

            var userId= User.FindFirstValue(ClaimTypes.NameIdentifier);


           IEnumerable<OrderHeader>orders=await  _unitOfWork.OrderHeader.GetAllAsync(x => x.UserId == userId
           &&x.PaymentStatus==Payment_Status.PaymentStatusApproved, false, new[] { "OrderDetails.Product" } );


            var orderrsVM = orders.Select(x => new MyOrdersVM
            {
                Id=x.Id,
                OrderDate=x.OrderDate,
                OrderTotalPrice=x.OrderTotal,
               Count=x.OrderDetails.Count,
               PaymentStatus=x.PaymentStatus

            }).ToList();

            return View(orderrsVM);

        }

        [HttpGet]
        public async Task<IActionResult> Details(int orderHederId)
        {
            IEnumerable<OrderDetails> details = await _unitOfWork.OrderDetails.GetAllAsync(x => x.OrderHeaderId == orderHederId, false, new[] { "Product.ProductImages" });

            var detilsVm = details.Select(x => new OrderDetailsVM
            {
                ProductId=x.ProductId,
                ProductName=x.Product.Title,
                Count = x.Count,
                Price = x.Price,
                ImageUrl = x.Product.ProductImages.Select(x => x.ImageIrl).FirstOrDefault()
            }).ToList();

            return View(detilsVm);

        }

           


           



    }
}
