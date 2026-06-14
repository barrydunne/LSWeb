using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSesIdentity;
using Foundation.Application.Ses;
using Foundation.Domain.Ses;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSesIdentity;

public class GetSesIdentityQueryHandlerTests
{
    private readonly ISesClient _client = Substitute.For<ISesClient>();

    private GetSesIdentityQueryHandler CreateSut()
        => new(_client, NullLogger<GetSesIdentityQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsDetail()
    {
        // Arrange
        var detail = new SesIdentityDetail("sender@example.com", "EmailAddress", "Pending");
        _client
            .GetIdentityAsync("sender@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSesIdentityQuery("sender@example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Identity.Identity.Should().Be("sender@example.com");
        result.Value.Identity.IdentityType.Should().Be("EmailAddress");
        result.Value.Identity.VerificationStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetIdentityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SesIdentityDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSesIdentityQuery("sender@example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
