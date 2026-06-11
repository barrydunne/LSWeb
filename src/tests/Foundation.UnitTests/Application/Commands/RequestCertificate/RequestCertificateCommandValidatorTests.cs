using Foundation.Application.Commands.RequestCertificate;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RequestCertificate;

public class RequestCertificateCommandValidatorTests
{
    private readonly RequestCertificateCommandValidator _sut =
        new(NullLogger<RequestCertificateCommandValidator>.Instance);

    private static RequestCertificateCommand Valid(
        string domainName = "example.com",
        string validationMethod = "DNS",
        IReadOnlyList<string>? subjectAlternativeNames = null)
        => new(domainName, validationMethod, subjectAlternativeNames ?? []);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailMethodAndSansProvided_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(validationMethod: "EMAIL", subjectAlternativeNames: ["www.example.com"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDomainNameEmpty_ReturnsErrorForDomainName()
    {
        var result = await _sut.ValidateAsync(
            Valid(domainName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCertificateCommand.DomainName));
    }

    [Fact]
    public async Task ValidateAsync_WhenValidationMethodEmpty_ReturnsErrorForValidationMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(validationMethod: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCertificateCommand.ValidationMethod));
    }

    [Fact]
    public async Task ValidateAsync_WhenValidationMethodUnsupported_ReturnsErrorForValidationMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(validationMethod: "PHONE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCertificateCommand.ValidationMethod));
    }

    [Fact]
    public async Task ValidateAsync_WhenSubjectAlternativeNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(subjectAlternativeNames: [string.Empty]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.StartsWith(
            nameof(RequestCertificateCommand.SubjectAlternativeNames), StringComparison.Ordinal));
    }
}
