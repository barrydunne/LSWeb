using Foundation.Application.Commands.CreateHttpIntegration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpIntegration;

public class CreateHttpIntegrationCommandValidatorTests
{
    private readonly CreateHttpIntegrationCommandValidator _sut =
        new(NullLogger<CreateHttpIntegrationCommandValidator>.Instance);

    private static CreateHttpIntegrationCommand Valid(
        string apiId = "abc123",
        string integrationType = "HTTP_PROXY",
        string? integrationUri = "https://example.test")
        => new(apiId, integrationType, "GET", integrationUri, "1.0", "proxy");

    [Theory]
    [InlineData("AWS")]
    [InlineData("AWS_PROXY")]
    [InlineData("HTTP")]
    [InlineData("HTTP_PROXY")]
    [InlineData("MOCK")]
    public async Task ValidateAsync_WhenIntegrationTypeAllowed_IsValid(string integrationType)
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: integrationType), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenApiIdEmpty_ReturnsErrorForApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(apiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpIntegrationCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenIntegrationTypeEmpty_ReturnsErrorForIntegrationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpIntegrationCommand.IntegrationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenIntegrationTypeInvalid_ReturnsErrorForIntegrationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "GRPC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpIntegrationCommand.IntegrationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenUriMissingForNonMockType_ReturnsErrorForIntegrationUri()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "HTTP_PROXY", integrationUri: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpIntegrationCommand.IntegrationUri));
    }

    [Fact]
    public async Task ValidateAsync_WhenUriMissingForMockType_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "MOCK", integrationUri: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }
}
