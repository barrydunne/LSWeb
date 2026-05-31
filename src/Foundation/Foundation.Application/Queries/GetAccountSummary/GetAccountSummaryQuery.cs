using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetAccountSummary;

/// <summary>
/// Get the account-wide IAM entity counts and quotas reported by the backend.
/// </summary>
public record GetAccountSummaryQuery : IQuery<GetAccountSummaryQueryResult>;

/// <summary>
/// The account-wide IAM entity counts and quotas reported by the backend.
/// </summary>
/// <param name="Summary">The account summary.</param>
public record GetAccountSummaryQueryResult(IamAccountSummary Summary);
