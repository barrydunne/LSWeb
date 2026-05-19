using Foundation.Application.Commands.CreateLambdaFunction;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateLambdaFunction;

public class CreateLambdaFunctionCommandValidatorTests
{
    private readonly CreateLambdaFunctionCommandValidator _sut =
        new(NullLogger<CreateLambdaFunctionCommandValidator>.Instance);

    private static CreateLambdaFunctionCommand Command(
        string functionName = "orders",
        string runtime = "python3.12",
        string handler = "index.handler",
        string role = "arn:role",
        int memorySize = 256,
        int timeout = 30,
        string zip = "QkFTRTY0")
        => new(functionName, runtime, handler, role, "desc", memorySize, timeout, zip);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFunctionNameEmpty_ReturnsErrorForFunctionName()
    {
        var result = await _sut.ValidateAsync(Command(functionName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.FunctionName));
    }

    [Fact]
    public async Task ValidateAsync_WhenRuntimeEmpty_ReturnsErrorForRuntime()
    {
        var result = await _sut.ValidateAsync(Command(runtime: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.Runtime));
    }

    [Fact]
    public async Task ValidateAsync_WhenHandlerEmpty_ReturnsErrorForHandler()
    {
        var result = await _sut.ValidateAsync(Command(handler: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.Handler));
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleEmpty_ReturnsErrorForRole()
    {
        var result = await _sut.ValidateAsync(Command(role: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.Role));
    }

    [Fact]
    public async Task ValidateAsync_WhenMemorySizeNotPositive_ReturnsErrorForMemorySize()
    {
        var result = await _sut.ValidateAsync(Command(memorySize: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.MemorySize));
    }

    [Fact]
    public async Task ValidateAsync_WhenTimeoutNotPositive_ReturnsErrorForTimeout()
    {
        var result = await _sut.ValidateAsync(Command(timeout: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.Timeout));
    }

    [Fact]
    public async Task ValidateAsync_WhenZipNotValidBase64_ReturnsErrorForZip()
    {
        var result = await _sut.ValidateAsync(Command(zip: "not valid base64!!!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLambdaFunctionCommand.ZipFileBase64));
    }
}
