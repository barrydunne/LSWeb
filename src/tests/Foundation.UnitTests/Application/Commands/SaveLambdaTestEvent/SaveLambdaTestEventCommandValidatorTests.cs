using Foundation.Application.Commands.SaveLambdaTestEvent;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SaveLambdaTestEvent;

public class SaveLambdaTestEventCommandValidatorTests
{
    private readonly SaveLambdaTestEventCommandValidator _sut =
        new(NullLogger<SaveLambdaTestEventCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new SaveLambdaTestEventCommand("orders", "first", "{}"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new SaveLambdaTestEventCommand(string.Empty, "first", "{}"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SaveLambdaTestEventCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new SaveLambdaTestEventCommand("orders", string.Empty, "{}"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SaveLambdaTestEventCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenPayloadNull_ReturnsErrorForPayload()
    {
        var result = await _sut.ValidateAsync(
            new SaveLambdaTestEventCommand("orders", "first", null!), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SaveLambdaTestEventCommand.Payload));
    }
}
