using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.GetUserPool;

/// <summary>
/// Reads the full configuration of a single Amazon Cognito user pool.
/// </summary>
/// <param name="Id">The unique identifier of the user pool.</param>
public record GetUserPoolQuery(string Id) : IQuery<GetUserPoolQueryResult>;

/// <summary>
/// The result of reading a user pool.
/// </summary>
/// <param name="UserPool">The user pool detail.</param>
public record GetUserPoolQueryResult(UserPoolDetail UserPool);
