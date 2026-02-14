using Application.EF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Stripe;
using TBL.Core.Contracts;
using TBL.Core.Contracts.ServiceContracts;
using TBL.Core.Enums;
using TBL.Core.Models;
using TBL.EF.DBIntializer;
using TBL.EF.Repositories;
using TBL.EF.Service;
using static System.Net.WebRequestMethods;

namespace TBR.Store
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            builder.Services.AddSession();

            builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")
        //            ,
        //sqlOptions =>
        //{
        //    sqlOptions.EnableRetryOnFailure(
        //        maxRetryCount: 15,
        //        maxRetryDelay: TimeSpan.FromSeconds(30),
        //        errorNumbersToAdd: null
        //    );
        //}
        );
            });
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
                
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAccountService, TBL.EF.Service.AccountService>();
            builder.Services.AddScoped<IDbIntializer, DbIntializer>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Customer/Account/AccessDenied";
                options.LoginPath = "/Customer/Account/LogIn";
                options.LogoutPath = "/Customer/Account/LogOut";
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("NotAuthorized", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return !context.User.Identity.IsAuthenticated;
                    });
                });
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            await SeedDataBase();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
            app.Run();

            async Task SeedDataBase()
            {
                using (var scope = app.Services.CreateScope())
                {
                   var  dbIntilaizer= scope.ServiceProvider.GetRequiredService<IDbIntializer>();

                   await  dbIntilaizer.Intialize();
                }
            }
        }
    }
}
