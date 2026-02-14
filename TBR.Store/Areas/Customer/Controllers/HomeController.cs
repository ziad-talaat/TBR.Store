using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using TBL.Core.Contracts;
using TBL.Core.Converter;
using TBL.Core.Models;
using TBL.Core.ViewModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using Humanizer;

namespace TBR.Store.Areas.Customer.Controllers
{
    [Area(nameof(Areas.Customer))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;   
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork= unitOfWork;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string ?searchBy,string?searchValue,string ?sortBy,string ?categoryValue ,bool isAssending=true,int pageNumber=1)
        {

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            Pagination<Product> pageDetails=  _unitOfWork.Products.GetAllSortedAndFilterdInPage(searchBy,searchValue,sortBy, categoryValue, isAssending, pageNumber, new[] {nameof(Product.ProductImages)});
            if (isAjax)
            {
                return PartialView("_filterdProducts", pageDetails);
            }

            ////ViewBag.CurrentSearchBy = searchBy;
            ////ViewBag.CurrentSearchValue = searchValue;
            ////ViewBag.CurrentSortBy = sortBy;
            ////ViewBag.CurrentOrder = isAssending;
            ////ViewBag.CurrentCategory = categoryValue;
            
            ViewBag.SearchItems = new List<SelectListItem>
            {
                new SelectListItem { Value = nameof(Product.Title), Text = "Title", Selected = (searchBy == nameof(Product.Title)) },
              new SelectListItem { Value = nameof(Product.ISBN), Text = "ISBN", Selected = (searchBy == nameof(Product.ISBN)) },
              new SelectListItem { Value = nameof(Product.Author), Text = "Author", Selected = (searchBy == nameof(Product.Author)) },
              new SelectListItem { Value = nameof(Product.Price), Text = "Price", Selected = (searchBy == nameof(Product.Price)) },
            }
            ;

            var categoriesNames = await _unitOfWork.Category.GetCategoriesName();
            ViewBag.FilterCategories = categoriesNames.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (categoryValue == x)
            }).ToList();

          
            
            return View(pageDetails);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            Product ?product = await _unitOfWork.Products.GetSpecific(x => x.Id == id, false, new[] {nameof(Product.Category),nameof(Product.ProductImages)});
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var vote = await _unitOfWork.Vote.GetSpecificVote(userId, id);
           string voteType = vote?.VoteType.ToString();
            ViewBag.CurrentVote= voteType;
            if (product==null)
            {
                TempData["Error"] = "no such product ";
                return RedirectToAction(nameof(HomeController.Index));
            }
            ShoppingCart cart = new()
            {
                Product = product,
                Count = 1,
                ProductId = id,
                UserId = userId.ToString()
            };
            var Comminted = _unitOfWork.FeedBack.GetQueryy().FirstOrDefault(x => x.UserId == userId && x.ProductId == id);
           

                ViewBag.CanComment = false ;
            var boughtProductsIds=  _unitOfWork.OrderHeader.GetApprovedProduct(userId);
            if(boughtProductsIds.Any(x=>x==id) && Comminted is null) {
                ViewBag.CanComment = true;
            }
           
                return View(cart);
        }   
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart cart)
        {
            var claimIdentity =(ClaimsIdentity) User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            cart.UserId = userId;

           ShoppingCart ? cartExist = await _unitOfWork.ShoppingCart.GetSpecific(x => x.UserId == userId&&x.ProductId==cart.ProductId,true);

            if (cartExist != null) {

                cartExist.Count += cart.Count;
                _unitOfWork.ShoppingCart.Update(cartExist);
                await _unitOfWork.CompleteAsync();
            }
            else {
                try
                {
                    await _unitOfWork.ShoppingCart.AddAsync(cart);
                    await _unitOfWork.CompleteAsync();
                    TempData["success"] = "cart added successfully";
                }
                catch (DbUpdateException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(HomeController.Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Vote(Voting voteType,int productId)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingVote = await _unitOfWork.Vote.GetSpecificVote(userId, productId);

            if (existingVote != null)
            {
                if (voteType == existingVote.VoteType)
                {
                    _unitOfWork.Vote.Remove(existingVote);
                    await _unitOfWork.CompleteAsync();
                    return Json(new { status = true, data = $"x{voteType}" });
                }
                else
                {
                    existingVote.VoteType = voteType;
                    existingVote.VotingTime = DateTime.Now;
                    _unitOfWork.Vote.Update(existingVote);
                    await _unitOfWork.CompleteAsync();
                    return Json(new { status = true, data = $"o{voteType}" });
                }
            }
            else
            {
                UserProduct_Voting newVote = new UserProduct_Voting()
                {
                    ProductId = productId,
                    UserId = userId,
                    VotingTime = DateTime.Now,
                    VoteType = voteType,
                };
                try
                {
                   await  _unitOfWork.Vote.AddAsync(newVote);
                    await _unitOfWork.CompleteAsync();
                    return Json(new { status = true, data = $"o{voteType}" });
                }
                catch(DbUpdateException ex)
                {
                    TempData["Error"] = "some error while voting try later";
                    return Json(new { status = false });
                }
            }
           
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddFeedBack(FeedBackVM feedBack)
        {
            if (!ModelState.IsValid)
                return Json(new {status=false,message="Invalid Input"});
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Json(new { status = false, message = "Not Authorized" });

            var Comminted=_unitOfWork.FeedBack.GetQueryy().FirstOrDefault(x=>x.UserId== userId && x.ProductId==feedBack.ProductId);
            if(Comminted!=null)
                return Json(new { status = false, message = "there is a comment" });
            var comment = new FeedBack()
            {
                UserId = userId,
                ProductId = feedBack.ProductId,
                IsEdited = false,
                Date = DateTime.Now,
                Comment = feedBack.Content
            };
            try
            {

            await _unitOfWork.FeedBack.AddAsync(comment);
           await  _unitOfWork.CompleteAsync();
            return Json(new { status = true, message = "comment added successfully" });
            }
            catch(DbUpdateException)
            {
            return Json(new { status = false, message = "Error" });
            }
        }

        [HttpGet("GetFeedBack")]
        public  IActionResult GetFeedBack(int productId)
        {
            return ViewComponent("FeedBackPerProduct", new { productId });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentID)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Json(false);
            var comment= _unitOfWork.FeedBack.GetQueryy().SingleOrDefault(x => x.Id == commentID);

            if (comment is null || userId != comment.UserId)
                return Json(false);

           _unitOfWork.FeedBack.Remove(comment);
          await  _unitOfWork.CompleteAsync();
            return Json(true);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateComment(FeedBackVM feedBackVM)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Json(false);

            var comment = _unitOfWork.FeedBack.GetQueryy().AsTracking().SingleOrDefault(x => x.Id ==feedBackVM.CommentId);

            if (comment is null || userId != comment.UserId)
                return Json(false);

                comment.Comment = feedBackVM.Content;
                comment.IsEdited = true;
                comment.Date = DateTime.Now;
            try
            {
                await _unitOfWork.CompleteAsync();
                return Json(new {status=true,NewContent=comment.Comment,NewDate=comment.Date.Humanize()});
            }

            catch (DbUpdateException)
            {
                return Json(false);
            }
        }
    }
}
