using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nhập tên danh mục")]
    [StringLength(80)]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
