using Foundation.Application.Commands.UpdateParameterValue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateParameterValue;

public class UpdateParameterValueCommandValidatorTests
{
    private readonly UpdateParameterValueCommandValidator _sut =
        new(NullLogger<UpdateParameterValueCommandValidator>.Instance);

    private static UpdateParameterValueCommand Valid(
        string name = "/app/config/key",
        string value = "value")
        => new(name, value);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateParameterValueCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "/" + new string('a', 2048)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateParameterValueCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateParameterValueCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenValueEmpty_ReturnsErrorForValue()
    {
        var result = await _sut.ValidateAsync(
            Valid(value: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateParameterValueCommand.Value));
    }

    [Fact]
    public async Task ValidateAsync_WhenValueTooLong_ReturnsErrorForValue()
    {
        var result = await _sut.ValidateAsync(
            Valid(value: new string('a', 8193)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateParameterValueCommand.Value));
    }
}
