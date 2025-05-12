using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.Core.Contracts.ServiceContracts
{
    public interface IAccountService
    {
        Task<Tuple<IdentityResult, ApplicationUser?>> RegisterUser(RegisterVM registerVM);
        Task<SignInResult> Login(LoginVM loginVM);
        Task LogOut();
    }
}
