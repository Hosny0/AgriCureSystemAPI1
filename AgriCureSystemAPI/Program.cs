using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using AgriCureSystemAPI.Utility.DBInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Scalar;
using Scalar.AspNetCore;
using Stripe;

namespace AgriCureSystemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(
                           option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                           );


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
              options =>
              {
                  options.Password.RequireNonAlphanumeric = false;
                  // options.SignIn.RequireConfirmedEmail = true;
                  options.SignIn.RequireConfirmedEmail = true;
              }
             )
             .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            //  builder.Services.ConfigureApplicationCookie(options =>
            // {
            //    options.LoginPath = "/Identity/Account/Login";
            //  options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //});

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IUserOTPRepository, UserOTPRepository>();
            builder.Services.AddScoped<IDBInitializer, DBInitializer>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
