using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreatePolicy;

/// <summary>
/// Create a customer managed policy with the supplied name, document, and optional description and path.
/// </summary>
/// <param name="PolicyName">The name of the policy to create.</param>
/// <param name="PolicyDocument">The JSON policy document for the initial version.</param>
/// <param name="Description">The optional description for the policy, or <see langword="null"/> for none.</param>
/// <param name="Path">The optional path for the policy, or <see langword="null"/> for the default path.</param>
public record CreatePolicyCommand(string PolicyName, string PolicyDocument, string? Description, string? Path) : ICommand;
