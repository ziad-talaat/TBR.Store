using Application.EF.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Models;

namespace TBL.EF.Repositories
{
    public class VotingRepository : BaseRepository<UserProduct_Voting>, IVoting
    {
        public VotingRepository(AppDbContext context):base(context)
        {
            
        }
        public async Task<UserProduct_Voting?> GetSpecificVote(string userId, int productId)
        {
            return await _context.UserProduct_Voting.SingleOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        }
    }
}
