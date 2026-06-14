using Foundation.Application.Commands.CreateLambdaFunctionUrl;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateLambdaFunctionUrl;

public class CreateLambdaFunctionUrlCommandValidatorTests
{
    private readonly CreateLambdaFunctionUrlCommandValidator _sut =
        new(NullLogger<CreateLambdaFunctionUrlCommandValidator>.Instance);

    [Theory]
    [InlineData("NONE")]
    [InlineData("AWS_IAM")]
    public async Task ValidateAsync_WhenValid_IsValid(string authType)
    {
        var result = await _sut.ValidateAsync(
            new CreateLambdaFunctionUrlCommand("orders", authType), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLambdaFunctionUrlCommand(string.Empty, "NONE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionUrlCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthTypeUnknown_ReturnsErrorForAuthType()
    {
        var result = await _sut.ValidateAsync(
            new CreateLambdaFunctionUrlCommand("orders", "PUBLIC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionUrlCommand.AuthType));
    }
}
