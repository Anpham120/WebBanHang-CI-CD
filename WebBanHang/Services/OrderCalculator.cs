namespace WebBanHang.Services;

public static class OrderCalculator
{
    // Sửa price * quantity thành price + quantity để demo Unit Test FAIL.
    public static decimal Calculate(decimal price, int quantity)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        return price * quantity;
    }
}
