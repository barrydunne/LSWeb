using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListIamGroups;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListIamGroups;

public class ListIamGroupsQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListIamGroupsQueryHandler CreateSut()
        => new(_client, NullLogger<ListIamGroupsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsGroups()
    {
        // Arrange
        var group = new IamGroup("developers", "arn:aws:iam::000000000000:group/developers", "AGPA1", "/", DateTimeOffset.UtcNow);
        _client
            .ListGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<IamGroup>>([group]));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Groups.Should().ContainSingle();
        var actual = result.Value.Groups[0];
        actual.GroupName.Should().Be(group.GroupName);
        actual.Arn.Should().Be(group.Arn);
        actual.GroupId.Should().Be(group.GroupId);
        actual.Path.Should().Be(group.Path);
        actual.CreateDate.Should().Be(group.CreateDate);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(new Error("list boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
