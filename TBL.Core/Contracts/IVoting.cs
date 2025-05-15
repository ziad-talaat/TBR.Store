using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.Contracts
{
    public interface IVoting:IBaseRepository<UserProduct_Voting>
    {
        Task<UserProduct_Voting?> GetSpecificVote(string userId, int productId);
        Task<List<Product>> TopVoted();
    }
}
