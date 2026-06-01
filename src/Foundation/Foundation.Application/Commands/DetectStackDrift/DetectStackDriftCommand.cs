using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DetectStackDrift;

/// <summary>
/// Start a CloudFormation drift detection operation against a single stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack to detect drift on.</param>
public record DetectStackDriftCommand(string StackName) : ICommand<string>;
