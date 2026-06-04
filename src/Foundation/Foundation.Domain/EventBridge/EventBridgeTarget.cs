namespace Foundation.Domain.EventBridge;

/// <summary>
/// A single target invoked when an EventBridge rule matches. The target ARN points at the resource
/// the event is delivered to, for example a Lambda function, SQS queue, SNS topic, or state machine.
/// </summary>
/// <param name="Id">The target identifier, unique within the rule.</param>
/// <param name="Arn">The Amazon Resource Name of the resource the rule delivers events to.</param>
public sealed record EventBridgeTarget(
    string Id,
    string Arn);
