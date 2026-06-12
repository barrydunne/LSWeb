using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.ListUsers;

/// <summary>
/// Lists the users within an Amazon Cognito user pool.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool whose users should be listed.</param>
public record ListUsersQuery(string UserPoolId) : IQuery<ListUsersQueryResult>;

/// <summary>
/// The result of listing users.
/// </summary>
/// <param name="Users">The users found within the user pool.</param>
public record ListUsersQueryResult(IReadOnlyList<CognitoUserSummary> Users);
