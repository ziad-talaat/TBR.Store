using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using TBL.Core.Contracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.Core.ViewComponants
{
    [ViewComponent]
    public class FeedBackPerProduct:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public FeedBackPerProduct(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
          var feedBAcks= await _unitOfWork.FeedBack.GetAllAsync(x => x.ProductId == productId, false, new[] {nameof(FeedBack.User)});

           
            var result = feedBAcks.Select(x => new FeedBackUserVM
            { UserName = x.User.Email,
              UserId=x.UserId,
                Message = x.Comment,
                Date = x.Date.Humanize(),
                ImageUrl = x.User?.ImageUrl,
                CommentId=x.Id
            }).ToList();
            ViewBag.commentCount = _unitOfWork.Products.FeedBacksCount(productId);
            return View(result);
        }
    }
}
