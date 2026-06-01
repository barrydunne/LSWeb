using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Commands.CreateStack;

/// <summary>
/// Create a CloudFormation stack from a template body with optional parameters and capabilities.
/// </summary>
/// <param name="StackName">The name of the stack to create.</param>
/// <param name="TemplateBody">The template body that defines the stack.</param>
/// <param name="Parameters">The input parameters to deploy the stack with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
public record CreateStackCommand(
    string StackName,
    string TemplateBody,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<string> Capabilities) : ICommand<string>;
