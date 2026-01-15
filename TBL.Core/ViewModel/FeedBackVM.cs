using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.ViewModel
{
    public class FeedBackVM
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int productId { get; set; }
        [Required]
        public string content { get; set; }
    }
}
