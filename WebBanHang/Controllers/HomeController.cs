using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Controllers;

public class HomeController(ShopDbContext db) : Controller
{
    public IActionResult Index()
    {
        var orders = db.Orders.Include(o => o.Product)
            .OrderByDescending(o => o.CreatedAtUtc).ToList();
        return View(new DashboardViewModel
        {
            ProductCount = db.Products.Count(),
            CategoryCount = db.Categories.Count(),
            OrderCount = orders.Count(o => !o.IsCancelled),
            Revenue = orders.Where(o => !o.IsCancelled).Sum(o => o.Total),
            Version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "local",
            RecentOrders = orders.Take(5).ToList()
        });
    }

    public IActionResult Error() => View();
}
