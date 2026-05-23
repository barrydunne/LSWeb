using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Amazon.SQS.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Foundation.Infrastructure.Aws;
using SqsMessage = Foundation.Domain.Sqs.SqsMessage;
using SqsQueue = Foundation.Domain.Sqs.SqsQueue;
using SqsQueueAttributes = Foundation.Domain.Sqs.SqsQueueAttributes;
using SqsQueueSubscription = Foundation.Domain.Sqs.SqsQueueSubscription;
using SqsRedrive = Foundation.Domain.Sqs.SqsRedrive;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Reads SQS queues through the resilient AWS gateway so the same code works against LocalStack or
/// real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SqsClientAdapter : ISqsClient
{
    private const string ServiceKey = "sqs";

    private static readonly List<string> _countAttributeNames =
    [
        "ApproximateNumberOfMessages",
        "ApproximateNumberOfMessagesNotVisible",
        "ApproximateNumberOfMessagesDelayed",
    ];

    private readonly IAwsGateway _gateway;

    public SqsClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<SqsQueue>>> ListQueuesAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSQSClient, IReadOnlyList<SqsQueue>>(
            ServiceKey,
            async (client, token) =>
            {
                var queues = new List<SqsQueue>();
                string? nextToken = null;

                do
                {
                    var listResponse = await client.ListQueuesAsync(
                        new ListQueuesRequest { NextToken = nextToken, MaxResults = 1000 },
                        token);

                    foreach (var queueUrl in listResponse.QueueUrls ?? [])
                    {
                        var attributes = await client.GetQueueAttributesAsync(
                            new GetQueueAttributesRequest { QueueUrl = queueUrl, AttributeNames = _countAttributeNames },
                            token);

                        queues.Add(SqsQueueMapper.ToQueue(queueUrl, attributes.Attributes));
                    }

                    nextToken = listResponse.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return queues;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<SqsMessage>>> ReceiveMessagesAsync(
        string queueName, SqsPollMode mode, int maxMessages, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSQSClient, IReadOnlyList<SqsMessage>>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = Math.Clamp(maxMessages, 1, 10),
                    WaitTimeSeconds = 0,
                    MessageSystemAttributeNames = ["All"],
                    MessageAttributeNames = ["All"],
                    VisibilityTimeout = mode == SqsPollMode.Peek ? 0 : null,
                };

                var response = await client.ReceiveMessageAsync(request, token);

                IReadOnlyList<SqsMessage> messages = (response.Messages ?? [])
                    .Select(SqsMessageMapper.ToMessage)
                    .ToList();
                return messages;
            },
            cancellationToken);

    public async Task<Result> CreateQueueAsync(string queueName, bool fifoQueue, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateQueueRequest { QueueName = queueName };
                if (fifoQueue)
                    request.Attributes = new Dictionary<string, string> { ["FifoQueue"] = "true" };

                await client.CreateQueueAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);
                await client.DeleteQueueAsync(new DeleteQueueRequest { QueueUrl = queueUrl }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteMessageAsync(string queueName, string receiptHandle, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);
                await client.DeleteMessageAsync(
                    new DeleteMessageRequest { QueueUrl = queueUrl, ReceiptHandle = receiptHandle },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PurgeQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);
                await client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = queueUrl }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> SendMessageAsync(
        string queueName,
        string body,
        IReadOnlyDictionary<string, string> messageAttributes,
        string? messageGroupId,
        string? messageDeduplicationId,
        CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var request = new SendMessageRequest { QueueUrl = queueUrl, MessageBody = body };

                if (messageAttributes.Count > 0)
                    request.MessageAttributes = messageAttributes.ToDictionary(
                        pair => pair.Key,
                        pair => new MessageAttributeValue { DataType = "String", StringValue = pair.Value });

                if (!string.IsNullOrEmpty(messageGroupId))
                    request.MessageGroupId = messageGroupId;

                if (!string.IsNullOrEmpty(messageDeduplicationId))
                    request.MessageDeduplicationId = messageDeduplicationId;

                await client.SendMessageAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<SqsQueueSubscription>>> GetQueueSubscriptionsAsync(
        string queueName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSQSClient, IReadOnlyList<SqsQueueSubscription>>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var attributes = await client.GetQueueAttributesAsync(
                    new GetQueueAttributesRequest { QueueUrl = queueUrl, AttributeNames = ["Policy"] },
                    token);

                var policy = attributes.Attributes?.GetValueOrDefault("Policy");

                return SqsSubscriptionMapper.ParseSubscriptions(policy);
            },
            cancellationToken);

    public Task<Result<SqsQueueAttributes>> GetQueueAttributesAsync(
        string queueName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSQSClient, SqsQueueAttributes>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var attributes = await client.GetQueueAttributesAsync(
                    new GetQueueAttributesRequest { QueueUrl = queueUrl, AttributeNames = ["All"] },
                    token);

                return SqsQueueAttributesMapper.ToAttributes(attributes.Attributes);
            },
            cancellationToken);

    public async Task<Result> SetQueueAttributesAsync(
        string queueName, IReadOnlyDictionary<string, string> attributes, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                await client.SetQueueAttributesAsync(
                    new SetQueueAttributesRequest
                    {
                        QueueUrl = queueUrl,
                        Attributes = attributes.ToDictionary(pair => pair.Key, pair => pair.Value),
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<SqsRedrive>> GetQueueRedriveAsync(
        string queueName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSQSClient, SqsRedrive>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var attributes = await client.GetQueueAttributesAsync(
                    new GetQueueAttributesRequest
                    {
                        QueueUrl = queueUrl,
                        AttributeNames = ["RedrivePolicy", "RedriveAllowPolicy"],
                    },
                    token);

                var redrivePolicy = attributes.Attributes?.GetValueOrDefault("RedrivePolicy");
                var redriveAllowPolicy = attributes.Attributes?.GetValueOrDefault("RedriveAllowPolicy");

                return SqsRedriveMapper.ParseRedrive(redrivePolicy, redriveAllowPolicy);
            },
            cancellationToken);

    public async Task<Result> StartMessageRedriveAsync(string queueName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSQSClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var queueUrl = await ResolveQueueUrlAsync(client, queueName, token);

                var attributes = await client.GetQueueAttributesAsync(
                    new GetQueueAttributesRequest { QueueUrl = queueUrl, AttributeNames = ["QueueArn"] },
                    token);

                var sourceArn = attributes.Attributes!["QueueArn"];

                await client.StartMessageMoveTaskAsync(
                    new StartMessageMoveTaskRequest { SourceArn = sourceArn }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static async Task<string> ResolveQueueUrlAsync(
        AmazonSQSClient client, string queueName, CancellationToken cancellationToken)
    {
        var response = await client.GetQueueUrlAsync(
            new GetQueueUrlRequest { QueueName = queueName }, cancellationToken);
        return response.QueueUrl;
    }
}
