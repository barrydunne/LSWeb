using Foundation.Application.Commands.UpdateStack;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateStack;

public class UpdateStackCommandValidatorTests
{
    private readonly UpdateStackCommandValidator _sut =
        new(NullLogger<UpdateStackCommandValidator>.Instance);

    private static UpdateStackCommand Valid(
        string stackName = "orders-stack",
        string templateBody = "{\"Resources\":{}}",
        IReadOnlyList<StackParameter>? parameters = null,
        IReadOnlyList<string>? capabilities = null)
        => new(stackName, templateBody,
            parameters ?? [new StackParameter("Env", "dev")],
            capabilities ?? ["CAPABILITY_IAM"]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameEmpty_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameTooLong_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTemplateBodyEmpty_ReturnsErrorForTemplateBody()
    {
        var result = await _sut.ValidateAsync(
            Valid(templateBody: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStackCommand.TemplateBody));
    }

    [Fact]
    public async Task ValidateAsync_WhenParameterKeyEmpty_ReturnsErrorForParameterKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(parameters: [new StackParameter(string.Empty, "dev")]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains(nameof(StackParameter.ParameterKey)));
    }

    [Fact]
    public async Task ValidateAsync_WhenCapabilityNotAllowed_ReturnsErrorForCapabilities()
    {
        var result = await _sut.ValidateAsync(
            Valid(capabilities: ["CAPABILITY_UNKNOWN"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains(nameof(UpdateStackCommand.Capabilities)));
    }
}
