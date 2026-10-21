using WebBanHang.Models;

namespace WebBanHang.Data;

public static class SeedData
{
    public static void Initialize(ShopDbContext db)
    {
        db.Database.EnsureCreated(); // Demo: cấu trúc DB cố định; bản thật nên dùng EF Migrations.
        if (db.Categories.Any()) return;

        var electronics = new Category { Name = "Điện tử" };
        var office = new Category { Name = "Văn phòng" };
        db.Categories.AddRange(electronics, office);
        db.Products.AddRange(
            new Product { Name = "Bàn phím cơ", Price = 750000m, Stock = 20, Category = electronics },
            new Product { Name = "Chuột không dây", Price = 350000m, Stock = 35, Category = electronics },
            new Product { Name = "Sổ ghi chép", Price = 45000m, Stock = 80, Category = office }
        );
        db.SaveChanges();
    }
}
