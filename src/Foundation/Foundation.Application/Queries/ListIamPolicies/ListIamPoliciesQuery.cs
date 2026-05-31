using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.ListIamPolicies;

/// <summary>
/// List the IAM managed policies available on the backend.
/// </summary>
/// <param name="AwsManaged">
/// When <see langword="false"/>, only customer (local) managed policies are listed; when
/// <see langword="true"/>, only AWS managed policies are listed for use in attach pickers.
/// </param>
public record ListIamPoliciesQuery(bool AwsManaged = false) : IQuery<ListIamPoliciesQueryResult>;

/// <summary>
/// The IAM managed policies available on the backend.
/// </summary>
/// <param name="Policies">The policies, ordered as returned by the backend.</param>
public record ListIamPoliciesQueryResult(IReadOnlyList<IamPolicy> Policies);
