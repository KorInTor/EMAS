using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Model.Event;
using Service.Connection;
using Service;

namespace EMAS_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();// добавляем IDistributedMemoryCache
            builder.Services.AddSession();  // добавляем сервисы сессии
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<DataBaseClient>();
            builder.Services.AddHostedService<NewEventNotifier>();

            var app = builder.Build();

            app.UseSession();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapHub<EventsHub>("/events");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
