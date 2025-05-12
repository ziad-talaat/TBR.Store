using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Address { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<UserProduct_Voting> UserProduct_Voting { get; set; } = new List<UserProduct_Voting>();
    }
}
