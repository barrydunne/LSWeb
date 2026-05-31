using Foundation.Application.Commands.StartExecution;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.StartExecution;

public class StartExecutionCommandValidatorTests
{
    private readonly StartExecutionCommandValidator _sut =
        new(NullLogger<StartExecutionCommandValidator>.Instance);

    private static StartExecutionCommand Valid(
        string stateMachineArn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow")
        => new(stateMachineArn, null, null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenStateMachineArnEmpty_ReturnsErrorForStateMachineArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(stateMachineArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(_ => _.PropertyName == nameof(StartExecutionCommand.StateMachineArn));
    }
}
