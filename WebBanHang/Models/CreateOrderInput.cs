using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models;

public class CreateOrderInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Chọn sản phẩm")]
    [Display(Name = "Sản phẩm")]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Số lượng từ 1 đến 100")]
    [Display(Name = "Số lượng")]
    public int Quantity { get; set; } = 1;
}
