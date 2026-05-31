using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Iam;

namespace Foundation.Application.Commands.CreateAccessKey;

/// <summary>
/// Create an access key for an IAM user. The secret access key is only returned once at creation time.
/// </summary>
/// <param name="UserName">The name of the user to create the access key for.</param>
public record CreateAccessKeyCommand(string UserName) : ICommand<IamAccessKeySecret>;
