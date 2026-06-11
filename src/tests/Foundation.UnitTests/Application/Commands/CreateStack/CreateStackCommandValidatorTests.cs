using Foundation.Application.Commands.CreateStack;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateStack;

public class CreateStackCommandValidatorTests
{
    private readonly CreateStackCommandValidator _sut =
        new(NullLogger<CreateStackCommandValidator>.Instance);

    private static CreateStackCommand Valid(
        string stackName = "orders-stack",
        string? templateBody = "{\"Resources\":{}}",
        string? templateUrl = null,
        IReadOnlyList<StackParameter>? parameters = null,
        IReadOnlyList<string>? capabilities = null)
        => new(stackName, templateBody, templateUrl,
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameTooLong_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNeitherTemplateBodyNorUrlProvided_ReturnsErrorForTemplate()
    {
        var result = await _sut.ValidateAsync(
            Valid(templateBody: string.Empty, templateUrl: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == "Template");
    }

    [Fact]
    public async Task ValidateAsync_WhenBothTemplateBodyAndUrlProvided_ReturnsErrorForTemplate()
    {
        var result = await _sut.ValidateAsync(
            Valid(templateBody: "{\"Resources\":{}}", templateUrl: "https://example.s3.amazonaws.com/template.json"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == "Template");
    }

    [Fact]
    public async Task ValidateAsync_WhenOnlyTemplateUrlProvided_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(templateBody: null, templateUrl: "https://example.s3.amazonaws.com/template.json"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
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
        result.Errors.Should().Contain(_ => _.PropertyName.Contains(nameof(CreateStackCommand.Capabilities)));
    }
}
