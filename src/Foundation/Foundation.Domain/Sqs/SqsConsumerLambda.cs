namespace Foundation.Domain.Sqs;

/// <summary>
/// A Lambda function that consumes a queue through an event source mapping. Lets the UI show the
/// relationship as a cross-resource link to the triggered function.
/// </summary>
/// <param name="FunctionName">The bare name of the function that the queue triggers.</param>
/// <param name="FunctionArn">The ARN of the function the event source mapping targets; empty when not reported.</param>
/// <param name="State">The event source mapping state reported by AWS, for example <c>Enabled</c> or <c>Disabled</c>; empty when not reported.</param>
public sealed record SqsConsumerLambda(string FunctionName, string FunctionArn, string State);
