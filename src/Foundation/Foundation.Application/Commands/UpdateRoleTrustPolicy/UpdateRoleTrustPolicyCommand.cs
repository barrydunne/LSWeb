using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateRoleTrustPolicy;

/// <summary>
/// Replace the trust policy (assume-role policy) of an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role whose trust policy is replaced.</param>
/// <param name="PolicyDocument">The new trust policy JSON document.</param>
public record UpdateRoleTrustPolicyCommand(string RoleName, string PolicyDocument) : ICommand;
