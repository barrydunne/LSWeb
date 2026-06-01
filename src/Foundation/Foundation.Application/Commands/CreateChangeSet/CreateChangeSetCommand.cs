using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Commands.CreateChangeSet;

/// <summary>
/// Create a CloudFormation change set that previews the changes a revised template would make.
/// </summary>
/// <param name="StackName">The name of the stack the change set targets.</param>
/// <param name="ChangeSetName">The name to assign to the change set.</param>
/// <param name="ChangeSetType">Whether the change set creates a new stack (<c>CREATE</c>) or updates one (<c>UPDATE</c>).</param>
/// <param name="TemplateBody">The template body the change set evaluates.</param>
/// <param name="Parameters">The input parameters to evaluate the change set with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
public record CreateChangeSetCommand(
    string StackName,
    string ChangeSetName,
    string ChangeSetType,
    string TemplateBody,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<string> Capabilities) : ICommand<string>;
