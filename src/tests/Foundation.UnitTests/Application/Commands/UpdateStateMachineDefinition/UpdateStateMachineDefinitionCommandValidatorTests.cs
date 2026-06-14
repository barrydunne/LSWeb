using Foundation.Application.Commands.UpdateStateMachineDefinition;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateStateMachineDefinition;

public class UpdateStateMachineDefinitionCommandValidatorTests
{
    private readonly UpdateStateMachineDefinitionCommandValidator _sut =
        new(NullLogger<UpdateStateMachineDefinitionCommandValidator>.Instance);

    private const string ValidDefinition = "{\"StartAt\":\"A\",\"States\":{\"A\":{\"Type\":\"Pass\",\"End\":true}}}";
    private const string Arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders";

    private static UpdateStateMachineDefinitionCommand Valid(
        string arn = Arn, string? definition = null)
        => new(arn, definition ?? ValidDefinition);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenArnNotValid_ReturnsErrorForArn()
    {
        var result = await _sut.ValidateAsync(Valid(arn: "nope"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStateMachineDefinitionCommand.StateMachineArn));
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateStateMachineDefinitionCommand.Definition));
    }
}
