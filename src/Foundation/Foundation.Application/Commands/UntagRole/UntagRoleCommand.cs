using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UntagRole;

/// <summary>
/// Remove tags from an IAM role by key.
/// </summary>
/// <param name="RoleName">The name of the role to untag.</param>
/// <param name="TagKeys">The keys of the tags to remove.</param>
public record UntagRoleCommand(string RoleName, IReadOnlyList<string> TagKeys) : ICommand;
