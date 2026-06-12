using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.GetUser;

/// <summary>
/// Reads the full configuration of a single Amazon Cognito user.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user.</param>
public record GetUserQuery(string UserPoolId, string Username) : IQuery<GetUserQueryResult>;

/// <summary>
/// The result of reading a user.
/// </summary>
/// <param name="User">The user detail.</param>
public record GetUserQueryResult(CognitoUserDetail User);
