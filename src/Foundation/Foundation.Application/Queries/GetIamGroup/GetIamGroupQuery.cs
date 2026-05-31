using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetIamGroup;

/// <summary>
/// Get the full detail of a single IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to retrieve.</param>
public record GetIamGroupQuery(string GroupName) : IQuery<GetIamGroupQueryResult>;

/// <summary>
/// The detail of the requested IAM group.
/// </summary>
/// <param name="Group">The group detail.</param>
public record GetIamGroupQueryResult(IamGroupDetail Group);
