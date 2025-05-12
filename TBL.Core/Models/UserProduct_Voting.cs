using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Enums;

namespace TBL.Core.Models
{
    public class UserProduct_Voting
    {
        public string  UserId { get; set; }
        public int ProductId { get; set; }

        public ApplicationUser User  { get; set; }
        public Product Product  { get; set; }

        public DateTime VotingTime { get; set; }
        public Voting VoteType { get; set; }
    }
    
}
