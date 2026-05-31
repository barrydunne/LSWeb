using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListIamPolicies;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListIamPolicies;

public class ListIamPoliciesQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListIamPoliciesQueryHandler CreateSut()
        => new(_client, NullLogger<ListIamPoliciesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_WhenClientSucceeds_ReturnsPolicies(bool awsManaged)
    {
        // Arrange
        var policy = new IamPolicy(
            "deploy-policy",
            "arn:aws:iam::000000000000:policy/deploy-policy",
            "ANPA1",
            "/",
            "v1",
            0,
            true,
            "Deploy policy",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
        _client
            .ListPoliciesAsync(awsManaged, Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<IamPolicy>>([policy]));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamPoliciesQuery(awsManaged), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Policies.Should().ContainSingle();
        var actual = result.Value.Policies[0];
        actual.PolicyName.Should().Be(policy.PolicyName);
        actual.Arn.Should().Be(policy.Arn);
        actual.PolicyId.Should().Be(policy.PolicyId);
        actual.Path.Should().Be(policy.Path);
        actual.DefaultVersionId.Should().Be(policy.DefaultVersionId);
        actual.AttachmentCount.Should().Be(policy.AttachmentCount);
        actual.IsAttachable.Should().Be(policy.IsAttachable);
        actual.Description.Should().Be(policy.Description);
        actual.CreateDate.Should().Be(policy.CreateDate);
        actual.UpdateDate.Should().Be(policy.UpdateDate);
        await _client.Received(1).ListPoliciesAsync(awsManaged, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListPoliciesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new Error("list boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamPoliciesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
