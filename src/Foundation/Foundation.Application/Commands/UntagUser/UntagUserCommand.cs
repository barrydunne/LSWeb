using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UntagUser;

/// <summary>
/// Remove tags from an IAM user by key.
/// </summary>
/// <param name="UserName">The name of the user to untag.</param>
/// <param name="TagKeys">The keys of the tags to remove.</param>
public record UntagUserCommand(string UserName, IReadOnlyList<string> TagKeys) : ICommand;
