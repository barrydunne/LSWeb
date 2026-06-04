using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSesIdentities;
using Foundation.Application.Ses;
using Foundation.Domain.Ses;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSesIdentities;

public class ListSesIdentitiesQueryHandlerTests
{
    private readonly ISesClient _client = Substitute.For<ISesClient>();

    private ListSesIdentitiesQueryHandler CreateSut()
        => new(_client, NullLogger<ListSesIdentitiesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsIdentities()
    {
        // Arrange
        IReadOnlyList<SesIdentity> identities =
        [
            new("sender@example.com", "EmailAddress", "Success"),
        ];
        _client
            .ListIdentitiesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(identities)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSesIdentitiesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Identities.Should().ContainSingle(_ => _.Identity == "sender@example.com");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListIdentitiesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SesIdentity>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSesIdentitiesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
