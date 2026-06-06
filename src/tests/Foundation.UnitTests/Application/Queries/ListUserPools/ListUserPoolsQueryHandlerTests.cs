using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.ListUserPools;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListUserPools;

public class ListUserPoolsQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private ListUserPoolsQueryHandler CreateSut()
        => new(_client, NullLogger<ListUserPoolsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUserPools()
    {
        // Arrange
        IReadOnlyList<UserPoolSummary> userPools =
        [
            new("eu-west-1_abc123", "customers", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListUserPoolsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(userPools)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUserPoolsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserPools.Should().ContainSingle(_ => _.Id == "eu-west-1_abc123");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListUserPoolsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<UserPoolSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUserPoolsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
