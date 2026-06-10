using Foundation.Application.Commands.TestHttpRoute;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TestHttpRoute;

public class TestHttpRouteCommandValidatorTests
{
    private readonly TestHttpRouteCommandValidator _sut =
        new(NullLogger<TestHttpRouteCommandValidator>.Instance);

    private static TestHttpRouteCommand Valid(
        string apiId = "api-1",
        string stage = "$default",
        string method = "GET",
        string path = "/orders")
        => new(apiId, stage, method, path, null, null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenApiIdEmpty_ReturnsErrorForApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(apiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestHttpRouteCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageEmpty_ReturnsErrorForStage()
    {
        var result = await _sut.ValidateAsync(
            Valid(stage: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestHttpRouteCommand.Stage));
    }

    [Fact]
    public async Task ValidateAsync_WhenMethodInvalid_ReturnsErrorForMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(method: "FETCH"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestHttpRouteCommand.Method));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathEmpty_ReturnsErrorForPath()
    {
        var result = await _sut.ValidateAsync(
            Valid(path: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestHttpRouteCommand.Path));
    }
}
