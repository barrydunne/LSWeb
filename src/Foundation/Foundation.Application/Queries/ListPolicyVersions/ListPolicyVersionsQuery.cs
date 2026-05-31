using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.ListPolicyVersions;

/// <summary>
/// List the versions of an IAM managed policy.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy whose versions to list.</param>
public record ListPolicyVersionsQuery(string PolicyArn) : IQuery<ListPolicyVersionsQueryResult>;

/// <summary>
/// The versions of an IAM managed policy.
/// </summary>
/// <param name="Versions">The versions, ordered as returned by the backend.</param>
public record ListPolicyVersionsQueryResult(IReadOnlyList<IamPolicyVersion> Versions);
