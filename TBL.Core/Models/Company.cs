using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be exactly 11 digits.")]
        public string  PhoneNumber { get; set; }

        public ICollection<ApplicationUser>Users { get; set; }=new List<ApplicationUser>();
    }
}
