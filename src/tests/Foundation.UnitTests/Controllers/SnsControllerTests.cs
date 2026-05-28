using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Commands.DeleteSnsTopic;
using Foundation.Application.Queries.ListSnsSubscriptions;
using Foundation.Application.Queries.ListSnsTopics;
using Foundation.Domain.Sns;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SnsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SnsController> _logger = Substitute.For<ILogger<SnsController>>();

    private SnsController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListTopics_WhenQuerySucceeds_ReturnsOkWithTopics()
    {
        // Arrange
        IReadOnlyList<SnsTopic> topics =
        [
            new("orders-topic", "arn:aws:sns:eu-west-1:000000000000:orders-topic"),
        ];
        _sender
            .Send(Arg.Any<ListSnsTopicsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsTopicsQueryResult>>(
                new ListSnsTopicsQueryResult(topics)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTopics(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnsTopicListResponse>>().Subject;
        var topic = ok.Value!.Topics.Should().ContainSingle().Subject;
        topic.Name.Should().Be("orders-topic");
        topic.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task ListTopics_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSnsTopicsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsTopicsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTopics(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateTopic_WhenCommandSucceeds_ReturnsCreatedAndForwardsName()
    {
        // Arrange
        CreateSnsTopicCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateSnsTopicCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTopic(
            new SnsTopicCreateRequest("orders-topic"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("orders-topic");
    }

    [Fact]
    public async Task CreateTopic_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTopic(
            new SnsTopicCreateRequest("orders-topic"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteTopic_WhenCommandSucceeds_ReturnsNoContentAndForwardsArn()
    {
        // Arrange
        DeleteSnsTopicCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteSnsTopicCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTopic(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task DeleteTopic_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTopic(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListSubscriptions_WhenQuerySucceeds_ReturnsOkWithSubscriptions()
    {
        // Arrange
        ListSnsSubscriptionsQuery? captured = null;
        IReadOnlyList<SnsSubscription> subscriptions =
        [
            new(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f",
                "sqs",
                "arn:aws:sqs:eu-west-1:000000000000:orders",
                "000000000000"),
        ];
        _sender
            .Send(Arg.Do<ListSnsSubscriptionsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsSubscriptionsQueryResult>>(
                new ListSnsSubscriptionsQueryResult(subscriptions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnsSubscriptionListResponse>>().Subject;
        var subscription = ok.Value!.Subscriptions.Should().ContainSingle().Subject;
        subscription.SubscriptionArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f");
        subscription.Protocol.Should().Be("sqs");
        subscription.Endpoint.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders");
        subscription.Owner.Should().Be("000000000000");
        captured.Should().NotBeNull();
        captured!.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task ListSubscriptions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSnsSubscriptionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsSubscriptionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
