using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Iam;

namespace Foundation.Application.Commands.TagPolicy;

/// <summary>
/// Add or update key/value tags on an IAM managed policy.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to tag.</param>
/// <param name="Tags">The tags to add or update.</param>
public record TagPolicyCommand(string PolicyArn, IReadOnlyList<IamTag> Tags) : ICommand;
