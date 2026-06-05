namespace Foundation.Domain.EventBridge;

/// <summary>
/// The configuration used to add a single target to an EventBridge rule. The target ARN points at
/// the resource matched events are delivered to, for example an SQS queue, SNS topic, or Lambda.
/// </summary>
/// <param name="Id">The target identifier, unique within the rule.</param>
/// <param name="Arn">The Amazon Resource Name of the resource the rule delivers events to.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking the target, or <see langword="null"/> when none is required.</param>
/// <param name="Input">A constant JSON text passed to the target instead of the matched event, or <see langword="null"/> to pass the event.</param>
public sealed record EventBridgeTargetSpecification(
    string Id,
    string Arn,
    string? RoleArn,
    string? Input);
