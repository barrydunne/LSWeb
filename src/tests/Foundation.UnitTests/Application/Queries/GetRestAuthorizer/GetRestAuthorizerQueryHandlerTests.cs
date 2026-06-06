using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.GetRestAuthorizer;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRestAuthorizer;

public class GetRestAuthorizerQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private GetRestAuthorizerQueryHandler CreateSut()
        => new(_client, NullLogger<GetRestAuthorizerQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAuthorizer()
    {
        // Arrange
        var detail = new RestAuthorizerDetail(
            "auth-1", "pool-authorizer", "COGNITO_USER_POOLS",
            ["arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc"],
            "method.request.header.Authorization", "COGNITO_USER_POOLS");
        _client
            .GetAuthorizerAsync("api-1", "auth-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestAuthorizerDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestAuthorizerQuery("api-1", "auth-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Authorizer.Id.Should().Be("auth-1");
        result.Value.Authorizer.Name.Should().Be("pool-authorizer");
        result.Value.Authorizer.Type.Should().Be("COGNITO_USER_POOLS");
        result.Value.Authorizer.ProviderARNs.Should().ContainSingle();
        result.Value.Authorizer.IdentitySource.Should().Be("method.request.header.Authorization");
        result.Value.Authorizer.AuthType.Should().Be("COGNITO_USER_POOLS");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetAuthorizerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestAuthorizerDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestAuthorizerQuery("api-1", "auth-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
