using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Commands.CreateStack;

/// <summary>
/// Create a CloudFormation stack from an inline template body or an S3 template URL with optional parameters and capabilities.
/// </summary>
/// <param name="StackName">The name of the stack to create.</param>
/// <param name="TemplateBody">The inline template body that defines the stack, or <see langword="null"/> when creating from a URL.</param>
/// <param name="TemplateUrl">The S3 URL of the template that defines the stack, or <see langword="null"/> when creating from an inline body.</param>
/// <param name="Parameters">The input parameters to deploy the stack with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
public record CreateStackCommand(
    string StackName,
    string? TemplateBody,
    string? TemplateUrl,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<string> Capabilities) : ICommand<string>;
