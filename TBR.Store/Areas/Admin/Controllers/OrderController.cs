using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBR.Store.Areas.Admin.Controllers
{

     [Area("Admin")]
    [Authorize] 
    
    public class OrderController : Controller
	{

		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<ApplicationUser> _userManager;
		[BindProperty]
		public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
		[HttpGet]
        public async Task<IActionResult> Index()
		{
			

			return View();
		}
		 [HttpGet]
		 public async Task<IActionResult> Details(int id)
		 {
			orderVM =new()
			{

				OrderHeader = await _unitOfWork.OrderHeader.GetSpecific(x => x.Id == id, false, new[] { nameof(OrderHeader.User) }),
				OrderDetails = await _unitOfWork.OrderDetails.GetAllAsync(x => x.OrderHeaderId == id, false, new[] {nameof(OrderDetails.Product)})
			};
			return View(orderVM);
  		 }

		[HttpPost]
		[Authorize(Roles=$"{Roles.Role_Employee},{Roles.Role_Admin}")]
		public async Task<IActionResult> UpdateOrderDetail()
		{
			var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetSpecific(x => x.Id == orderVM.OrderHeader.Id, false);

            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            orderHeaderFromDb.Adress = orderVM.OrderHeader.Adress;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			await _unitOfWork.CompleteAsync();

			return RedirectToAction(nameof(Details), new { id = orderHeaderFromDb.Id });

        }


		public async Task<IActionResult> GetAll() {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            var roles = await _userManager.GetRolesAsync(user);


            IEnumerable<OrderHeader> orderHeader= new List<OrderHeader>();
            if (!roles.Contains(Roles.Role_Employee) && !roles.Contains(Roles.Role_Admin))
            {
                orderHeader = await _unitOfWork.OrderHeader
                .GetAllAsync(x => x.UserId == userId, false, new[] { nameof(OrderHeader.User) });

            }
            else
            {

                orderHeader = await _unitOfWork.OrderHeader
                     .GetAllAsync(false, new[] { nameof(OrderHeader.User) });
            }

			return Json(new { data = orderHeader });

		}

        [HttpPost]
        [Authorize(Roles = $"{Roles.Role_Employee},{Roles.Role_Admin}")]
        public async Task<IActionResult> StartProcessing() 
        {
               await  _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, Payment_Status.StatusProcessing);
                await _unitOfWork.CompleteAsync();
            
                TempData["success"] = "order Processed Successfully";
                return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });
            
        }


        [HttpPost]
        [Authorize(Roles = $"{Roles.Role_Employee},{Roles.Role_Admin}")]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetSpecific(x => x.Id == orderVM.OrderHeader.Id, true);
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.OrderStatus = Payment_Status.StatusShipped;
            orderHeader.ShippingDate=DateTime.Now;
            if(orderHeader.PaymentStatus == Payment_Status.PaymentStatusDelayedPayment)
            {
              orderHeader.ShippingDueDate=DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            await _unitOfWork.CompleteAsync();

            TempData["success"] = "order Shipped Successfully";
            return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Role_Employee},{Roles.Role_Admin}")]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetSpecific(x => x.Id == orderVM.OrderHeader.Id, true);

            if (orderHeader.PaymentStatus == Payment_Status.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                await _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Payment_Status.StatusCancelled, Payment_Status.StatusRefunded);
            }
            else
            {
              await   _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Payment_Status.StatusCancelled, Payment_Status.StatusCancelled);
            }
           await  _unitOfWork.CompleteAsync();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Role_Employee},{Roles.Role_Admin}")]
        public async Task<IActionResult> Pay()
        {
            orderVM.OrderHeader = await _unitOfWork.OrderHeader
                .GetSpecific(u => u.Id == orderVM.OrderHeader.Id,true);
            orderVM.OrderDetails = await _unitOfWork.OrderDetails
                .GetAllAsync(u => u.OrderHeaderId == orderVM.OrderHeader.Id, true, new[] {nameof(OrderDetails.Product)});

            #region Stripe
            //   var domain = "http://localhost:34196/";
            //   var options = new SessionCreateOptions
            //   {
            //       SuccessUrl = domain + $"Admin/order/orderconfirmation?id={orderVM.OrderHeader.Id}",
            //       CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
            //       LineItems = new List<SessionLineItemOptions>(),
            //       Mode = "payment",
            //   };
            //
            //   foreach (var item in orderVM.OrderDetails)
            //   {
            //       var sessionLineItem = new SessionLineItemOptions
            //       {
            //           PriceData = new SessionLineItemPriceDataOptions
            //           {
            //               UnitAmount = (long)(item.Price * 100), 
            //               Currency = "usd",
            //               ProductData = new SessionLineItemPriceDataProductDataOptions
            //               {
            //                   Name = item.Product.Title
            //               }
            //           },
            //           Quantity = item.Count
            //       };
            //       options.LineItems.Add(sessionLineItem);
            //   }
            //
            //
            //   var service = new SessionService();
            //   Session session = service.Create(options);
            // await _unitOfWork.OrderHeader.UpdateStrpePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            //await   _unitOfWork.CompleteAsync();
            //   Response.Headers.Add("Location", session.Url);
            //   return new StatusCodeResult(303);
            #endregion
            return RedirectToAction(nameof(PaymentConfirmation), new { orderHeaderId = orderVM.OrderHeader.Id });
        }

        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader =await  _unitOfWork.OrderHeader.GetSpecific(u => u.Id == orderHeaderId,true);
            if (orderHeader!.PaymentStatus == Payment_Status.PaymentStatusDelayedPayment)
            {

               // var service = new SessionService();
               // Session session = service.Get(orderHeader.SessionId);
               //
               // if (session.PaymentStatus.ToLower() == "paid")
               // {
                  // await  _unitOfWork.OrderHeader.UpdateStrpePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                   await  _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, Payment_Status.PaymentStatusApproved);
                   await  _unitOfWork.CompleteAsync();
               // }


            }


            return View(orderHeaderId);
        }


    }


}

