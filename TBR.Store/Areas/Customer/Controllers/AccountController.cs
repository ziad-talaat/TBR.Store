using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Contracts.ServiceContracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public AccountController(IAccountService accountService, 
            SignInManager<ApplicationUser> applicationUser,
             RoleManager<IdentityRole> roleManager,
             UserManager<ApplicationUser> userManager,
              IUnitOfWork unitOfWork)
        {
            _accountService = accountService;
            _signInManager = applicationUser;
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        public async Task<IActionResult> Register()
        {
           

            var viewModel = new RegisterVM
            {
                RolesItems = await GetRolesList(),
                CompanyItems =await  GetCompanyList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!await _roleManager.RoleExistsAsync(Roles.Role_Admin))
            {
                await  _roleManager.CreateAsync(new IdentityRole(Roles.Role_Customer));
                await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Employee));
               await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Admin));
               await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Company));
            }
               

            if (ModelState.IsValid)
            {
                Tuple<IdentityResult, ApplicationUser?> tuple = await _accountService.RegisterUser(registerVM);

                if (tuple.Item2 == null)
                {
                    foreach (var error in tuple.Item1.Errors)
                    {
                        if (error.Description.Contains("Email"))
                        {
                            ModelState.AddModelError("Email", "The email address is already in use.");
                        }
                        else
                        {
                            ModelState.AddModelError("UserName", "The UserName address is already in use.");
                        }
                    }
                }

                var result = tuple.Item1;
                var user = tuple.Item2;

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    await _userManager.AddToRoleAsync(user, registerVM.Role??Roles.Role_Customer);

                    return Redirect("~/");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(nameof(registerVM.Password), error.Description);
                        
                    }
                }
            }
         

            var viewModel = new RegisterVM
            {
                RolesItems = await GetRolesList(),
                CompanyItems= await GetCompanyList()
            };

            return View(viewModel);


        }


        [HttpGet]
        public IActionResult Login(string returnUrl=null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM,string url=null)
        {
            url= url ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await _accountService.Login(loginVM);
                if (result.Succeeded)
                {
                    return LocalRedirect(url);
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(loginVM);
        }


        public async Task<IActionResult> LogOut()
        {
            await _accountService.LogOut();
               return Redirect("~/");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        [NonAction]
        private async Task<List<SelectListItem>> GetCompanyList()
        {
            var companies = await _unitOfWork.Company.GetAllAsync(false);
            var companiesNames = companies.ToList().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            return companiesNames;
        }
        [NonAction]
        private async Task<List<SelectListItem>> GetRolesList()
        {
            return await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();
        }

    }
}
