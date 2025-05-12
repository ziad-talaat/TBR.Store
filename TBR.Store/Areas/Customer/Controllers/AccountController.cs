using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public AccountController(IAccountService accountService, 
            SignInManager<ApplicationUser> applicationUser,
             RoleManager<IdentityRole> roleManager,
             UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _signInManager = applicationUser;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var roles = await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();

            var viewModel = new RegisterVM
            {
              RolesItems=roles
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
            var roles = await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();

            var viewModel = new RegisterVM
            {
                RolesItems = roles
            };

            return View(viewModel);



        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.Login(loginVM);
                if (result.Succeeded)
                {
                    return Redirect("~/");
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

    }
}
