using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.ListUserPoolClients;

/// <summary>
/// Lists the app clients configured within an Amazon Cognito user pool.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool whose app clients should be listed.</param>
public record ListUserPoolClientsQuery(string UserPoolId) : IQuery<ListUserPoolClientsQueryResult>;

/// <summary>
/// The result of listing app clients.
/// </summary>
/// <param name="Clients">The app clients found within the user pool.</param>
public record ListUserPoolClientsQueryResult(IReadOnlyList<UserPoolClientSummary> Clients);
