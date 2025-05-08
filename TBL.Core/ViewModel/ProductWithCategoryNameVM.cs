using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.ViewModel
{
    public class ProductWithCategoryNameVM
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Describtion { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        [Display(Name = "List Price")]
        [Range(1, 1000)]
        public double DisplayPrice { get; set; }




        [Required]
        [Display(Name = "price 1-50")]
        [Range(1, 1000)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "price for 50+")]
        [Range(1, 1000)]
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "price for 100+")]
        [Range(1, 1000)]
        public double Price100 { get; set; }


        public string CategoryName { get; set; }
    }
}
