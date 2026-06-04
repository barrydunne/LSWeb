using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CertificateManager;
using Foundation.Domain.CertificateManager;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class CertificateManagerResourceSourceTests
{
    private readonly ICertificateManagerClient _client = Substitute.For<ICertificateManagerClient>();

    private CertificateManagerResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsAcm()
        => CreateSut().ServiceKey.Should().Be("acm");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsCertificatesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<Certificate> certificates =
        [
            new(
                "arn:aws:acm:eu-west-1:000000000000:certificate/abc def",
                "example.com",
                "ISSUED",
                null),
        ];
        _client
            .ListCertificatesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(certificates)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("acm");
        entry.ResourceId.Should().Be("arn:aws:acm:eu-west-1:000000000000:certificate/abc def");
        entry.DisplayName.Should().Be("example.com");
        entry.Route.Should().Be(
            "/services/acm/arn%3Aaws%3Aacm%3Aeu-west-1%3A000000000000%3Acertificate%2Fabc%20def");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListCertificatesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Certificate>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
