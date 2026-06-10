using Foundation.Application.Commands.UpdateHttpIntegration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateHttpIntegration;

public class UpdateHttpIntegrationCommandValidatorTests
{
    private readonly UpdateHttpIntegrationCommandValidator _sut =
        new(NullLogger<UpdateHttpIntegrationCommandValidator>.Instance);

    private static UpdateHttpIntegrationCommand Valid(
        string apiId = "abc123",
        string integrationId = "int1",
        string integrationType = "HTTP_PROXY",
        string? integrationUri = "https://example.test")
        => new(apiId, integrationId, integrationType, "GET", integrationUri, "1.0", "proxy");

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpIntegrationCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenIntegrationIdEmpty_ReturnsErrorForIntegrationId()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpIntegrationCommand.IntegrationId));
    }

    [Fact]
    public async Task ValidateAsync_WhenIntegrationTypeEmpty_ReturnsErrorForIntegrationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpIntegrationCommand.IntegrationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenIntegrationTypeInvalid_ReturnsErrorForIntegrationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "GRPC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpIntegrationCommand.IntegrationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenUriMissingForNonMockType_ReturnsErrorForIntegrationUri()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "HTTP_PROXY", integrationUri: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpIntegrationCommand.IntegrationUri));
    }

    [Fact]
    public async Task ValidateAsync_WhenUriMissingForMockType_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(integrationType: "MOCK", integrationUri: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }
}
