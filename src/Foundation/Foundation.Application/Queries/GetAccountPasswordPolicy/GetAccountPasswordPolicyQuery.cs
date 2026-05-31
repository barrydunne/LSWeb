using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetAccountPasswordPolicy;

/// <summary>
/// Get the account password policy, if one is set on the backend.
/// </summary>
public record GetAccountPasswordPolicyQuery : IQuery<GetAccountPasswordPolicyQueryResult>;

/// <summary>
/// The account password policy, or an indication that no policy is set.
/// </summary>
/// <param name="Policy">The password policy, or <see langword="null"/> when no policy is set.</param>
public record GetAccountPasswordPolicyQueryResult(IamPasswordPolicy? Policy);
