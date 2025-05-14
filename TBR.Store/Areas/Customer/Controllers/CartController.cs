using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TBL.Core.Contracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var cartVM = new CartVM
            {
                CartList = await _unitOfWork.ShoppingCart
                .GetAllAsync(x => x.UserId == userId, false, new[] {nameof(ShoppingCart.Product)}),
            };

            foreach(var cart in cartVM.CartList)
            {
                double price=GetPriceBasedOnQuantity(cart);
                cartVM.OrderTotal +=( price * cart.Count); 
                cart.Price = price;
            }
             return View(cartVM);
        }

     [HttpGet]
        public async Task<IActionResult> Plus(int cartId)
        {
            ShoppingCart? cart = await _unitOfWork.ShoppingCart.GetSpecific(x => x.Id == cartId, true);
            if (cart != null)
            {
                cart.Count++;
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(CartController.Index), nameof(CartController));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            ShoppingCart? cart = await _unitOfWork.ShoppingCart.GetSpecific(x => x.Id == cartId, true);
            if (cart.Count == 1)
                _unitOfWork.ShoppingCart.Remove(cart);
            if (cart != null)
            {
                cart.Count--;
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(CartController.Index), nameof(CartController));
        }



        [NonAction]
        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if (cart.Count <= 50)
                return cart.Product.Price;
            else if(cart.Count>=51 && cart.Count <=100)
                return cart.Product.Price50;

            return cart.Product.Price100;
        }




    }
}
