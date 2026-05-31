using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetIamGroup;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetIamGroup;

public class GetIamGroupQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetIamGroupQueryHandler CreateSut()
        => new(_client, NullLogger<GetIamGroupQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsGroupDetail()
    {
        // Arrange
        var detail = new IamGroupDetail(
            "developers",
            "arn:aws:iam::000000000000:group/developers",
            "AGPA1",
            "/team/",
            DateTimeOffset.UtcNow,
            ["alice", "bob"],
            [new IamAttachedPolicy("ReadOnlyAccess", "arn:aws:iam::aws:policy/ReadOnlyAccess")],
            [new IamInlinePolicy("inline", "{\"Version\":\"2012-10-17\"}")]);
        _client
            .GetGroupAsync("developers", Arg.Any<CancellationToken>())
            .Returns(Ok(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamGroupQuery("developers"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var actual = result.Value.Group;
        actual.GroupName.Should().Be(detail.GroupName);
        actual.Arn.Should().Be(detail.Arn);
        actual.GroupId.Should().Be(detail.GroupId);
        actual.Path.Should().Be(detail.Path);
        actual.CreateDate.Should().Be(detail.CreateDate);
        actual.Members.Should().BeEquivalentTo(detail.Members);
        actual.AttachedPolicies.Should().ContainSingle();
        actual.AttachedPolicies[0].PolicyName.Should().Be("ReadOnlyAccess");
        actual.AttachedPolicies[0].PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnlyAccess");
        actual.InlinePolicies.Should().ContainSingle();
        actual.InlinePolicies[0].PolicyName.Should().Be("inline");
        actual.InlinePolicies[0].PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetGroupAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Error("get boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamGroupQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("get boom");
    }
}
