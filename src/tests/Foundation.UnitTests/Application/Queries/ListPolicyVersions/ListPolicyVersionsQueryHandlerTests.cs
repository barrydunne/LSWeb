using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListPolicyVersions;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListPolicyVersions;

public class ListPolicyVersionsQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListPolicyVersionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListPolicyVersionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsVersions()
    {
        // Arrange
        var arn = "arn:aws:iam::000000000000:policy/deploy-policy";
        var version = new IamPolicyVersion("v1", true, DateTimeOffset.UtcNow);
        _client
            .ListPolicyVersionsAsync(arn, Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<IamPolicyVersion>>([version]));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListPolicyVersionsQuery(arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Versions.Should().ContainSingle();
        var actual = result.Value.Versions[0];
        actual.VersionId.Should().Be(version.VersionId);
        actual.IsDefaultVersion.Should().Be(version.IsDefaultVersion);
        actual.CreateDate.Should().Be(version.CreateDate);
        await _client.Received(1).ListPolicyVersionsAsync(arn, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListPolicyVersionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Error("list boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListPolicyVersionsQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
