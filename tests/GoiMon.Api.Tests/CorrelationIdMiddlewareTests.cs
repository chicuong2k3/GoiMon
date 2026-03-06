using System;
using GoiMon.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace GoiMon.Api.Tests;

public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    [Fact]
    public async Task InvokeAsync_WhenHeaderAbsent_GeneratesNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(innerHttpContext => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert — middleware generates a GUID and stores it in HttpContext.Items
        Assert.True(context.Items.ContainsKey("CorrelationId"));
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderPresent_UsesExistingCorrelationId()
    {
        // Arrange
        var existingId = "test-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = existingId;
        var middleware = new CorrelationIdMiddleware(innerHttpContext => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(existingId, context.Request.Headers[CorrelationIdHeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_SetsCorrelationIdInHttpContextItems()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(innerHttpContext => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey("CorrelationId"));
        Assert.False(string.IsNullOrWhiteSpace(context.Items["CorrelationId"]?.ToString()));
    }
}
