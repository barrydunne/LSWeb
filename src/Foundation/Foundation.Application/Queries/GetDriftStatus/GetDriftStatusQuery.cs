using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.GetDriftStatus;

/// <summary>
/// Get the status of a CloudFormation drift detection operation.
/// </summary>
/// <param name="DriftDetectionId">The id of the drift detection operation to poll.</param>
public record GetDriftStatusQuery(string DriftDetectionId) : IQuery<GetDriftStatusQueryResult>;

/// <summary>
/// The status of a CloudFormation drift detection operation.
/// </summary>
/// <param name="Status">The drift detection status.</param>
public record GetDriftStatusQueryResult(StackDriftStatus Status);
