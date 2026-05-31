using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListIamRoles;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListIamRoles;

public class ListIamRolesQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListIamRolesQueryHandler CreateSut()
        => new(_client, NullLogger<ListIamRolesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRoles()
    {
        // Arrange
        var role = new IamRole(
            "deploy-role",
            "arn:aws:iam::000000000000:role/deploy-role",
            "AROA1",
            "/",
            DateTimeOffset.UtcNow,
            "Deploy role");
        _client
            .ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<IamRole>>([role]));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamRolesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Roles.Should().ContainSingle();
        var actual = result.Value.Roles[0];
        actual.RoleName.Should().Be(role.RoleName);
        actual.Arn.Should().Be(role.Arn);
        actual.RoleId.Should().Be(role.RoleId);
        actual.Path.Should().Be(role.Path);
        actual.CreateDate.Should().Be(role.CreateDate);
        actual.Description.Should().Be(role.Description);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(new Error("list boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListIamRolesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
