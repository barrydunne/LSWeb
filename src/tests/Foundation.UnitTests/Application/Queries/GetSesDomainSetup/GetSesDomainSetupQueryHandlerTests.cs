using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSesDomainSetup;
using Foundation.Application.Ses;
using Foundation.Domain.Ses;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSesDomainSetup;

public class GetSesDomainSetupQueryHandlerTests
{
    private readonly ISesClient _client = Substitute.For<ISesClient>();

    private GetSesDomainSetupQueryHandler CreateSut()
        => new(_client, NullLogger<GetSesDomainSetupQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSetup()
    {
        // Arrange
        var setup = new SesDomainSetup("example.com", "Pending", "token", "NotStarted", ["a", "b"]);
        _client
            .GetDomainSetupAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(setup)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSesDomainSetupQuery("example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Setup.Domain.Should().Be("example.com");
        result.Value.Setup.VerificationToken.Should().Be("token");
        result.Value.Setup.DkimTokens.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetDomainSetupAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SesDomainSetup>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSesDomainSetupQuery("example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
