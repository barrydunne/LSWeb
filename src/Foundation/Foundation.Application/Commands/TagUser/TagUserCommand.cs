using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Iam;

namespace Foundation.Application.Commands.TagUser;

/// <summary>
/// Add or update key/value tags on an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to tag.</param>
/// <param name="Tags">The tags to add or update.</param>
public record TagUserCommand(string UserName, IReadOnlyList<IamTag> Tags) : ICommand;
