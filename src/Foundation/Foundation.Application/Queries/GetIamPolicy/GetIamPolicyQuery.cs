using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetIamPolicy;

/// <summary>
/// Get the full detail of a single IAM managed policy.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to retrieve.</param>
public record GetIamPolicyQuery(string PolicyArn) : IQuery<GetIamPolicyQueryResult>;

/// <summary>
/// The full detail of a single IAM managed policy.
/// </summary>
/// <param name="Policy">The policy detail.</param>
public record GetIamPolicyQueryResult(IamPolicyDetail Policy);
