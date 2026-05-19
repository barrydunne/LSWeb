using Foundation.Application.Commands.DeleteLambdaFunction;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLambdaFunction;

public class DeleteLambdaFunctionCommandValidatorTests
{
    private readonly DeleteLambdaFunctionCommandValidator _sut =
        new(NullLogger<DeleteLambdaFunctionCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaFunctionCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaFunctionCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLambdaFunctionCommand.FunctionName));
    }
}
