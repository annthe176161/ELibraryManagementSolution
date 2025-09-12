using ELibraryManagement.Web.Services;
using ELibraryManagement.Web.Services.Interfaces;
using ELibraryManagement.Web.Services.Implementations;
using ELibraryManagement.Web.Middleware;

namespace ELibraryManagement.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add HttpClient for API calls
            builder.Services.AddHttpClient<IBookApiService, BookApiService>();
            builder.Services.AddScoped<IBookApiService, BookApiService>();

            // Add Auth services
            builder.Services.AddHttpClient<IAuthApiService, AuthApiService>();
            builder.Services.AddScoped<IAuthApiService, AuthApiService>();

            // Add Review services
            builder.Services.AddHttpClient<IReviewApiService, ReviewApiService>();
            builder.Services.AddScoped<IReviewApiService, ReviewApiService>();

            // Add Borrow services
            builder.Services.AddHttpClient<IBorrowApiService, BorrowApiService>();
            builder.Services.AddScoped<IBorrowApiService, BorrowApiService>();

            // Add Category services
            builder.Services.AddHttpClient<ICategoryApiService, CategoryApiService>();
            builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Add Session middleware
            app.UseSession();

            // Add authentication logging middleware for debugging
            app.UseMiddleware<AuthenticationLoggingMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
