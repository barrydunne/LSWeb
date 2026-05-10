using Foundation.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace Foundation.UnitTests.Middleware;

public class CorrelationMiddlewareTests
{
    private const string HeaderName = "X-Correlation-ID";

    private bool _nextCalled;

    [Fact]
    public async Task InvokeAsync_NoInboundHeader_GeneratesCorrelationId()
    {
        // Arrange
        var sut = new CorrelationMiddleware(Next);
        var context = new DefaultHttpContext();

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        var written = context.Response.Headers[HeaderName].ToString();
        written.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(written, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_InboundHeaderPresent_HonoursExistingCorrelationId()
    {
        // Arrange
        var sut = new CorrelationMiddleware(Next);
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "existing-id";

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.Headers[HeaderName].ToString().Should().Be("existing-id");
    }

    private Task Next(HttpContext context)
    {
        _nextCalled = true;
        return Task.CompletedTask;
    }
}
