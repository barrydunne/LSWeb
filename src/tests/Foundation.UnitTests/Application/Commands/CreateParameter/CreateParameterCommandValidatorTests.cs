using Foundation.Application.Commands.CreateParameter;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateParameter;

public class CreateParameterCommandValidatorTests
{
    private readonly CreateParameterCommandValidator _sut =
        new(NullLogger<CreateParameterCommandValidator>.Instance);

    private static CreateParameterCommand Valid(
        string name = "/app/config/key",
        string type = "String",
        string value = "value",
        string? description = null)
        => new(name, type, value, description);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionProvided_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: "primary config value"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("StringList")]
    [InlineData("SecureString")]
    public async Task ValidateAsync_WhenTypeIsAllowed_IsValid(string type)
    {
        var result = await _sut.ValidateAsync(
            Valid(type: type), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "/" + new string('a', 2048)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeEmpty_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(
            Valid(type: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Type));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeNotAllowed_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(
            Valid(type: "Number"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Type));
    }

    [Fact]
    public async Task ValidateAsync_WhenValueEmpty_ReturnsErrorForValue()
    {
        var result = await _sut.ValidateAsync(
            Valid(value: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Value));
    }

    [Fact]
    public async Task ValidateAsync_WhenValueTooLong_ReturnsErrorForValue()
    {
        var result = await _sut.ValidateAsync(
            Valid(value: new string('a', 8193)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Value));
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionTooLong_ReturnsErrorForDescription()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: new string('a', 1025)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateParameterCommand.Description));
    }
}
