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
        public int ProductId { get; set; }
        [Required]
        public string Content { get; set; }
        public int? CommentId { get; set; }
    }
}
