
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TBL.Core.ViewModel
{
    public class RegisterVM
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password should match Password")]
        public string ConfirmPassword { get; set; }

        public string? Role { get; set; }
        public IEnumerable<SelectListItem>? RolesItems { get; set; }

         public int? CompanyId { get; set; }
        public IEnumerable<SelectListItem>? CompanyItems { get; set; }



    }
}
