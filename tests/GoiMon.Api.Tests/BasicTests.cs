using GoiMon.Shared.Models;
using Xunit;

namespace GoiMon.Api.Tests;

public class BasicTests
{
    [Fact]
    public void Product_Model_Defaults()
    {
        var p = new Product { Name = "Test", PriceCents = 100 };
        Assert.Equal("Test", p.Name);
        Assert.Equal(100, p.PriceCents);
    }
}
