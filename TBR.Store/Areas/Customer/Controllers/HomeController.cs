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
        public async Task<IActionResult> Index(string ?searchBy,string?searchValue,string ?sortBy,string ?value ,bool isAssending=true,int pageNumber=1)
        {
            

            Pagination<Product> pageDetails=  _unitOfWork.Products.GetAllSortedAndFilterdInPage(searchBy,searchValue,sortBy, value, isAssending,pageNumber);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchValue = searchValue;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentOrder = isAssending;
            ViewBag.CurrentCategory = value;
            ViewBag.SearchItems = new List<SelectListItem>
            {
              new SelectListItem { Value = nameof(Product.Title), Text = "Title" , Selected = (searchBy == nameof(Product.Title))},
              new SelectListItem { Value = nameof(Product.ISBN), Text = "ISBN" ,Selected = (searchBy == nameof(Product.ISBN))},
              new SelectListItem { Value = nameof(Product.Author), Text = "Author",Selected = (searchBy == nameof(Product.Author)) },
              new SelectListItem { Value = nameof(Product.Price), Text = "Price",Selected = (searchBy == nameof(Product.Price)) },
            };




            var categoriesNames =await _unitOfWork.Category.GetCategoriesName();

            ViewBag.FilterCategories = categoriesNames.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (value == x)
            }).ToList();


            
            return View(pageDetails);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            Product ?product = await _unitOfWork.Products.GetSpecific(x => x.Id == id, false, new[] {nameof(Product.Category)});
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
                Count =1,
                ProductId=id,
            };

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
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Vote(Voting voteType,int ProductId)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }



            var existingVote = await _unitOfWork.Vote.GetSpecificVote(userId, ProductId);

            if (existingVote != null)
            {
                if (voteType == existingVote.VoteType)
                {
                    _unitOfWork.Vote.Remove(existingVote);

                }
                else
                {
                    existingVote.VoteType = voteType;
                    existingVote.VotingTime = DateTime.Now;
                    _unitOfWork.Vote.Update(existingVote);

                }
                await _unitOfWork.CompleteAsync();

            }

            else
            {
                UserProduct_Voting newVote = new UserProduct_Voting()
                {
                    ProductId = ProductId,
                    UserId = userId,
                    VotingTime = DateTime.Now,
                    VoteType = voteType,
                };
                try
                {
                   await  _unitOfWork.Vote.AddAsync(newVote);
                    await _unitOfWork.CompleteAsync();
                }
                catch(DbUpdateException ex)
                {
                    TempData["Error"] = "some error while voting try later";
                }
            }


            return RedirectToAction("Details",  new { id = ProductId });
        }

        
    }
}
