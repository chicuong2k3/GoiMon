using System.Security.Claims;
using GoiMon.Api.Domain.Enums;
using GoiMon.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading.Tasks;

namespace GoiMon.Api.Tests.Authorization;

public class AuthorizationPolicyTests
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddAuthorizationCore();
        services.AddPolicyMatrix();

        var provider = services.BuildServiceProvider();
        _authorizationService = provider.GetRequiredService<IAuthorizationService>();
    }

    [Fact]
    public async Task Cashier_Cannot_Access_Manager_Endpoint_HardDelete()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, UserRole.Cashier.ToString())
        }, "mock"));

        // Act
        var result = await _authorizationService.AuthorizeAsync(user, Policies.Order.HardDelete);

        // Assert
        Assert.False(result.Succeeded, "Cashier should not be authorized to HardDelete orders.");
    }

    [Fact]
    public async Task Manager_Can_Access_Manager_Endpoint_HardDelete()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, UserRole.Manager.ToString())
        }, "mock"));

        // Act
        var result = await _authorizationService.AuthorizeAsync(user, Policies.Order.HardDelete);

        // Assert
        Assert.True(result.Succeeded, "Manager should be authorized to HardDelete orders.");
    }

    [Fact]
    public async Task Cashier_Cannot_Void_Order()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, UserRole.Cashier.ToString())
        }, "mock"));

        // Act
        var result = await _authorizationService.AuthorizeAsync(user, Policies.Order.Void);

        // Assert
        Assert.False(result.Succeeded, "Cashier should not be able to manually trigger Void order (needs Supervisor).");
    }

    [Fact]
    public async Task Supervisor_Can_Void_Order()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, UserRole.Supervisor.ToString())
        }, "mock"));

        // Act
        var result = await _authorizationService.AuthorizeAsync(user, Policies.Order.Void);

        // Assert
        Assert.True(result.Succeeded, "Supervisor should be able to Void order.");
    }
}
