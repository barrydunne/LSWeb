using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Commands.UpdateStack;

/// <summary>
/// Update a CloudFormation stack with a revised template body, parameters, and capabilities.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack to update.</param>
/// <param name="TemplateBody">The revised template body to apply.</param>
/// <param name="Parameters">The input parameters to apply to the stack.</param>
/// <param name="Capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
public record UpdateStackCommand(
    string StackName,
    string TemplateBody,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<string> Capabilities) : ICommand<string>;
