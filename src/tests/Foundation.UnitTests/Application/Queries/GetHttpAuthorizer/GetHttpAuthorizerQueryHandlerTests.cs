using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.GetHttpAuthorizer;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetHttpAuthorizer;

public class GetHttpAuthorizerQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private GetHttpAuthorizerQueryHandler CreateSut()
        => new(_client, NullLogger<GetHttpAuthorizerQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAuthorizer()
    {
        // Arrange
        var detail = new HttpAuthorizerDetail(
            "auth1",
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);
        _client
            .GetAuthorizerAsync("abc123", "auth1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpAuthorizerDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpAuthorizerQuery("abc123", "auth1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var authorizer = result.Value.Authorizer;
        authorizer.AuthorizerId.Should().Be("auth1");
        authorizer.Name.Should().Be("jwt-authorizer");
        authorizer.AuthorizerType.Should().Be("JWT");
        authorizer.IdentitySource.Should().ContainSingle().Which.Should().Be("$request.header.Authorization");
        authorizer.JwtIssuer.Should().Be("https://example.com/issuer");
        authorizer.JwtAudience.Should().ContainSingle().Which.Should().Be("client1");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetAuthorizerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpAuthorizerDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpAuthorizerQuery("abc123", "auth1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
