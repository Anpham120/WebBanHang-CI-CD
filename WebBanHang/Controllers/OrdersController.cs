using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Controllers;

public class OrdersController(ShopDbContext db) : Controller
{
    public IActionResult Index() => View(db.Orders.Include(o => o.Product).OrderByDescending(o => o.Id).ToList());

    private void LoadProducts(int selectedId = 0) =>
        ViewBag.Products = new SelectList(db.Products.OrderBy(p => p.Name).ToList(), "Id", "Name", selectedId);

    [HttpGet]
    public IActionResult Create()
    {
        LoadProducts();
        return View(new CreateOrderInput());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(CreateOrderInput input)
    {
        var product = db.Products.Find(input.ProductId);
        if (product is null) ModelState.AddModelError(nameof(input.ProductId), "Sản phẩm không tồn tại");
        if (product is not null && input.Quantity > product.Stock)
            ModelState.AddModelError(nameof(input.Quantity), "Không đủ hàng trong kho");

        if (!ModelState.IsValid)
        {
            LoadProducts(input.ProductId);
            return View(input);
        }

        product!.Stock -= input.Quantity;
        db.Orders.Add(new SaleOrder
        {
            ProductId = product.Id,
            Quantity = input.Quantity,
            UnitPrice = product.Price,
            CreatedAtUtc = DateTime.UtcNow
        });
        db.SaveChanges();
        TempData["Message"] = "Tạo đơn hàng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var order = db.Orders.Include(o => o.Product).FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        if (!order.IsCancelled)
        {
            order.IsCancelled = true;
            if (order.Product is not null) order.Product.Stock += order.Quantity;
            db.SaveChanges();
            TempData["Message"] = "Đã hủy đơn và hoàn lại tồn kho.";
        }
        return RedirectToAction(nameof(Index));
    }
}
