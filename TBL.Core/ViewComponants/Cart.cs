using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Models;

namespace TBL.Core.ViewComponants
{
    [ViewComponent]
    public class CartViewComponent:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdentity =(ClaimsIdentity) User.Identity;
            var userId = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int count = 0;
            if (userId != null) 
            {
                var carts = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.UserId == userId, false);
                count = carts.Count();
            }
            return View(count);

        }
    }
}
