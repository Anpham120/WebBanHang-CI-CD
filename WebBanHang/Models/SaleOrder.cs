namespace WebBanHang.Models;

public class SaleOrder
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
    public decimal Total => Services.OrderCalculator.Calculate(UnitPrice, Quantity);
}
