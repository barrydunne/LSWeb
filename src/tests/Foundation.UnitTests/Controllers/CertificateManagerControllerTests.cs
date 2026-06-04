using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListCertificates;
using Foundation.Domain.CertificateManager;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class CertificateManagerControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<CertificateManagerController> _logger =
        Substitute.For<ILogger<CertificateManagerController>>();

    private CertificateManagerController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListCertificates_WhenQuerySucceeds_ReturnsOkWithCertificates()
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
        _sender
            .Send(Arg.Any<ListCertificatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListCertificatesQueryResult>>(
                new ListCertificatesQueryResult(certificates)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListCertificates(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CertificateListResponse>>().Subject;
        var certificate = ok.Value!.Certificates.Should().ContainSingle().Subject;
        certificate.Arn.Should().Be("arn:aws:acm:eu-west-1:000000000000:certificate/abc");
        certificate.DomainName.Should().Be("example.com");
        certificate.Status.Should().Be("ISSUED");
        certificate.Type.Should().Be("AMAZON_ISSUED");
    }

    [Fact]
    public async Task ListCertificates_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListCertificatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListCertificatesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListCertificates(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
