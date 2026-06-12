using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.ListUsers;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListUsers;

public class ListUsersQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private ListUsersQueryHandler CreateSut()
        => new(_client, NullLogger<ListUsersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUsers()
    {
        // Arrange
        IReadOnlyList<CognitoUserSummary> users =
        [
            new("alice", "CONFIRMED", true, DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListUsersAsync("eu-west-1_abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(users)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUsersQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Users.Should().ContainSingle(_ => _.Username == "alice");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListUsersAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<CognitoUserSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUsersQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
