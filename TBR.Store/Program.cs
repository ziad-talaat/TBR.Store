using Application.EF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TBL.Core.Contracts;
using TBL.Core.Contracts.ServiceContracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.EF.Repositories;
using TBL.EF.Service;
using static System.Net.WebRequestMethods;

namespace TBR.Store
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();

            builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr"));
            });
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
                
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAccountService, TBL.EF.Service.AccountService>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Customer/Account/AccessDenied";
                options.LoginPath = "/Customer/Account/LogIn";
                options.LogoutPath = "/Customer/Account/LogOut";
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
    }
}
