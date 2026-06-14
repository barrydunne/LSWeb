using Foundation.Application.Commands.DeleteLambdaFunctionUrl;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLambdaFunctionUrl;

public class DeleteLambdaFunctionUrlCommandValidatorTests
{
    private readonly DeleteLambdaFunctionUrlCommandValidator _sut =
        new(NullLogger<DeleteLambdaFunctionUrlCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaFunctionUrlCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLambdaFunctionUrlCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLambdaFunctionUrlCommand.FunctionName));
    }
}
