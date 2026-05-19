namespace Foundation.Domain.Lambda;

/// <summary>
/// An event source mapping that links an event source (such as an SQS queue or DynamoDB stream)
/// to the Lambda function it triggers.
/// </summary>
/// <param name="Uuid">The unique identifier of the mapping.</param>
/// <param name="EventSourceArn">The Amazon Resource Name of the event source; empty when not reported.</param>
/// <param name="FunctionArn">The Amazon Resource Name of the target function; empty when not reported.</param>
/// <param name="State">The mapping state reported by AWS, for example <c>Enabled</c> or <c>Disabled</c>; empty when not reported.</param>
/// <param name="BatchSize">The maximum number of records delivered to the function in a single batch.</param>
/// <param name="LastModified">The timestamp the mapping was last updated, in ISO 8601 form; empty when not reported.</param>
public sealed record LambdaEventSourceMapping(
    string Uuid,
    string EventSourceArn,
    string FunctionArn,
    string State,
    int BatchSize,
    string LastModified);
