using Microsoft.AspNetCore.Mvc;
using TBL.Core.Contracts;
using TBL.Core.Enums;

namespace TBL.Core.ViewComponants
{
    [ViewComponent]
    public class DownVote : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public DownVote(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {

            var Dislikes = await _unitOfWork.Vote.GetAllAsync(x => x.ProductId == productId && x.VoteType == Voting.DownVote, false);

            int count = Dislikes.Count();
            return View(count);
        }
    }
}
