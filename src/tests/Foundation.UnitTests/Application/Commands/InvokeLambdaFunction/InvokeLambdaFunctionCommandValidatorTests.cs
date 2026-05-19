using Foundation.Application.Commands.InvokeLambdaFunction;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.InvokeLambdaFunction;

public class InvokeLambdaFunctionCommandValidatorTests
{
    private readonly InvokeLambdaFunctionCommandValidator _sut =
        new(NullLogger<InvokeLambdaFunctionCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new InvokeLambdaFunctionCommand("orders", "{}"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new InvokeLambdaFunctionCommand(string.Empty, "{}"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(InvokeLambdaFunctionCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPayloadNull_ReturnsErrorForPayload()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new InvokeLambdaFunctionCommand("orders", null!), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(InvokeLambdaFunctionCommand.Payload));
    }
}
