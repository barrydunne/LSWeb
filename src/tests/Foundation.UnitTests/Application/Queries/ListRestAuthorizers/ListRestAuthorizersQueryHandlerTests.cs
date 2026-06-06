using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.ListRestAuthorizers;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRestAuthorizers;

public class ListRestAuthorizersQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ListRestAuthorizersQueryHandler CreateSut()
        => new(_client, NullLogger<ListRestAuthorizersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAuthorizers()
    {
        // Arrange
        IReadOnlyList<RestAuthorizerSummary> authorizers =
        [
            new("auth-1", "pool-authorizer", "COGNITO_USER_POOLS"),
        ];
        _client
            .ListAuthorizersAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(authorizers)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestAuthorizersQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var authorizer = result.Value.Authorizers.Should().ContainSingle().Subject;
        authorizer.Id.Should().Be("auth-1");
        authorizer.Name.Should().Be("pool-authorizer");
        authorizer.Type.Should().Be("COGNITO_USER_POOLS");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListAuthorizersAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestAuthorizerSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestAuthorizersQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
