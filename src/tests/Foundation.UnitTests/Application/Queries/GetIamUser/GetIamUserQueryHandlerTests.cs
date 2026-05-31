using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetIamUser;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetIamUser;

public class GetIamUserQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetIamUserQueryHandler CreateSut()
        => new(_client, NullLogger<GetIamUserQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUserDetail()
    {
        // Arrange
        var detail = new IamUserDetail(
            "alice",
            "arn:aws:iam::000000000000:user/alice",
            "AID1",
            "/",
            DateTimeOffset.UnixEpoch,
            ["developers"],
            [new IamAttachedPolicy("ReadOnly", "arn:aws:iam::aws:policy/ReadOnly")],
            ["inline-policy"],
            [new IamAccessKey("AKIA1", "Active", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, "s3", "eu-west-1")],
            [new IamTag("env", "dev")],
            "arn:aws:iam::aws:policy/Boundary");
        _client
            .GetUserAsync("alice", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamUserQuery("alice"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = result.Value.User;
        user.UserName.Should().Be("alice");
        user.Arn.Should().Be("arn:aws:iam::000000000000:user/alice");
        user.UserId.Should().Be("AID1");
        user.Path.Should().Be("/");
        user.CreateDate.Should().Be(DateTimeOffset.UnixEpoch);
        user.Groups.Should().ContainSingle().Which.Should().Be("developers");
        user.InlinePolicyNames.Should().ContainSingle().Which.Should().Be("inline-policy");
        var policy = user.AttachedPolicies.Should().ContainSingle().Subject;
        policy.PolicyName.Should().Be("ReadOnly");
        policy.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
        var key = user.AccessKeys.Should().ContainSingle().Subject;
        key.AccessKeyId.Should().Be("AKIA1");
        key.Status.Should().Be("Active");
        key.CreateDate.Should().Be(DateTimeOffset.UnixEpoch);
        key.LastUsedDate.Should().Be(DateTimeOffset.UnixEpoch);
        key.LastUsedService.Should().Be("s3");
        key.LastUsedRegion.Should().Be("eu-west-1");
        var tag = user.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("env");
        tag.Value.Should().Be("dev");
        user.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
    }

    [Fact]
    public async Task Handle_WhenUserMissing_PropagatesError()
    {
        // Arrange
        _client
            .GetUserAsync("ghost", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamUserDetail>>(new Error("The user with name ghost cannot be found.")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamUserQuery("ghost"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("The user with name ghost cannot be found.");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetUserAsync("alice", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamUserDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamUserQuery("alice"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
