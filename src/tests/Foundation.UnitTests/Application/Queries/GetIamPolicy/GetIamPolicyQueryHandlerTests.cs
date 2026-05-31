using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetIamPolicy;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetIamPolicy;

public class GetIamPolicyQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetIamPolicyQueryHandler CreateSut()
        => new(_client, NullLogger<GetIamPolicyQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsPolicyDetail()
    {
        // Arrange
        var detail = new IamPolicyDetail(
            "deploy-policy",
            "arn:aws:iam::000000000000:policy/deploy-policy",
            "ANPA1",
            "/team/",
            "v2",
            1,
            true,
            "Deploy policy",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "{\"Version\":\"2012-10-17\",\"Statement\":[]}",
            [new IamPolicyVersion("v2", true, DateTimeOffset.UtcNow), new IamPolicyVersion("v1", false, DateTimeOffset.UtcNow)],
            [new IamTag("team", "platform")]);
        _client
            .GetPolicyAsync(detail.Arn, Arg.Any<CancellationToken>())
            .Returns(Ok(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamPolicyQuery(detail.Arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var actual = result.Value.Policy;
        actual.PolicyName.Should().Be(detail.PolicyName);
        actual.Arn.Should().Be(detail.Arn);
        actual.PolicyId.Should().Be(detail.PolicyId);
        actual.Path.Should().Be(detail.Path);
        actual.DefaultVersionId.Should().Be(detail.DefaultVersionId);
        actual.AttachmentCount.Should().Be(detail.AttachmentCount);
        actual.IsAttachable.Should().Be(detail.IsAttachable);
        actual.Description.Should().Be(detail.Description);
        actual.CreateDate.Should().Be(detail.CreateDate);
        actual.UpdateDate.Should().Be(detail.UpdateDate);
        actual.DefaultVersionDocument.Should().Be(detail.DefaultVersionDocument);
        actual.Versions.Should().HaveCount(2);
        actual.Versions[0].VersionId.Should().Be("v2");
        actual.Versions[0].IsDefaultVersion.Should().BeTrue();
        actual.Versions[1].VersionId.Should().Be("v1");
        actual.Versions[1].IsDefaultVersion.Should().BeFalse();
        actual.Tags.Should().ContainSingle();
        actual.Tags[0].Key.Should().Be("team");
        actual.Tags[0].Value.Should().Be("platform");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetPolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Error("get boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamPolicyQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("get boom");
    }
}
