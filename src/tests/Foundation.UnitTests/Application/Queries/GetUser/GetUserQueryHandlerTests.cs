using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.GetUser;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetUser;

public class GetUserQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private GetUserQueryHandler CreateSut()
        => new(_client, NullLogger<GetUserQueryHandler>.Instance);

    private static CognitoUserDetail Detail()
        => new(
            "alice",
            "CONFIRMED",
            true,
            [new CognitoUserAttributeEntry("email", "alice@example.com")],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUser()
    {
        // Arrange
        _client
            .GetUserAsync("eu-west-1_abc123", "alice", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserQuery("eu-west-1_abc123", "alice"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Username.Should().Be("alice");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserQuery("eu-west-1_abc123", "alice"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
