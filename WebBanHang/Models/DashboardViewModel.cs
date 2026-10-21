namespace WebBanHang.Models;

public class DashboardViewModel
{
    public int ProductCount { get; set; }
    public int CategoryCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public string Version { get; set; } = "local";
    public List<SaleOrder> RecentOrders { get; set; } = new();
}
