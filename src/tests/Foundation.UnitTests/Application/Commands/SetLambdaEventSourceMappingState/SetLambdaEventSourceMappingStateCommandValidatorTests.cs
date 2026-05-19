using Foundation.Application.Commands.SetLambdaEventSourceMappingState;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetLambdaEventSourceMappingState;

public class SetLambdaEventSourceMappingStateCommandValidatorTests
{
    private readonly SetLambdaEventSourceMappingStateCommandValidator _sut =
        new(NullLogger<SetLambdaEventSourceMappingStateCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new SetLambdaEventSourceMappingStateCommand("orders", "uuid-1", true),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new SetLambdaEventSourceMappingStateCommand(string.Empty, "uuid-1", true),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetLambdaEventSourceMappingStateCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenUuidEmpty_ReturnsErrorForUuid()
    {
        var result = await _sut.ValidateAsync(
            new SetLambdaEventSourceMappingStateCommand("orders", string.Empty, true),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetLambdaEventSourceMappingStateCommand.Uuid));
    }
}
