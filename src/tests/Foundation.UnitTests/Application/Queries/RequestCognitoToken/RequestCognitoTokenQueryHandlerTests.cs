using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.RequestCognitoToken;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.RequestCognitoToken;

public class RequestCognitoTokenQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private RequestCognitoTokenQueryHandler CreateSut()
        => new(_client, NullLogger<RequestCognitoTokenQueryHandler>.Instance);

    private static TokenResult Token()
        => new(
            "access-token",
            "id-token",
            "refresh-token",
            "Bearer",
            3600,
            [new CognitoUserAttributeEntry("sub", "abc")]);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTokenAndForwardsArguments()
    {
        // Arrange
        _client
            .RequestTokenAsync("eu-west-1_abc123", "client-1", "alice", "Passw0rd!", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<TokenResult>>(Token()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RequestCognitoTokenQuery("eu-west-1_abc123", "client-1", "alice", "Passw0rd!"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.AccessToken.Should().Be("access-token");
        result.Value.Token.Claims.Should().ContainSingle(_ => _.Name == "sub");
        await _client.Received(1).RequestTokenAsync(
            "eu-west-1_abc123", "client-1", "alice", "Passw0rd!", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .RequestTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<TokenResult>>(new Error("auth boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RequestCognitoTokenQuery("eu-west-1_abc123", "client-1", "alice", "Passw0rd!"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("auth boom");
    }
}
