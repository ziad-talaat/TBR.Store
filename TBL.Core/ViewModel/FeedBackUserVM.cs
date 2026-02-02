using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.ViewModel
{
    public class FeedBackUserVM
    {
        public string UserId { get; set; }
        public int CommentId { get; set; }
        public string  UserName { get; set; }
        public string  Message { get; set; }
        public string  Date { get; set; }
        public bool  IsEdited { get; set; }
        public string  ImageUrl { get; set; }
      
    }
}
