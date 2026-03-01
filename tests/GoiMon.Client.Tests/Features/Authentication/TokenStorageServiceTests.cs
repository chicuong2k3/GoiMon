using System;
using Xunit;
using GoiMon.Client.Features.Authentication.Models;
using GoiMon.Client.Features.Authentication.Services;

namespace GoiMon.Client.Tests.Features.Authentication;

public class TokenStorageServiceTests
{
    private readonly TokenStorageService _service;

    public TokenStorageServiceTests()
    {
        _service = new TokenStorageService();
    }

    [Fact]
    public void SetToken_StoresTokenSuccessfully()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

        // Act
        _service.SetToken(token);

        // Assert
        var storedToken = _service.GetToken();
        Assert.NotNull(storedToken);
        Assert.Equal(token, storedToken);
    }

    [Fact]
    public void GetToken_ReturnsNullWhenNotSet()
    {
        // Act
        var token = _service.GetToken();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void ClearToken_RemovesToken()
    {
        // Arrange
        var token = "test-jwt-token";
        _service.SetToken(token);

        // Act
        _service.ClearToken();

        // Assert
        Assert.Null(_service.GetToken());
    }

    [Fact]
    public void HasToken_ReturnsTrueWhenTokenExists()
    {
        // Arrange
        _service.SetToken("test-token");

        // Act
        var hasToken = _service.HasToken();

        // Assert
        Assert.True(hasToken);
    }

    [Fact]
    public void HasToken_ReturnsFalseWhenTokenNotExists()
    {
        // Act
        var hasToken = _service.HasToken();

        // Assert
        Assert.False(hasToken);
    }
}
