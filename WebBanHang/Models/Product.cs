using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nhập tên sản phẩm")]
    [StringLength(120)]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Giá phải từ 0 trở lên")]
    [Display(Name = "Giá bán (VND)")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "Tồn kho phải từ 0 trở lên")]
    [Display(Name = "Tồn kho")]
    public int Stock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
    public ICollection<SaleOrder> Orders { get; set; } = new List<SaleOrder>();
}
