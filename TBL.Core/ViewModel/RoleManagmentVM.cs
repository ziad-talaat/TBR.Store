
using Microsoft.AspNetCore.Mvc.Rendering;
using TBL.Core.Models;

namespace TBL.Core.ViewModel
{
    public class RoleManagmentVM
    {
        public ApplicationUser AppUser { get; set; }
        public IEnumerable<SelectListItem> RolesList { get; set; }
        public IEnumerable<SelectListItem>? CompanyList { get; set; }
    }
}
