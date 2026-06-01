using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.GetStackTemplate;

/// <summary>
/// Get the template body that defines a single CloudFormation stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack whose template to get.</param>
public record GetStackTemplateQuery(string StackName) : IQuery<GetStackTemplateQueryResult>;

/// <summary>
/// The template that defines a single CloudFormation stack.
/// </summary>
/// <param name="Template">The stack template.</param>
public record GetStackTemplateQueryResult(CloudFormationStackTemplate Template);
