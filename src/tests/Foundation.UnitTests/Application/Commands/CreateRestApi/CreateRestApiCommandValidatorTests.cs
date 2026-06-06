using Foundation.Application.Commands.CreateRestApi;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRestApi;

public class CreateRestApiCommandValidatorTests
{
    private readonly CreateRestApiCommandValidator _sut =
        new(NullLogger<CreateRestApiCommandValidator>.Instance);

    private static CreateRestApiCommand Valid(
        string name = "orders",
        IReadOnlyList<string>? endpointConfigurationTypes = null)
        => new(name, "desc", "1.0", "HEADER", endpointConfigurationTypes ?? ["REGIONAL"]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEndpointTypesEmpty_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(endpointConfigurationTypes: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 1025)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenEndpointTypeInvalid_ReturnsErrorForEndpointConfigurationTypes()
    {
        var result = await _sut.ValidateAsync(
            Valid(endpointConfigurationTypes: ["INVALID"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.StartsWith(nameof(CreateRestApiCommand.EndpointConfigurationTypes)));
    }
}
