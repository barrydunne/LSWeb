using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListIamUsers;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListIamUsers;

public class ListIamUsersQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListIamUsersQueryHandler CreateSut()
        => new(_client, NullLogger<ListIamUsersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUsers()
    {
        // Arrange
        IReadOnlyList<IamUser> users =
        [
            new("alice", "arn:aws:iam::000000000000:user/alice", "AID1", "/", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(users)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamUsersQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = result.Value.Users.Should().ContainSingle().Subject;
        user.UserName.Should().Be("alice");
        user.Arn.Should().Be("arn:aws:iam::000000000000:user/alice");
        user.UserId.Should().Be("AID1");
        user.Path.Should().Be("/");
        user.CreateDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamUser>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamUsersQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
