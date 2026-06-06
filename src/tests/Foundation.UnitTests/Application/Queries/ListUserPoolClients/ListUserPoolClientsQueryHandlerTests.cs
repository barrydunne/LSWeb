using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Application.Queries.ListUserPoolClients;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListUserPoolClients;

public class ListUserPoolClientsQueryHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private ListUserPoolClientsQueryHandler CreateSut()
        => new(_client, NullLogger<ListUserPoolClientsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsClients()
    {
        // Arrange
        IReadOnlyList<UserPoolClientSummary> clients =
        [
            new("client-1", "web", "eu-west-1_abc123"),
        ];
        _client
            .ListUserPoolClientsAsync("eu-west-1_abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(clients)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUserPoolClientsQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Clients.Should().ContainSingle(_ => _.ClientId == "client-1");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListUserPoolClientsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<UserPoolClientSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListUserPoolClientsQuery("eu-west-1_abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
