using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.ListHttpIntegrations;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHttpIntegrations;

public class ListHttpIntegrationsQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ListHttpIntegrationsQueryHandler CreateSut()
        => new(_client, NullLogger<ListHttpIntegrationsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsIntegrations()
    {
        // Arrange
        IReadOnlyList<HttpIntegrationSummary> integrations =
        [
            new("int1", "HTTP_PROXY", "GET", "https://example.test", "1.0", "proxy"),
        ];
        _client
            .ListIntegrationsAsync("abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(integrations)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpIntegrationsQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var integration = result.Value.Integrations.Should().ContainSingle().Subject;
        integration.IntegrationId.Should().Be("int1");
        integration.IntegrationType.Should().Be("HTTP_PROXY");
        integration.IntegrationMethod.Should().Be("GET");
        integration.IntegrationUri.Should().Be("https://example.test");
        integration.PayloadFormatVersion.Should().Be("1.0");
        integration.Description.Should().Be("proxy");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListIntegrationsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpIntegrationSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpIntegrationsQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
