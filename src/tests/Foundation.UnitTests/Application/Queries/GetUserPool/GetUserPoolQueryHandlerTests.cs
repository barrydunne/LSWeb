using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.GetUserPool;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetUserPool;

public class GetUserPoolQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private GetUserPoolQueryHandler CreateSut()
        => new(_client, NullLogger<GetUserPoolQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsUserPool()
    {
        // Arrange
        var detail = new UserPoolDetail(
            "eu-west-1_abc123",
            "customers",
            "arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc123",
            "OFF",
            0,
            ["email"],
            ["email"],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        _client
            .GetUserPoolAsync("eu-west-1_abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserPoolQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserPool.Id.Should().Be("eu-west-1_abc123");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetUserPoolAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetUserPoolQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
