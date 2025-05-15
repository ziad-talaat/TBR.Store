using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Enums;

namespace TBL.Core.ViewComponants
{
    [ViewComponent]
    public class UpVote:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpVote(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {

           var likes= await _unitOfWork.Vote.GetAllAsync(x => x.ProductId == productId && x.VoteType == Voting.UpVote, false);

            int count=likes.Count();
            return  View(count);
        }
    }
}
