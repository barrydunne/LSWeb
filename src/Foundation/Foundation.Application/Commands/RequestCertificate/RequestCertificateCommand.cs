using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RequestCertificate;

/// <summary>
/// Request a new certificate from ACM for the supplied domain.
/// </summary>
/// <param name="DomainName">The fully qualified domain name the certificate should secure.</param>
/// <param name="ValidationMethod">The validation method, such as <c>DNS</c> or <c>EMAIL</c>.</param>
/// <param name="SubjectAlternativeNames">Additional domain names the certificate should cover, or an empty list when none are required.</param>
public record RequestCertificateCommand(
    string DomainName,
    string ValidationMethod,
    IReadOnlyList<string> SubjectAlternativeNames) : ICommand<string>;
