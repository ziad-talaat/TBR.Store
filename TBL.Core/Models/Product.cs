using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class Product
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
        [Display(Name ="List Price")]
        [Range(1,1000)]
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


        [Required(ErrorMessage ="Should select a category")]
        public int? CategoryId { get; set; }
        [ForeignKey(nameof(Product.CategoryId))]
        public Category? Category { get; set; }

    
        public string? ImageURL { get; set; }


        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<UserProduct_Voting> UserProduct_Voting { get; set; } = new List<UserProduct_Voting>();
        public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    }
}
