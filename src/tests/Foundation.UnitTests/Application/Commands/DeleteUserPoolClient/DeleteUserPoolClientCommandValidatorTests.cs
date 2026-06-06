using Foundation.Application.Commands.DeleteUserPoolClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteUserPoolClient;

public class DeleteUserPoolClientCommandValidatorTests
{
    private readonly DeleteUserPoolClientCommandValidator _sut =
        new(NullLogger<DeleteUserPoolClientCommandValidator>.Instance);

    private static DeleteUserPoolClientCommand Valid(
        string userPoolId = "eu-west-1_abc123",
        string clientId = "client-1")
        => new(userPoolId, clientId);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserPoolClientCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientIdEmpty_ReturnsErrorForClientId()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserPoolClientCommand.ClientId));
    }
}
