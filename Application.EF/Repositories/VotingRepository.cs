using Application.EF.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Enums;
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

        public async Task<List<Product>> TopVoted()
        {
            var products = await _context.Product
            .Include(p => p.UserProduct_Voting)
            .Select(p => new
            {
                Product = p,
                Score = p.UserProduct_Voting.Count(v => v.VoteType == Voting.UpVote) -
                        p.UserProduct_Voting.Count(v => v.VoteType == Voting.DownVote)
            })
            .OrderByDescending(p => p.Score)
            .Select(p => p.Product)
            .ToListAsync();
            return products;
        }
    }
}
