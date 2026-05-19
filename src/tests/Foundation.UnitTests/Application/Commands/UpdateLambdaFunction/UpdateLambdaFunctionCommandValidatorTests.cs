using Foundation.Application.Commands.UpdateLambdaFunction;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaFunction;

public class UpdateLambdaFunctionCommandValidatorTests
{
    private readonly UpdateLambdaFunctionCommandValidator _sut =
        new(NullLogger<UpdateLambdaFunctionCommandValidator>.Instance);

    private static UpdateLambdaFunctionCommand Command(
        string functionName = "orders",
        string runtime = "python3.12",
        string handler = "index.handler",
        string role = "arn:role",
        int memorySize = 256,
        int timeout = 30,
        string? zip = null)
        => new(functionName, runtime, handler, role, "desc", memorySize, timeout, zip);

    [Fact]
    public async Task ValidateAsync_WhenValidWithoutCode_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenValidWithCode_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(zip: "QkFTRTY0"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(Command(functionName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenRuntimeEmpty_ReturnsErrorForRuntime()
    {
        var result = await _sut.ValidateAsync(Command(runtime: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.Runtime));
    }

    [Fact]
    public async Task ValidateAsync_WhenHandlerEmpty_ReturnsErrorForHandler()
    {
        var result = await _sut.ValidateAsync(Command(handler: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.Handler));
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleEmpty_ReturnsErrorForRole()
    {
        var result = await _sut.ValidateAsync(Command(role: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.Role));
    }

    [Fact]
    public async Task ValidateAsync_WhenMemorySizeNotPositive_ReturnsErrorForMemorySize()
    {
        var result = await _sut.ValidateAsync(Command(memorySize: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.MemorySize));
    }

    [Fact]
    public async Task ValidateAsync_WhenTimeoutNotPositive_ReturnsErrorForTimeout()
    {
        var result = await _sut.ValidateAsync(Command(timeout: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.Timeout));
    }

    [Fact]
    public async Task ValidateAsync_WhenCodeSuppliedNotValidBase64_ReturnsErrorForZip()
    {
        var result = await _sut.ValidateAsync(Command(zip: "not valid base64!!!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateLambdaFunctionCommand.ZipFileBase64));
    }
}
