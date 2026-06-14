using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.S3;

namespace Foundation.Application.Commands.PutS3BucketNotifications;

/// <summary>
/// Replace the event notification configuration of an S3 bucket with the supplied set of rules.
/// </summary>
/// <param name="BucketName">The bucket to update.</param>
/// <param name="Notifications">The complete set of notification rules to apply; an empty list clears all notifications.</param>
public record PutS3BucketNotificationsCommand(
    string BucketName,
    IReadOnlyList<S3NotificationConfiguration> Notifications) : ICommand;
