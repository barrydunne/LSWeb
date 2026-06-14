using Foundation.Application.Commands.CreateStateMachine;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateStateMachine;

public class CreateStateMachineCommandValidatorTests
{
    private readonly CreateStateMachineCommandValidator _sut =
        new(NullLogger<CreateStateMachineCommandValidator>.Instance);

    private const string ValidDefinition = "{\"StartAt\":\"A\",\"States\":{\"A\":{\"Type\":\"Pass\",\"End\":true}}}";
    private const string RoleArn = "arn:aws:iam::000000000000:role/sfn";

    private static CreateStateMachineCommand Valid(
        string name = "orders",
        string? definition = null,
        string roleArn = RoleArn,
        string type = "STANDARD")
        => new(name, definition ?? ValidDefinition, roleArn, type);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("STANDARD")]
    [InlineData("EXPRESS")]
    public async Task ValidateAsync_WhenTypeSupported_IsValid(string type)
    {
        var result = await _sut.ValidateAsync(Valid(type: type), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStateMachineCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleNotAnArn_ReturnsErrorForRoleArn()
    {
        var result = await _sut.ValidateAsync(Valid(roleArn: "not-an-arn"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStateMachineCommand.RoleArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeUnsupported_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(Valid(type: "MAGIC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStateMachineCommand.Type));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{\"States\":{}}")]
    [InlineData("{\"StartAt\":\"A\"}")]
    [InlineData("{\"StartAt\":1,\"States\":{\"A\":{}}}")]
    [InlineData("{\"StartAt\":\"A\",\"States\":[]}")]
    [InlineData("[1,2,3]")]
    public async Task ValidateAsync_WhenDefinitionInvalid_ReturnsErrorForDefinition(string definition)
    {
        var result = await _sut.ValidateAsync(Valid(definition: definition), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateStateMachineCommand.Definition));
    }
}
