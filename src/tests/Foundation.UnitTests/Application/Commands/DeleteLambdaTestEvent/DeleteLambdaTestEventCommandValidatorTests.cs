using Foundation.Application.Commands.DeleteLambdaTestEvent;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLambdaTestEvent;

public class DeleteLambdaTestEventCommandValidatorTests
{
    private readonly DeleteLambdaTestEventCommandValidator _sut =
        new(NullLogger<DeleteLambdaTestEventCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaTestEventCommand("orders", "first"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaTestEventCommand(string.Empty, "first"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLambdaTestEventCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaTestEventCommand("orders", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLambdaTestEventCommand.Name));
    }
}
