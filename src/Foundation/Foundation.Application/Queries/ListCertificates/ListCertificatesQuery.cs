using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CertificateManager;

namespace Foundation.Application.Queries.ListCertificates;

/// <summary>
/// List the ACM certificates available on the backend.
/// </summary>
public record ListCertificatesQuery : IQuery<ListCertificatesQueryResult>;

/// <summary>
/// The ACM certificates available on the backend.
/// </summary>
/// <param name="Certificates">The certificates, ordered as returned by the backend.</param>
public record ListCertificatesQueryResult(IReadOnlyList<Certificate> Certificates);
