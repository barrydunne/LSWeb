namespace Foundation.Domain.Lambda;

/// <summary>
/// A named, reusable test-event payload for invoking a Lambda function. Test events are saved per
/// function so a payload can be named once and replayed later.
/// </summary>
/// <param name="Name">The name that identifies the test event within a function.</param>
/// <param name="Payload">The JSON payload sent to the function when the event is invoked.</param>
public sealed record LambdaTestEvent(string Name, string Payload);
