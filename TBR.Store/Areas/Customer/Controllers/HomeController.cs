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
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISearchTrie _searchTrie;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork,ISearchTrie searchTrie)
        {
            _logger = logger;
            _unitOfWork= unitOfWork;
            _searchTrie= searchTrie;
        }


        [HttpGet]
        public async Task<IActionResult> Index(SearchAndSortDataModel data)
        {
            ViewBag.CurrentSearchValue = data.searchValue;
            ViewBag.CurrentSortBy = data.sortBy;
            ViewBag.CurrentCategory = data.categoryValue;
            ViewBag.CurrentOrder = data.isAssending;
            ViewBag.CurrentPage = data.pageNumber;

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            Pagination<Product> pageDetails=  _unitOfWork.Products.GetAllSortedAndFilterdInPage(data.searchValue, data.sortBy, data.categoryValue, data.isAssending, data.pageNumber, new[] {nameof(Product.ProductImages)});
            if (isAjax)
            {
                if (data.fromSearch is true)
                {
                  var product= await _unitOfWork.Products.GetSpecific(x => x.Title == data.searchValue, true);
                   if( product is not null)
                    {
                        product.ClickedCount++;
                       _unitOfWork?.CompleteAsync();
                        _searchTrie.UpdateCount(product.Title, product.ClickedCount);
                    }

                }
              
                return PartialView("_filterdProducts", pageDetails);
            }

            var categoriesNames = await _unitOfWork.Category.GetCategoriesName();
            ViewBag.FilterCategories = categoriesNames.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (data.categoryValue == x)
            }).ToList();

           

            return View(pageDetails);
        }
       

          [HttpGet]
         public IActionResult GetSearchBar(string value)
       {
            //var result = _unitOfWork.Products.GetSearchValue(value);

             //.Where(x => x.Title.ToLower().StartsWith(value.ToLower()))
             //   .OrderByDescending(x => x.ClickedCount)
             //   .Take(5).Select(x => x.Title)
             //   .ToList();


            List<(string, int)> words = new List<(string ,int)>();
             _searchTrie.AutoComplete(value?.ToLower(), words);
            return PartialView("_searchBar",words.OrderByDescending(x=>x.Item2).Take(5).Select(x=>x.Item1).ToList());
         }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id, SearchAndSortDataModel data)
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

            ViewBag.searchValue = data.searchValue;
            ViewBag.sortBy = data.sortBy;
            ViewBag.categoryValue = data.categoryValue;
            ViewBag.isAssending = data.isAssending;
            ViewBag.pageNumber = data.pageNumber;



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
        /**/{
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
        [HttpGet]
        public IActionResult AboutUs()
        {
            
            return View("About");
        }
    }
}
