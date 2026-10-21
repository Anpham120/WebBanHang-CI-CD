using WebBanHang.Services;
using Xunit;

namespace WebBanHang.Tests;

public class OrderCalculatorTests
{
    [Theory]
    [InlineData(100000, 2, 200000)]
    [InlineData(25000, 3, 75000)]
    [InlineData(0, 5, 0)]
    public void Calculate_ReturnsCorrectTotal(int price, int quantity, int expected)
    {
        Assert.Equal((decimal)expected, OrderCalculator.Calculate(price, quantity));
    }

    [Fact]
    public void Calculate_RejectsZeroQuantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderCalculator.Calculate(100m, 0));
    }

    [Fact]
    public void Calculate_RejectsNegativePrice()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderCalculator.Calculate(-1m, 2));
    }
}
