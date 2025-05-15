using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Address { get; set; }
        [Required(ErrorMessage ="can't be blank")]
        public string PostalCode { get; set; }
        public string? ImageUrl { get; set; }

        public int? CompanyId { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<UserProduct_Voting> UserProduct_Voting { get; set; } = new List<UserProduct_Voting>();
        [JsonIgnore]
        public ICollection<OrderHeader> OrderHeaders { get; set; } = new List<OrderHeader>();
    }
}
