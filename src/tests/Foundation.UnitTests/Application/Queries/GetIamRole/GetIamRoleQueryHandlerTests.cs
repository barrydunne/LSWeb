using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetIamRole;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetIamRole;

public class GetIamRoleQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetIamRoleQueryHandler CreateSut()
        => new(_client, NullLogger<GetIamRoleQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRoleDetail()
    {
        // Arrange
        var detail = new IamRoleDetail(
            "deploy-role",
            "arn:aws:iam::000000000000:role/deploy-role",
            "AROA1",
            "/team/",
            DateTimeOffset.UtcNow,
            "Deploy role",
            3600,
            "{\"Version\":\"2012-10-17\",\"Statement\":[]}",
            [new IamAttachedPolicy("ReadOnlyAccess", "arn:aws:iam::aws:policy/ReadOnlyAccess")],
            [new IamInlinePolicy("inline", "{\"Version\":\"2012-10-17\"}")],
            [new IamTag("team", "platform")],
            "arn:aws:iam::aws:policy/Boundary");
        _client
            .GetRoleAsync("deploy-role", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamRoleDetail?>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleQuery("deploy-role"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var actual = result.Value.Role;
        actual.Should().NotBeNull();
        actual!.RoleName.Should().Be(detail.RoleName);
        actual.Arn.Should().Be(detail.Arn);
        actual.RoleId.Should().Be(detail.RoleId);
        actual.Path.Should().Be(detail.Path);
        actual.CreateDate.Should().Be(detail.CreateDate);
        actual.Description.Should().Be(detail.Description);
        actual.MaxSessionDuration.Should().Be(detail.MaxSessionDuration);
        actual.AssumeRolePolicyDocument.Should().Be(detail.AssumeRolePolicyDocument);
        actual.AttachedPolicies.Should().ContainSingle();
        actual.AttachedPolicies[0].PolicyName.Should().Be("ReadOnlyAccess");
        actual.AttachedPolicies[0].PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnlyAccess");
        actual.InlinePolicies.Should().ContainSingle();
        actual.InlinePolicies[0].PolicyName.Should().Be("inline");
        actual.InlinePolicies[0].PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        actual.Tags.Should().ContainSingle();
        actual.Tags[0].Key.Should().Be("team");
        actual.Tags[0].Value.Should().Be("platform");
        actual.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Error("get boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("get boom");
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsSuccessWithNullRole()
    {
        // Arrange
        _client
            .GetRoleAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamRoleDetail?>>((IamRoleDetail?)null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().BeNull();
    }
}
