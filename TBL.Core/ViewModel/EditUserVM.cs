using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.ViewModel
{
    public class EditUserVM
    {
        [Required(ErrorMessage = "can't be blank")]
        public string Address { get; set; }

        [Required(ErrorMessage = "can't be blank")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "can't be blank")]
        public string Email { get; set; }

        [Required(ErrorMessage = "can't be blank")]
        public string PhoneNumber { get; set; }

        public static void MapToUser(EditUserVM userVM ,ApplicationUser user)
        {

            user.UserName = userVM.UserName;
            user.Email = userVM.Email;
            user.PhoneNumber = userVM.PhoneNumber;
            user.Address = userVM.Address;
            
        }
        public static void MapToEditUser(ApplicationUser user, EditUserVM userVM)
        {

            userVM.UserName = user.UserName;
            userVM.Email = user.Email;
            userVM.PhoneNumber = user.PhoneNumber;
            userVM.Address = user.Address;
            
        }

    }

}
