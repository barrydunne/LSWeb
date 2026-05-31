using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Iam;

namespace Foundation.Application.Commands.TagRole;

/// <summary>
/// Add or update key/value tags on an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to tag.</param>
/// <param name="Tags">The tags to add or update.</param>
public record TagRoleCommand(string RoleName, IReadOnlyList<IamTag> Tags) : ICommand;
