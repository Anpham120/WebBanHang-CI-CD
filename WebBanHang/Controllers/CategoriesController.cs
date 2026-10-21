using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Controllers;

public class CategoriesController(ShopDbContext db) : Controller
{
    public IActionResult Index() => View(db.Categories.Include(c => c.Products).OrderBy(c => c.Id).ToList());

    [HttpGet]
    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Category model)
    {
        if (!ModelState.IsValid) return View(model);
        db.Categories.Add(new Category { Name = model.Name.Trim() });
        db.SaveChanges();
        TempData["Message"] = "Đã thêm danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = db.Categories.Find(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Category model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var item = db.Categories.Find(id);
        if (item is null) return NotFound();
        item.Name = model.Name.Trim();
        db.SaveChanges();
        TempData["Message"] = "Đã cập nhật danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var item = db.Categories.Find(id);
        if (item is null) return NotFound();
        if (db.Products.Any(p => p.CategoryId == id))
        {
            TempData["Error"] = "Không thể xóa danh mục đang có sản phẩm.";
            return RedirectToAction(nameof(Index));
        }
        db.Categories.Remove(item);
        db.SaveChanges();
        TempData["Message"] = "Đã xóa danh mục.";
        return RedirectToAction(nameof(Index));
    }
}
