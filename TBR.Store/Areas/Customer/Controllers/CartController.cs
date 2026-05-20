using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TBL.Core.Contracts;
using TBL.Core.Contracts.ServiceContracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    [Authorize]
    public class CartController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IViewRenderService _viewRender;
        [BindProperty]
        public OrderInfoVM orderInfo { get; set; }

        public CartController(IUnitOfWork unitOfWork)//, IViewRenderService viewRender)
        {
            _unitOfWork= unitOfWork;
            //_viewRender= viewRender;
        }

        public async Task<IActionResult> Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var cartVM = new CartVM
            {
                CartList = await _unitOfWork.ShoppingCart
                .GetAllAsync(x => x.UserId == userId, false, new[] { nameof(ShoppingCart.Product) }),
               
            };

            IEnumerable<ProductImages> images = await _unitOfWork.ProductImages.GetAllAsync();

            foreach(var cart in cartVM.CartList)
            {
                cart.Product.ProductImages = images.Where(x => x.ProductId == cart.Product.Id).ToList();
                double price=GetPriceBasedOnQuantity(cart);
                cartVM.OrderTotal +=( price * cart.Count); 
                cart.Price = price;
            }
             return View(cartVM);
        }

        [HttpGet]
        public async Task<IActionResult> Plus(int cartId)
        {
            ShoppingCart? cart = await _unitOfWork.ShoppingCart
                 .GetSpecific(x => x.Id == cartId, true, new[] { nameof(ShoppingCart.Product) });
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (cart == null || cart.UserId != userId)
                return Json(false);


            var allUserCartsPrices = _unitOfWork.ShoppingCart.GetQueryy().Where(x=>x.UserId==userId).Select(x=>new
            {
               
                count =x.Count,
                price=GetPriceBasedOnQuantity(x.Count,x.Product)
            }).ToList();

            double oldPrice = GetPriceBasedOnQuantity(cart);
            decimal oldTotalPrice = (decimal) allUserCartsPrices.Sum(x => x.count * x.price);


                cart.Count++;
                await _unitOfWork.CompleteAsync();
            decimal newTotalPrice=0;
            double price= GetPriceBasedOnQuantity(cart);

            if (oldPrice!=price)
            {
                oldTotalPrice -=(decimal) ((cart.Count - 1) * oldPrice);
                newTotalPrice = oldTotalPrice + (decimal)(price * cart.Count);
            }
            else{

               newTotalPrice = (decimal)price + oldTotalPrice;
            }
            var productId = cart.ProductId;

            return Json(new {status=true,totalPrice=newTotalPrice,newPrice = price, productId, totalPricePerItem = price * cart?.Count });
        }
        [HttpGet]
        public async Task<IActionResult> Minus(int cartId)
        {
            var userIdentity = (ClaimsIdentity)User.Identity;
            var userId = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCart? cart = await _unitOfWork.ShoppingCart
                .GetSpecific(x => x.Id == cartId, true, new[] { nameof(ShoppingCart.Product) });
            if (cart == null || cart.UserId != userId)
                return Json(false);

           


            var allUserCartsPrices = _unitOfWork.ShoppingCart.GetQueryy().Where(x => x.UserId == userId).Select(x => new
            {
               
                count = x.Count,
                price = GetPriceBasedOnQuantity(x.Count, x.Product)
            }).ToList();

            double oldPrice = GetPriceBasedOnQuantity(cart);
            decimal oldTotalPrice = (decimal)allUserCartsPrices.Sum(x => x.count * x.price);


            int productId = 0;
            productId = cart.ProductId;

            if (cart.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count--;
            }
                await _unitOfWork.CompleteAsync();

            decimal newTotalPrice = 0;
            double price = GetPriceBasedOnQuantity(cart);

            if (oldPrice != price)
            {
                oldTotalPrice -= (decimal)((cart.Count + 1) * oldPrice);
                newTotalPrice = oldTotalPrice + (decimal)(price * cart.Count);
            }
            else
            {

                newTotalPrice = oldTotalPrice - (decimal)price;
            }
            string cartCount = _unitOfWork.ShoppingCart.GetQueryy().Where(x => x.UserId == userId).Count().ToString();


            return Json(new { status = true, totalPrice = newTotalPrice, newPrice = price ,productId,totalPricePerItem=price*cart?.Count, cartCount });
        }
         [HttpGet]
        public async Task<IActionResult> Delete(int cartId)
        {
            var userIdentity = (ClaimsIdentity)User.Identity;
            var userId = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCart? cart = await _unitOfWork.ShoppingCart.GetSpecific(x => x.Id == cartId, false, new[] {nameof(ShoppingCart.Product) });

            if (cart is null)
                return Json(new {status=false});
                _unitOfWork.ShoppingCart.Remove(cart);
                await _unitOfWork.CompleteAsync();
            
            double price = GetPriceBasedOnQuantity(cart);
            decimal takedOffPrice = (decimal)price * cart.Count;

            string cartCount = _unitOfWork.ShoppingCart.GetQueryy().Where(x => x.UserId == userId).Count().ToString();

            return Json(new {status=true, removedAmount=takedOffPrice,productId=cart.ProductId, cartCount });
        }

        public async Task<IActionResult> Summary()
        {
           var claimIdentity = (ClaimsIdentity)User.Identity;
           var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            OrderInfoVM orderInfo = new OrderInfoVM()
            {
                CartList = await _unitOfWork.ShoppingCart
                .GetAllAsync(x => x.UserId == userId, false, new[] { nameof(ShoppingCart.Product) }),
                OrderHeader = new()
            };
            orderInfo.OrderHeader.User=await _unitOfWork.User.GetSpecific(x=>x.Id == userId);
            orderInfo.OrderHeader.Adress = orderInfo.OrderHeader.User.Address;
            orderInfo.OrderHeader.PhoneNumber = orderInfo.OrderHeader.User.PhoneNumber;
            orderInfo.OrderHeader.Name = orderInfo.OrderHeader.User.UserName;
            orderInfo.OrderHeader.PostalCode = orderInfo.OrderHeader.User.PostalCode;

            foreach(var cart in orderInfo.CartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                orderInfo.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(orderInfo);
        }
          


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            orderInfo.OrderHeader.UserId = userId;
            orderInfo.CartList = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.UserId == userId, true, new[] {nameof(ShoppingCart.Product)});

            if (string.IsNullOrEmpty(orderInfo.OrderHeader.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone Number can't be blank");
                return RedirectToAction("Summary", orderInfo);
            }

            orderInfo.OrderHeader.OrderDate = DateTime.Now;
            orderInfo.OrderHeader.UserId = userId;
            orderInfo.OrderHeader.User=await _unitOfWork.User.GetSpecific(x=>x.Id==userId);
			foreach (var cart in orderInfo.CartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				orderInfo.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			if (orderInfo.OrderHeader.User.CompanyId.GetValueOrDefault() == 0)
			{
				orderInfo.OrderHeader.PaymentStatus = Payment_Status.PaymentStatusPending;
				orderInfo.OrderHeader.OrderStatus = Payment_Status.StatusPending;
			}
			else
			{
				orderInfo.OrderHeader.PaymentStatus = Payment_Status.PaymentStatusDelayedPayment;
				orderInfo.OrderHeader.OrderStatus = Payment_Status.StatusApproved;
			}


			await _unitOfWork.OrderHeader.AddAsync(orderInfo.OrderHeader);
			await _unitOfWork.CompleteAsync();

			foreach (var cart in orderInfo.CartList)
			{
				OrderDetails details = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = orderInfo.OrderHeader.Id,
					Count = cart.Count,
					Price = cart.Price,
				};
				await _unitOfWork.OrderDetails.AddAsync(details);
				await _unitOfWork.CompleteAsync();
			}
            if (orderInfo.OrderHeader.User.CompanyId.GetValueOrDefault() == 0)
			{

                #region stripeLogic
                //var domain = "http://localhost:34196/";
                //var options = new SessionCreateOptions
                //{
                //	SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={orderInfo.OrderHeader.Id}",
                //	CancelUrl = domain + "customer/cart/index",
                //	LineItems = new List<SessionLineItemOptions>(),
                //	Mode = "payment",
                //};

                //foreach (var item in orderInfo.CartList)
                //{
                //	var sessionLineItem = new SessionLineItemOptions
                //	{
                //		PriceData = new SessionLineItemPriceDataOptions
                //		{
                //			UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                //			Currency = "usd",
                //			ProductData = new SessionLineItemPriceDataProductDataOptions
                //			{
                //				Name = item.Product.Title
                //			}
                //		},
                //		Quantity = item.Count
                //	};
                //	options.LineItems.Add(sessionLineItem);
                //}


                //var service = new SessionService();
                //Session session = service.Create(options);
                //await _unitOfWork.OrderHeader.UpdateStrpePaymentId(orderInfo.OrderHeader.Id, session.Id, session.PaymentIntentId);
                //await _unitOfWork.CompleteAsync();
                //Response.Headers.Add("Location", session.Url);
                //return new StatusCodeResult(303);
                #endregion
            }

            return RedirectToAction("OrderConfirmation", new {id=orderInfo.OrderHeader.Id});
		}





        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader header = await _unitOfWork.OrderHeader.GetSpecific(x => x.Id == id, true, new[] { nameof(OrderHeader.User) });

                if( header.PaymentStatus!= Payment_Status.PaymentStatusDelayedPayment)
                {
				#region Check if paid succssefully
				// var service=new SessionService();
				// Session session = service.Get(header.SessionId);
				// if (session.PaymentStatus.ToLower() == "paid")
				// {
				//  await _unitOfWork.OrderHeader.UpdateStrpePaymentId(id, session.Id, session.PaymentIntentId);
				#endregion
				await _unitOfWork.OrderHeader.UpdateStatus(id, Payment_Status.StatusApproved, Payment_Status.StatusApproved);
                   await  _unitOfWork.CompleteAsync();
				//}
			}
            IEnumerable<ShoppingCart> carts = await _unitOfWork.ShoppingCart.GetAllAsync(x=>x.UserId==header.UserId,true);
            _unitOfWork.ShoppingCart.RemoveRange(carts);
            await _unitOfWork.CompleteAsync();

            return View(id);
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

        [NonAction]
        private static double GetPriceBasedOnQuantity(int count,Product product)
        {
            if (count <= 50)
                return product.Price;
            else if (count >= 51 && count <= 100)
                return product.Price50;
            return product.Price100;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemsDetails()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           var data= _unitOfWork.Products.GetCartData(userId);

            string html = await RenderPartialViewToStringAsync(

          this,
          "_cartHover",
          data
      );

            return Json(new
            {
                html,
                data = data.Select(x => new
                {
                    x.AuthorName,
                    x.count,
                    x.ItemImagePath,
                    x.ItemName,
                    x.Price,
                    x.productId
                }).ToList()
            });
           

            //return PartialView("_cartHover", data);
        }

        public static async Task<string> RenderPartialViewToStringAsync(
  Controller controller,
  string viewName,
  object model)
        {
            controller.ViewData.Model = model;

            using var writer = new StringWriter();

            var viewEngine = controller.HttpContext.RequestServices
                .GetService<ICompositeViewEngine>();

            var viewResult = viewEngine.FindView(
                controller.ControllerContext,
                viewName,
                false
            );

            if (!viewResult.Success)
            {
                throw new Exception($"View '{viewName}' not found.");
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }

    }
}