using Foundation.Application.Commands.ImportCertificate;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ImportCertificate;

public class ImportCertificateCommandValidatorTests
{
    private const string Cert = "-----BEGIN CERTIFICATE-----\nabc\n-----END CERTIFICATE-----";
    private const string Key = "-----BEGIN PRIVATE KEY-----\nxyz\n-----END PRIVATE KEY-----";

    private readonly ImportCertificateCommandValidator _sut =
        new(NullLogger<ImportCertificateCommandValidator>.Instance);

    private static ImportCertificateCommand Valid(
        string certificate = Cert,
        string privateKey = Key,
        string? certificateChain = null)
        => new(certificate, privateKey, certificateChain);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenChainProvidedAndValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(certificateChain: Cert), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenCertificateEmpty_ReturnsErrorForCertificate()
    {
        var result = await _sut.ValidateAsync(
            Valid(certificate: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ImportCertificateCommand.Certificate));
    }

    [Fact]
    public async Task ValidateAsync_WhenCertificateNotPem_ReturnsErrorForCertificate()
    {
        var result = await _sut.ValidateAsync(
            Valid(certificate: "not-a-pem"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ImportCertificateCommand.Certificate));
    }

    [Fact]
    public async Task ValidateAsync_WhenPrivateKeyEmpty_ReturnsErrorForPrivateKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(privateKey: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ImportCertificateCommand.PrivateKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenPrivateKeyNotPem_ReturnsErrorForPrivateKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(privateKey: "not-a-key"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ImportCertificateCommand.PrivateKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenChainProvidedButNotPem_ReturnsErrorForChain()
    {
        var result = await _sut.ValidateAsync(
            Valid(certificateChain: "not-a-pem"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ImportCertificateCommand.CertificateChain));
    }
}
