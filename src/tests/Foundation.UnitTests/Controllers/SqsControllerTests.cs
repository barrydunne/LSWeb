using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSqsQueue;
using Foundation.Application.Commands.ChangeSqsMessageVisibility;
using Foundation.Application.Commands.DeleteSqsMessage;
using Foundation.Application.Commands.DeleteSqsQueue;
using Foundation.Application.Commands.PurgeSqsQueue;
using Foundation.Application.Commands.RedriveSqsMessages;
using Foundation.Application.Commands.SendSqsMessage;
using Foundation.Application.Commands.SetSqsQueueAttributes;
using Foundation.Application.Commands.SetSqsRedrivePolicy;
using Foundation.Application.Queries.GetSqsQueueAttributes;
using Foundation.Application.Queries.GetSqsQueueRedrive;
using Foundation.Application.Queries.ListSqsConsumerLambdas;
using Foundation.Application.Queries.ListSqsMessages;
using Foundation.Application.Queries.ListSqsQueues;
using Foundation.Application.Queries.ListSqsSubscriptions;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SqsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SqsController> _logger = Substitute.For<ILogger<SqsController>>();

    private SqsController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListQueues_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        IReadOnlyList<SqsQueue> queues =
        [
            new("orders", "http://localhost:4566/000000000000/orders", 3, 1, 2),
        ];
        _sender
            .Send(Arg.Any<ListSqsQueuesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsQueuesQueryResult>>(
                new ListSqsQueuesQueryResult(queues)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListQueues(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsQueueListResponse>>().Subject;
        var summary = ok.Value!.Queues.Should().ContainSingle().Subject;
        summary.Name.Should().Be("orders");
        summary.Url.Should().Be("http://localhost:4566/000000000000/orders");
        summary.ApproximateMessageCount.Should().Be(3);
        summary.ApproximateInFlightCount.Should().Be(1);
        summary.ApproximateDelayedCount.Should().Be(2);
    }

    [Fact]
    public async Task ListQueues_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSqsQueuesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsQueuesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListQueues(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PollMessages_WhenQuerySucceeds_ReturnsOkWithMessages()
    {
        // Arrange
        IReadOnlyList<SqsMessage> messages =
        [
            new("id-1", "receipt-1", "body",
                new Dictionary<string, string> { ["SentTimestamp"] = "1" },
                new Dictionary<string, string> { ["trace"] = "abc" }),
        ];
        _sender
            .Send(Arg.Any<ListSqsMessagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsMessagesQueryResult>>(
                new ListSqsMessagesQueryResult(messages)));
        var sut = CreateSut();

        // Act
        var result = await sut.PollMessages("orders", "peek", 10, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsMessageListResponse>>().Subject;
        var message = ok.Value!.Messages.Should().ContainSingle().Subject;
        message.MessageId.Should().Be("id-1");
        message.ReceiptHandle.Should().Be("receipt-1");
        message.Body.Should().Be("body");
        message.Attributes.Should().ContainKey("SentTimestamp");
        message.MessageAttributes.Should().ContainKey("trace");
    }

    [Fact]
    public async Task PollMessages_WhenModeConsume_SendsConsumeQueryWithDefaultMax()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSqsMessagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsMessagesQueryResult>>(
                new ListSqsMessagesQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.PollMessages("orders", "consume", 0, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ListSqsMessagesQuery>(query =>
                query.Mode == SqsPollMode.Consume && query.MaxMessages == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PollMessages_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSqsMessagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsMessagesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PollMessages("orders", null, 5, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteMessage_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSqsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteMessage("orders", "receipt-1", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteMessage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSqsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteMessage("orders", "receipt-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PurgeQueue_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PurgeSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PurgeQueue("orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<PurgeSqsQueueCommand>(command => command.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeQueue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PurgeSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PurgeQueue("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SendMessage_WhenCommandSucceeds_ReturnsAcceptedAndForwardsRequest()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SendSqsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = new SqsSendMessageRequest(
            "hello",
            new Dictionary<string, string> { ["source"] = "test" },
            "group-1",
            "dedup-1");

        // Act
        var result = await sut.SendMessage("orders.fifo", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Accepted>();
        await _sender.Received(1).Send(
            Arg.Is<SendSqsMessageCommand>(command =>
                command.QueueName == "orders.fifo"
                && command.Body == "hello"
                && command.MessageAttributes["source"] == "test"
                && command.MessageGroupId == "group-1"
                && command.MessageDeduplicationId == "dedup-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessage_WhenAttributesNull_SendsEmptyAttributes()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SendSqsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = new SqsSendMessageRequest("hello", null, null, null);

        // Act
        var result = await sut.SendMessage("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Accepted>();
        await _sender.Received(1).Send(
            Arg.Is<SendSqsMessageCommand>(command => command.MessageAttributes.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SendSqsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();
        var request = new SqsSendMessageRequest("hello", null, null, null);

        // Act
        var result = await sut.SendMessage("orders", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListSubscriptions_WhenQuerySucceeds_ReturnsOkWithSubscriptions()
    {
        // Arrange
        IReadOnlyList<SqsQueueSubscription> subscriptions =
        [
            new("arn:aws:sns:eu-west-1:000000000000:order-events", "order-events"),
        ];
        _sender
            .Send(Arg.Any<ListSqsSubscriptionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsSubscriptionsQueryResult>>(
                new ListSqsSubscriptionsQueryResult(subscriptions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsSubscriptionListResponse>>().Subject;
        var subscription = ok.Value!.Subscriptions.Should().ContainSingle().Subject;
        subscription.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:order-events");
        subscription.TopicName.Should().Be("order-events");
        await _sender.Received(1).Send(
            Arg.Is<ListSqsSubscriptionsQuery>(query => query.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListSubscriptions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSqsSubscriptionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsSubscriptionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListConsumerLambdas_WhenQuerySucceeds_ReturnsOkWithLambdas()
    {
        // Arrange
        IReadOnlyList<SqsConsumerLambda> lambdas =
        [
            new("order-processor", "arn:aws:lambda:eu-west-1:000000000000:function:order-processor", "Enabled"),
        ];
        _sender
            .Send(Arg.Any<ListSqsConsumerLambdasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsConsumerLambdasQueryResult>>(
                new ListSqsConsumerLambdasQueryResult(lambdas)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListConsumerLambdas("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsConsumerLambdaListResponse>>().Subject;
        var lambda = ok.Value!.Lambdas.Should().ContainSingle().Subject;
        lambda.FunctionName.Should().Be("order-processor");
        lambda.FunctionArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:order-processor");
        lambda.State.Should().Be("Enabled");
        await _sender.Received(1).Send(
            Arg.Is<ListSqsConsumerLambdasQuery>(query => query.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListConsumerLambdas_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSqsConsumerLambdasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSqsConsumerLambdasQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListConsumerLambdas("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateQueue_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateQueue(
            new SqsQueueCreateRequest("orders", false), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<CreateSqsQueueCommand>(command => command.QueueName == "orders" && !command.FifoQueue),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateQueue_WhenFifoRequested_SendsFifoCommand()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        await sut.CreateQueue(
            new SqsQueueCreateRequest("orders.fifo", true), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<CreateSqsQueueCommand>(command => command.QueueName == "orders.fifo" && command.FifoQueue),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateQueue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateQueue(
            new SqsQueueCreateRequest("orders", false), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteQueue_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteQueue("orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteSqsQueueCommand>(command => command.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteQueue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSqsQueueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteQueue("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetAttributes_WhenQuerySucceeds_ReturnsOkWithAttributes()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSqsQueueAttributesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSqsQueueAttributesQueryResult>>(
                new GetSqsQueueAttributesQueryResult(
                    new SqsQueueAttributes(
                        45, 86400, 10, 5, 262144, "arn:aws:sqs:eu-west-1:000000000000:orders", false, 7, 3, 2))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAttributes("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsQueueAttributesResponse>>().Subject;
        ok.Value!.VisibilityTimeoutSeconds.Should().Be(45);
        ok.Value.MessageRetentionPeriodSeconds.Should().Be(86400);
        ok.Value.DelaySeconds.Should().Be(10);
        ok.Value.ReceiveMessageWaitTimeSeconds.Should().Be(5);
        ok.Value.MaximumMessageSizeBytes.Should().Be(262144);
        ok.Value.QueueArn.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders");
        ok.Value.FifoQueue.Should().BeFalse();
        ok.Value.ApproximateMessageCount.Should().Be(7);
        ok.Value.ApproximateInFlightCount.Should().Be(3);
        ok.Value.ApproximateDelayedCount.Should().Be(2);
        await _sender.Received(1).Send(
            Arg.Is<GetSqsQueueAttributesQuery>(query => query.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAttributes_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSqsQueueAttributesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSqsQueueAttributesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAttributes("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetAttributes_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetSqsQueueAttributesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = new SqsQueueAttributesUpdateRequest(45, 86400, 10, 5);

        // Act
        var result = await sut.SetAttributes("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<SetSqsQueueAttributesCommand>(command =>
                command.QueueName == "orders"
                && command.VisibilityTimeoutSeconds == 45
                && command.MessageRetentionPeriodSeconds == 86400
                && command.DelaySeconds == 10
                && command.ReceiveMessageWaitTimeSeconds == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAttributes_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetSqsQueueAttributesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();
        var request = new SqsQueueAttributesUpdateRequest(45, 86400, 10, 5);

        // Act
        var result = await sut.SetAttributes("orders", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ChangeMessageVisibility_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ChangeSqsMessageVisibilityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.ChangeMessageVisibility(
            "orders", new SqsChangeMessageVisibilityRequest("rh", 60), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<ChangeSqsMessageVisibilityCommand>(command =>
                command.QueueName == "orders"
                && command.ReceiptHandle == "rh"
                && command.VisibilityTimeoutSeconds == 60),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeMessageVisibility_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ChangeSqsMessageVisibilityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ChangeMessageVisibility(
            "orders", new SqsChangeMessageVisibilityRequest("rh", 60), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetRedrivePolicy_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetSqsRedrivePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetRedrivePolicy(
            "orders",
            new SqsRedrivePolicyRequest("arn:aws:sqs:eu-west-1:000000000000:orders-dlq", 5),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<SetSqsRedrivePolicyCommand>(command =>
                command.QueueName == "orders"
                && command.DeadLetterTargetArn == "arn:aws:sqs:eu-west-1:000000000000:orders-dlq"
                && command.MaxReceiveCount == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRedrivePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetSqsRedrivePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetRedrivePolicy(
            "orders",
            new SqsRedrivePolicyRequest("arn:aws:sqs:eu-west-1:000000000000:orders-dlq", 5),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRedrive_WhenQuerySucceeds_ReturnsOkWithRedrive()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSqsQueueRedriveQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSqsQueueRedriveQueryResult>>(
                new GetSqsQueueRedriveQueryResult(
                    new SqsRedrive(
                        new SqsRedriveTarget("arn:aws:sqs:eu-west-1:000000000000:orders-dlq", "orders-dlq", 5),
                        [new SqsRedriveSource("arn:aws:sqs:eu-west-1:000000000000:orders", "orders")]))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRedrive("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsRedriveResponse>>().Subject;
        ok.Value!.DeadLetterTarget!.QueueName.Should().Be("orders-dlq");
        ok.Value.DeadLetterTarget.MaxReceiveCount.Should().Be(5);
        ok.Value.Sources.Should().ContainSingle(_ => _.QueueName == "orders");
        await _sender.Received(1).Send(
            Arg.Is<GetSqsQueueRedriveQuery>(query => query.QueueName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRedrive_WhenNoDeadLetterTarget_ReturnsOkWithNullTarget()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSqsQueueRedriveQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSqsQueueRedriveQueryResult>>(
                new GetSqsQueueRedriveQueryResult(new SqsRedrive(null, []))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRedrive("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SqsRedriveResponse>>().Subject;
        ok.Value!.DeadLetterTarget.Should().BeNull();
        ok.Value.Sources.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRedrive_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSqsQueueRedriveQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSqsQueueRedriveQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRedrive("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Redrive_WhenCommandSucceeds_ReturnsAcceptedAndForwardsRequest()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RedriveSqsMessagesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Redrive("orders-dlq", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Accepted>();
        await _sender.Received(1).Send(
            Arg.Is<RedriveSqsMessagesCommand>(command => command.QueueName == "orders-dlq"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Redrive_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RedriveSqsMessagesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Redrive("orders-dlq", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
