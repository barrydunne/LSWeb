using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.ListHttpAuthorizers;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHttpAuthorizers;

public class ListHttpAuthorizersQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ListHttpAuthorizersQueryHandler CreateSut()
        => new(_client, NullLogger<ListHttpAuthorizersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAuthorizers()
    {
        // Arrange
        IReadOnlyList<HttpAuthorizerSummary> authorizers =
        [
            new("auth1", "jwt-authorizer", "JWT"),
        ];
        _client
            .ListAuthorizersAsync("abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(authorizers)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpAuthorizersQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var authorizer = result.Value.Authorizers.Should().ContainSingle().Subject;
        authorizer.AuthorizerId.Should().Be("auth1");
        authorizer.Name.Should().Be("jwt-authorizer");
        authorizer.AuthorizerType.Should().Be("JWT");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListAuthorizersAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpAuthorizerSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpAuthorizersQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
