using Foundation.Application.Commands.CreateChangeSet;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateChangeSet;

public class CreateChangeSetCommandValidatorTests
{
    private readonly CreateChangeSetCommandValidator _sut =
        new(NullLogger<CreateChangeSetCommandValidator>.Instance);

    private static CreateChangeSetCommand Valid(
        string stackName = "orders-stack",
        string changeSetName = "add-queue",
        string changeSetType = "UPDATE",
        string templateBody = "{\"Resources\":{}}",
        IReadOnlyList<StackParameter>? parameters = null,
        IReadOnlyList<string>? capabilities = null)
        => new(stackName, changeSetName, changeSetType, templateBody,
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameEmpty_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.ChangeSetName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameTooLong_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.ChangeSetName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameHasInvalidCharacters_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: "1-bad"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.ChangeSetName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetTypeEmpty_ReturnsErrorForChangeSetType()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.ChangeSetType));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetTypeNotAllowed_ReturnsErrorForChangeSetType()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetType: "DELETE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.ChangeSetType));
    }

    [Fact]
    public async Task ValidateAsync_WhenTemplateBodyEmpty_ReturnsErrorForTemplateBody()
    {
        var result = await _sut.ValidateAsync(
            Valid(templateBody: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateChangeSetCommand.TemplateBody));
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
        result.Errors.Should().Contain(_ => _.PropertyName.Contains(nameof(CreateChangeSetCommand.Capabilities)));
    }
}
