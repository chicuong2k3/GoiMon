using GoiMon.Shared.Models;
using Xunit;

namespace GoiMon.Api.Tests;

public class BasicTests
{
    [Fact]
    public void Product_Model_Defaults()
    {
        var p = new Product { Name = "Test", Price = 1.00m };
        Assert.Equal("Test", p.Name);
        Assert.Equal(1.00m, p.Price);
    }
}
