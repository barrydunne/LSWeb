using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.GetUserPoolClient;

/// <summary>
/// Reads the full configuration of a single Amazon Cognito user pool app client.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientId">The unique identifier of the app client.</param>
public record GetUserPoolClientQuery(string UserPoolId, string ClientId) : IQuery<GetUserPoolClientQueryResult>;

/// <summary>
/// The result of reading an app client.
/// </summary>
/// <param name="Client">The app client detail.</param>
public record GetUserPoolClientQueryResult(UserPoolClientDetail Client);
