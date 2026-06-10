using Foundation.Application.Commands.TestInvokeRestMethod;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TestInvokeRestMethod;

public class TestInvokeRestMethodCommandValidatorTests
{
    private readonly TestInvokeRestMethodCommandValidator _sut =
        new(NullLogger<TestInvokeRestMethodCommandValidator>.Instance);

    private static TestInvokeRestMethodCommand Valid(
        string restApiId = "api-1",
        string resourceId = "res-2",
        string httpMethod = "POST",
        string pathWithQueryString = "/orders")
        => new(
            restApiId,
            resourceId,
            httpMethod,
            pathWithQueryString,
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            null,
            new Dictionary<string, string>());

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRestApiIdEmpty_ReturnsErrorForRestApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(restApiId: string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestInvokeRestMethodCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenResourceIdEmpty_ReturnsErrorForResourceId()
    {
        var result = await _sut.ValidateAsync(
            Valid(resourceId: string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestInvokeRestMethodCommand.ResourceId));
    }

    [Fact]
    public async Task ValidateAsync_WhenHttpMethodInvalid_ReturnsErrorForHttpMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(httpMethod: "FETCH"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestInvokeRestMethodCommand.HttpMethod));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathEmpty_ReturnsErrorForPathWithQueryString()
    {
        var result = await _sut.ValidateAsync(
            Valid(pathWithQueryString: string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestInvokeRestMethodCommand.PathWithQueryString));
    }

    [Fact]
    public async Task ValidateAsync_WhenHeadersNull_ReturnsErrorForHeaders()
    {
        var command = new TestInvokeRestMethodCommand(
            "api-1",
            "res-2",
            "GET",
            "/orders",
            null!,
            new Dictionary<string, string>(),
            null,
            new Dictionary<string, string>());

        var result = await _sut.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TestInvokeRestMethodCommand.Headers));
    }
}
