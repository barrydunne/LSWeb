using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.ApplySeedTemplate;
using Foundation.Application.Queries.GetSeedTemplates;
using Foundation.Domain.Seed;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SeedControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SeedController> _logger = Substitute.For<ILogger<SeedController>>();

    [Fact]
    public async Task Templates_WhenQuerySucceeds_ReturnsOkWithMappedTemplates()
    {
        // Arrange
        var queryResult = new GetSeedTemplatesQueryResult(
        [
            new SeedTemplate("messaging-starter", "Messaging starter", "desc",
            [
                new SeedResourceDescriptor("sqs", "Queue", "seed-orders-queue"),
                new SeedResourceDescriptor("sns", "Topic", "seed-orders-topic"),
            ]),
        ]);
        _sender
            .Send(Arg.Any<GetSeedTemplatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSeedTemplatesQueryResult>>(queryResult));
        var sut = new SeedController(_sender, _logger);

        // Act
        var result = await sut.Templates(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SeedTemplatesResponse>>().Subject;
        var template = ok.Value!.Templates.Should().ContainSingle().Subject;
        template.Id.Should().Be("messaging-starter");
        template.Name.Should().Be("Messaging starter");
        template.Description.Should().Be("desc");
        template.Resources.Should().BeEquivalentTo(
            [
                new SeedResourceResponse("sqs", "Queue", "seed-orders-queue"),
                new SeedResourceResponse("sns", "Topic", "seed-orders-topic"),
            ],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Templates_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSeedTemplatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSeedTemplatesQueryResult>>(new Error("boom")));
        var sut = new SeedController(_sender, _logger);

        // Act
        var result = await sut.Templates(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Apply_WhenCommandSucceeds_ReturnsOkWithMappedOutcome()
    {
        // Arrange
        var outcome = new SeedOutcome("op-1", "messaging-starter",
        [
            new SeedResourceResult("sqs", "Queue", "seed-orders-queue", true, null),
            new SeedResourceResult("sns", "Topic", "seed-orders-topic", false, "boom"),
        ]);
        _sender
            .Send(Arg.Any<ApplySeedTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeedOutcome>>(outcome));
        var sut = new SeedController(_sender, _logger);

        // Act
        var result = await sut.Apply("messaging-starter", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SeedOutcomeResponse>>().Subject;
        ok.Value!.OperationId.Should().Be("op-1");
        ok.Value.TemplateId.Should().Be("messaging-starter");
        ok.Value.TotalCount.Should().Be(2);
        ok.Value.SucceededCount.Should().Be(1);
        ok.Value.FailedCount.Should().Be(1);
        ok.Value.OverallState.Should().Be("Failed");
        ok.Value.Items.Should().BeEquivalentTo(
            [
                new SeedResourceResultResponse("sqs", "Queue", "seed-orders-queue", true, null),
                new SeedResourceResultResponse("sns", "Topic", "seed-orders-topic", false, "boom"),
            ],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Apply_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        ApplySeedTemplateCommand? captured = null;
        _sender
            .Send(Arg.Do<ApplySeedTemplateCommand>(_ => captured = _), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeedOutcome>>(new Error("boom")));
        var sut = new SeedController(_sender, _logger);

        // Act
        var result = await sut.Apply("nope", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
        captured!.TemplateId.Should().Be("nope");
    }
}
