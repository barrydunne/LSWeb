using Foundation.Application.Commands.UpdateLambdaEnvironment;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaEnvironment;

public class UpdateLambdaEnvironmentCommandValidatorTests
{
    private readonly UpdateLambdaEnvironmentCommandValidator _sut =
        new(NullLogger<UpdateLambdaEnvironmentCommandValidator>.Instance);

    private static IReadOnlyDictionary<string, string> EmptyVariables => new Dictionary<string, string>();

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new UpdateLambdaEnvironmentCommand("orders", EmptyVariables), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new UpdateLambdaEnvironmentCommand(string.Empty, EmptyVariables), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaEnvironmentCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenVariablesNull_ReturnsErrorForVariables()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new UpdateLambdaEnvironmentCommand("orders", null!), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaEnvironmentCommand.Variables));
    }
}
