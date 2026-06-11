using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.ImportCertificate;
using Foundation.Application.Commands.RequestCertificate;
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

    [Fact]
    public async Task ImportCertificate_WhenCommandSucceeds_ReturnsCreatedWithArn()
    {
        // Arrange
        CommandCapture<ImportCertificateCommand> captured = new();
        _sender
            .Send(Arg.Do<ImportCertificateCommand>(command => captured.Value = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:aws:acm:eu-west-1:000000000000:certificate/new"));
        var request = new CertificateImportRequest("cert-body", "key-body", "chain-body");
        var sut = CreateSut();

        // Act
        var result = await sut.ImportCertificate(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<CertificateImportResponse>>().Subject;
        created.Value!.Arn.Should().Be("arn:aws:acm:eu-west-1:000000000000:certificate/new");
        captured.Value!.Certificate.Should().Be("cert-body");
        captured.Value!.PrivateKey.Should().Be("key-body");
        captured.Value!.CertificateChain.Should().Be("chain-body");
    }

    [Fact]
    public async Task ImportCertificate_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ImportCertificateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("import boom")));
        var request = new CertificateImportRequest("cert-body", "key-body", null);
        var sut = CreateSut();

        // Act
        var result = await sut.ImportCertificate(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RequestCertificate_WhenCommandSucceeds_ReturnsCreatedWithArn()
    {
        // Arrange
        CommandCapture<RequestCertificateCommand> captured = new();
        _sender
            .Send(Arg.Do<RequestCertificateCommand>(command => captured.Value = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:aws:acm:eu-west-1:000000000000:certificate/req"));
        var request = new CertificateRequestRequest("example.com", "DNS", ["www.example.com"]);
        var sut = CreateSut();

        // Act
        var result = await sut.RequestCertificate(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<CertificateRequestResponse>>().Subject;
        created.Value!.Arn.Should().Be("arn:aws:acm:eu-west-1:000000000000:certificate/req");
        captured.Value!.DomainName.Should().Be("example.com");
        captured.Value!.ValidationMethod.Should().Be("DNS");
        captured.Value!.SubjectAlternativeNames.Should().ContainSingle().Which.Should().Be("www.example.com");
    }

    [Fact]
    public async Task RequestCertificate_WhenSubjectAlternativeNamesOmitted_PassesEmptyList()
    {
        // Arrange
        CommandCapture<RequestCertificateCommand> captured = new();
        _sender
            .Send(Arg.Do<RequestCertificateCommand>(command => captured.Value = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:aws:acm:eu-west-1:000000000000:certificate/req"));
        var request = new CertificateRequestRequest("example.com", "DNS", null);
        var sut = CreateSut();

        // Act
        await sut.RequestCertificate(request, TestContext.Current.CancellationToken);

        // Assert
        captured.Value!.SubjectAlternativeNames.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestCertificate_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RequestCertificateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("request boom")));
        var request = new CertificateRequestRequest("example.com", "DNS", null);
        var sut = CreateSut();

        // Act
        var result = await sut.RequestCertificate(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private sealed class CommandCapture<T>
    {
        public T? Value { get; set; }
    }
}
