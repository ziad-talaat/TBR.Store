﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class ShoppingCart
    {
        [BindNever]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ShoppingCart.ProductId))]
        [ValidateNever]
        public Product Product { get; set; }

        [Range(1,1000,ErrorMessage ="please enter a value 1:1000")]
        public  int Count{ get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(ShoppingCart.UserId))]
        [ValidateNever]
        public ApplicationUser User { get; set; }


        [NotMapped]
        public double Price{ get; set; }
    }
}
