using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetIamRoleConsumers;

/// <summary>
/// Find the resources that use a given IAM role, such as Lambda functions whose execution role
/// matches the role.
/// </summary>
/// <param name="RoleName">The name of the role whose consumers to find.</param>
public record GetIamRoleConsumersQuery(string RoleName) : IQuery<GetIamRoleConsumersQueryResult>;

/// <summary>
/// The resources that use an IAM role.
/// </summary>
/// <param name="Consumers">The consumers that reference the role, ordered by discovery.</param>
public record GetIamRoleConsumersQueryResult(IReadOnlyList<IamRoleConsumer> Consumers);
