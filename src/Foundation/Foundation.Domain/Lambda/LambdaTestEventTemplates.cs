namespace Foundation.Domain.Lambda;

/// <summary>
/// Provides a fixed set of starter test-event payloads that seed the test-event editor so common
/// AWS event shapes can be loaded without typing them by hand.
/// </summary>
public static class LambdaTestEventTemplates
{
    private static readonly IReadOnlyList<LambdaTestEvent> _templates =
    [
        new("Empty", "{}"),
        new(
            "API Gateway (HTTP)",
            """
            {
              "httpMethod": "GET",
              "path": "/",
              "headers": {},
              "queryStringParameters": null,
              "body": null,
              "isBase64Encoded": false
            }
            """),
        new(
            "S3 Put",
            """
            {
              "Records": [
                {
                  "eventSource": "aws:s3",
                  "eventName": "ObjectCreated:Put",
                  "s3": {
                    "bucket": { "name": "example-bucket" },
                    "object": { "key": "example-key" }
                  }
                }
              ]
            }
            """),
        new(
            "SQS Message",
            """
            {
              "Records": [
                {
                  "eventSource": "aws:sqs",
                  "messageId": "00000000-0000-0000-0000-000000000000",
                  "body": "Hello from SQS"
                }
              ]
            }
            """),
        new(
            "SNS Notification",
            """
            {
              "Records": [
                {
                  "EventSource": "aws:sns",
                  "Sns": {
                    "Subject": "Example",
                    "Message": "Hello from SNS"
                  }
                }
              ]
            }
            """),
        new(
            "Scheduled (EventBridge)",
            """
            {
              "source": "aws.events",
              "detail-type": "Scheduled Event",
              "detail": {}
            }
            """),
    ];

    /// <summary>
    /// Gets the available starter templates.
    /// </summary>
    public static IReadOnlyList<LambdaTestEvent> Templates => _templates;
}
