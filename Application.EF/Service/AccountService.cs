using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Contracts.ServiceContracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.EF.Service
{
    
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
        {
            _userManager = userManager;
            _signInManager = signinManager;
           
        }

        public async Task<SignInResult> Login(LoginVM loginVM)
        {
            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, false);
                return result;
            }

            return SignInResult.Failed;
        }

        public async Task LogOut()
        {
              await _signInManager.SignOutAsync();
        }

        public async Task<Tuple<IdentityResult,ApplicationUser?>> RegisterUser(RegisterVM registerVM)
        {
            ApplicationUser?user=await _userManager.FindByEmailAsync(registerVM.Email);

            if (user != null)
            {
                return new Tuple<IdentityResult, ApplicationUser?>(IdentityResult.Failed(new IdentityError { Description = "Email" }), null);
            }

            ApplicationUser? user2 = await _userManager.FindByNameAsync(registerVM.UserName);

            if (user2 != null)
            {
                return new Tuple<IdentityResult, ApplicationUser?>(IdentityResult.Failed(new IdentityError { Description = "UserName" }), null);
            }
        

            ApplicationUser Newuser = new ApplicationUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                Address = registerVM.Address,
                CompanyId= registerVM.CompanyId
            };


            var result= await _userManager.CreateAsync(Newuser, registerVM.Password);

            return new Tuple<IdentityResult, ApplicationUser?>(result, Newuser);
        }
    }
}
