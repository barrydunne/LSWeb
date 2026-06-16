using Foundation.Application.Commands.DeleteStateMachine;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteStateMachine;

public class DeleteStateMachineCommandValidatorTests
{
    private readonly DeleteStateMachineCommandValidator _sut =
        new(NullLogger<DeleteStateMachineCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteStateMachineCommand("arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenStateMachineArnEmpty_ReturnsErrorForStateMachineArn()
    {
        var result = await _sut.ValidateAsync(
            new DeleteStateMachineCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteStateMachineCommand.StateMachineArn));
    }
}
