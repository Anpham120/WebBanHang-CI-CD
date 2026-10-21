using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Controllers;

public class ProductsController(ShopDbContext db) : Controller
{
    public IActionResult Index() => View(db.Products.Include(p => p.Category).OrderBy(p => p.Id).ToList());

    private void LoadCategories(int selectedId = 0) =>
        ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", selectedId);

    [HttpGet]
    public IActionResult Create()
    {
        LoadCategories();
        return View(new Product());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Product model)
    {
        if (!db.Categories.Any(c => c.Id == model.CategoryId))
            ModelState.AddModelError(nameof(Product.CategoryId), "Danh mục không hợp lệ");
        if (!ModelState.IsValid)
        {
            LoadCategories(model.CategoryId);
            return View(model);
        }
        db.Products.Add(new Product
        {
            Name = model.Name.Trim(), Price = model.Price,
            Stock = model.Stock, CategoryId = model.CategoryId
        });
        db.SaveChanges();
        TempData["Message"] = "Đã thêm sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = db.Products.Find(id);
        if (product is null) return NotFound();
        LoadCategories(product.CategoryId);
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Product model)
    {
        if (id != model.Id) return BadRequest();
        if (!db.Categories.Any(c => c.Id == model.CategoryId))
            ModelState.AddModelError(nameof(Product.CategoryId), "Danh mục không hợp lệ");
        if (!ModelState.IsValid)
        {
            LoadCategories(model.CategoryId);
            return View(model);
        }
        var item = db.Products.Find(id);
        if (item is null) return NotFound();
        item.Name = model.Name.Trim();
        item.Price = model.Price;
        item.Stock = model.Stock;
        item.CategoryId = model.CategoryId;
        db.SaveChanges();
        TempData["Message"] = "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var item = db.Products.Find(id);
        if (item is null) return NotFound();
        if (db.Orders.Any(o => o.ProductId == id))
        {
            TempData["Error"] = "Không thể xóa sản phẩm đã có đơn hàng.";
            return RedirectToAction(nameof(Index));
        }
        db.Products.Remove(item);
        db.SaveChanges();
        TempData["Message"] = "Đã xóa sản phẩm.";
        return RedirectToAction(nameof(Index));
    }
}
