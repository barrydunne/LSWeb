using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Route53;

namespace Foundation.Application.Route53;

/// <summary>
/// Abstracts the Route 53 operations the application needs so the handlers stay free of any direct
/// AWS SDK dependency. The implementation flows every call through the resilient AWS gateway and
/// translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IRoute53Client
{
    /// <summary>
    /// List the hosted zones available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The hosted zones, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<HostedZone>>> ListHostedZonesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Create a public hosted zone with the supplied domain name and optional comment.
    /// </summary>
    /// <param name="name">The fully qualified domain name for the hosted zone.</param>
    /// <param name="comment">An optional comment describing the zone, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created hosted zone, or an error when the zone could not be created.</returns>
    Task<Result<HostedZone>> CreateHostedZoneAsync(
        string name, string? comment, CancellationToken cancellationToken);

    /// <summary>
    /// List the DNS resource record sets in a hosted zone.
    /// </summary>
    /// <param name="hostedZoneId">The identifier of the hosted zone.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The records, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<Route53Record>>> ListRecordsAsync(
        string hostedZoneId, CancellationToken cancellationToken);

    /// <summary>
    /// Create or replace a DNS resource record set in a hosted zone.
    /// </summary>
    /// <param name="hostedZoneId">The identifier of the hosted zone.</param>
    /// <param name="record">The record set to create or replace.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the record could not be written.</returns>
    Task<Result> UpsertRecordAsync(
        string hostedZoneId, Route53Record record, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a DNS resource record set from a hosted zone. The supplied record must match the
    /// existing record set exactly, including its TTL and values.
    /// </summary>
    /// <param name="hostedZoneId">The identifier of the hosted zone.</param>
    /// <param name="record">The record set to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the record could not be deleted.</returns>
    Task<Result> DeleteRecordAsync(
        string hostedZoneId, Route53Record record, CancellationToken cancellationToken);
}
