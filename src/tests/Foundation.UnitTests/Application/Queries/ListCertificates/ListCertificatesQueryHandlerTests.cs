using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CertificateManager;
using Foundation.Application.Queries.ListCertificates;
using Foundation.Domain.CertificateManager;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListCertificates;

public class ListCertificatesQueryHandlerTests
{
    private readonly ICertificateManagerClient _client = Substitute.For<ICertificateManagerClient>();

    private ListCertificatesQueryHandler CreateSut()
        => new(_client, NullLogger<ListCertificatesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsCertificates()
    {
        // Arrange
        IReadOnlyList<Certificate> certificates =
        [
            new(
                "arn:aws:acm:eu-west-1:000000000000:certificate/abc",
                "example.com",
                "ISSUED",
                "AMAZON_ISSUED"),
        ];
        _client
            .ListCertificatesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(certificates)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListCertificatesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Certificates.Should().ContainSingle(_ => _.DomainName == "example.com");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListCertificatesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Certificate>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListCertificatesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
