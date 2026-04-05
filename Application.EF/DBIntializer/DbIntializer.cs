using Application.EF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;

namespace TBL.EF.DBIntializer
{
    public class DbIntializer : IDbIntializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ISearchTrie _trie;
        private readonly IUnitOfWork _unitOfWork;


        public DbIntializer(RoleManager<IdentityRole> roleManager,
            AppDbContext context, UserManager<ApplicationUser> userManager,ISearchTrie trie,
            IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _context = context; 
            _userManager = userManager;
            _trie = trie;
            _unitOfWork = unitOfWork;
        }



        public async  Task Intialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch(Exception ex)
            {

            }



            if (!await _roleManager.RoleExistsAsync(Roles.Role_Admin))
            {
                      await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Customer));
                      await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Employee));
                      await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Admin));
                      await _roleManager.CreateAsync(new IdentityRole(Roles.Role_Company));
                  ApplicationUser user = new ApplicationUser()
                  {
                      UserName = "zeyad",
                      Email = "zeyad@email.com",
                      PhoneNumber = "01025185992",
                      Address = "dala",
                      PostalCode = "12qe",
                  };
                
                
                      await _userManager.CreateAsync(user, "12345_qweRT");
                
                  await _userManager.AddToRoleAsync(user, Roles.Role_Admin);

            }
            return; 
        }
        public  void SeedAsync()
        {
            var names = _unitOfWork.Products.GetQueryy().AsNoTracking().Select(x => new {x.Title,x.ClickedCount}).ToList();
            foreach (var name in names)
                _trie.Insert(name.Title.ToLower(),name.ClickedCount);
        }
    }
}

                




