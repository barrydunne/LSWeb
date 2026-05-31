using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetIamUser;

/// <summary>
/// Get the full detail of a single IAM user.
/// </summary>
/// <param name="UserName">The name of the user to retrieve.</param>
public record GetIamUserQuery(string UserName) : IQuery<GetIamUserQueryResult>;

/// <summary>
/// The detail of the requested IAM user.
/// </summary>
/// <param name="User">The user detail.</param>
public record GetIamUserQueryResult(IamUserDetail User);
