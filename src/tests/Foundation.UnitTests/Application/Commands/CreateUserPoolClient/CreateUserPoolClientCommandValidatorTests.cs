using Foundation.Application.Commands.CreateUserPoolClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateUserPoolClient;

public class CreateUserPoolClientCommandValidatorTests
{
    private readonly CreateUserPoolClientCommandValidator _sut =
        new(NullLogger<CreateUserPoolClientCommandValidator>.Instance);

    private static CreateUserPoolClientCommand Valid(
        string userPoolId = "eu-west-1_abc123",
        string clientName = "web",
        IReadOnlyList<string>? allowedOAuthFlows = null)
        => new(
            userPoolId,
            clientName,
            false,
            [],
            allowedOAuthFlows ?? ["code"],
            [],
            [],
            false);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            Valid(userPoolId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolClientCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientNameEmpty_ReturnsErrorForClientName()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolClientCommand.ClientName));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientNameTooLong_ReturnsErrorForClientName()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientName: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolClientCommand.ClientName));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientNameContainsInvalidCharacters_ReturnsErrorForClientName()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientName: "bad/name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolClientCommand.ClientName));
    }

    [Fact]
    public async Task ValidateAsync_WhenOAuthFlowInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowedOAuthFlows: ["password"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Allowed OAuth flows"));
    }
}
