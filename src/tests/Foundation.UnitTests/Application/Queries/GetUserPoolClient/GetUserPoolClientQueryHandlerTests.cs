using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.GetUserPoolClient;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetUserPoolClient;

public class GetUserPoolClientQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private GetUserPoolClientQueryHandler CreateSut()
        => new(_client, NullLogger<GetUserPoolClientQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static UserPoolClientDetail Detail()
        => new(
            "client-1",
            "web",
            "eu-west-1_abc123",
            "secret",
            true,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsClientDetail()
    {
        // Arrange
        _client
            .GetUserPoolClientAsync("eu-west-1_abc123", "client-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(Detail())));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserPoolClientQuery("eu-west-1_abc123", "client-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Client.ClientId.Should().Be("client-1");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetUserPoolClientAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserPoolClientQuery("eu-west-1_abc123", "client-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
