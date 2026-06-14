using Foundation.Application.Commands.UpdateLambdaFunctionUrl;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaFunctionUrl;

public class UpdateLambdaFunctionUrlCommandValidatorTests
{
    private readonly UpdateLambdaFunctionUrlCommandValidator _sut =
        new(NullLogger<UpdateLambdaFunctionUrlCommandValidator>.Instance);

    [Theory]
    [InlineData("NONE")]
    [InlineData("AWS_IAM")]
    public async Task ValidateAsync_WhenValid_IsValid(string authType)
    {
        var result = await _sut.ValidateAsync(
            new UpdateLambdaFunctionUrlCommand("orders", authType), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(
            new UpdateLambdaFunctionUrlCommand(string.Empty, "NONE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionUrlCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthTypeUnknown_ReturnsErrorForAuthType()
    {
        var result = await _sut.ValidateAsync(
            new UpdateLambdaFunctionUrlCommand("orders", "PUBLIC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionUrlCommand.AuthType));
    }
}
