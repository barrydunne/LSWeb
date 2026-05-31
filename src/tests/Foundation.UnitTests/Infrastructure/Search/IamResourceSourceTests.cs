using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Domain.Iam;
using Foundation.Domain.Search;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class IamResourceSourceTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private IamResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    private void StubAllEmpty()
    {
        _client.ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamUser>>([])));
        _client.ListGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamGroup>>([])));
        _client.ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamRole>>([])));
        _client.ListPoliciesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamPolicy>>([])));
    }

    [Fact]
    public void ServiceKey_IsIam()
        => CreateSut().ServiceKey.Should().Be("iam");

    [Fact]
    public async Task ListAsync_WhenAllTypesSucceed_MapsEachToATypePrefixedSearchEntry()
    {
        // Arrange
        StubAllEmpty();
        _client.ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamUser>>(
                [new("Alice", "arn:aws:iam::000000000000:user/Alice", "AIDA1", "/", null)])));
        _client.ListGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamGroup>>(
                [new("Admins", "arn:aws:iam::000000000000:group/Admins", "AGPA1", "/", null)])));
        _client.ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamRole>>(
                [new("LambdaExec", "arn:aws:iam::000000000000:role/LambdaExec", "AROA1", "/", null, null)])));
        _client.ListPoliciesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamPolicy>>(
                [new("ReadOnly", "arn:aws:iam::000000000000:policy/ReadOnly", "ANPA1", "/", "v1", 0, true, null, null, null)])));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEquivalentTo(
        [
            new SearchEntry("iam", "user/Alice", "Alice", "/services/iam/user/Alice"),
            new SearchEntry("iam", "group/Admins", "Admins", "/services/iam/group/Admins"),
            new SearchEntry("iam", "role/LambdaExec", "LambdaExec", "/services/iam/role/LambdaExec"),
            new SearchEntry("iam", "policy/ReadOnly", "ReadOnly", "/services/iam/policy/ReadOnly"),
        ]);
    }

    [Fact]
    public async Task ListAsync_ListsOnlyCustomerManagedPolicies()
    {
        // Arrange
        StubAllEmpty();
        var sut = CreateSut();

        // Act
        await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).ListPoliciesAsync(false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_EscapesResourceNamesInTheRoute()
    {
        // Arrange
        StubAllEmpty();
        _client.ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamRole>>(
                [new("My Role", "arn:aws:iam::000000000000:role/My Role", "AROA1", "/", null, null)])));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ResourceId.Should().Be("role/My Role");
        entry.DisplayName.Should().Be("My Role");
        entry.Route.Should().Be("/services/iam/role/My%20Role");
    }

    [Fact]
    public async Task ListAsync_WhenOneTypeFails_SkipsItButKeepsTheOthers()
    {
        // Arrange
        StubAllEmpty();
        _client.ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamUser>>>(new Error("boom")));
        _client.ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<IamRole>>(
                [new("LambdaExec", "arn:aws:iam::000000000000:role/LambdaExec", "AROA1", "/", null, null)])));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ResourceId.Should().Be("role/LambdaExec");
    }

    [Fact]
    public async Task ListAsync_WhenAllTypesFail_ReturnsEmptyList()
    {
        // Arrange
        _client.ListUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamUser>>>(new Error("boom")));
        _client.ListGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamGroup>>>(new Error("boom")));
        _client.ListRolesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamRole>>>(new Error("boom")));
        _client.ListPoliciesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<IamPolicy>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
