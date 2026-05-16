using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.ExecuteBulkAction;
using Foundation.Domain.Bulk;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class BulkControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<BulkController> _logger = Substitute.For<ILogger<BulkController>>();

    [Fact]
    public async Task Execute_WhenCommandSucceeds_ReturnsOkWithMappedOutcome()
    {
        // Arrange
        var outcome = new BulkActionOutcome("op-1", "delete",
        [
            new BulkActionItemResult("a", true, null),
            new BulkActionItemResult("b", false, "Resource id is required."),
        ]);
        _sender
            .Send(Arg.Any<ExecuteBulkActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BulkActionOutcome>>(outcome));
        var sut = new BulkController(_sender, _logger);

        // Act
        var result = await sut.Execute(
            "delete", new BulkActionRequest(["a", "b"]), TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<BulkActionResponse>>().Subject;
        ok.Value!.OperationId.Should().Be("op-1");
        ok.Value.Action.Should().Be("delete");
        ok.Value.TotalCount.Should().Be(2);
        ok.Value.SucceededCount.Should().Be(1);
        ok.Value.FailedCount.Should().Be(1);
        ok.Value.OverallState.Should().Be("Failed");
        ok.Value.Items.Should().HaveCount(2);
        ok.Value.Items[1].Error.Should().Be("Resource id is required.");
    }

    [Fact]
    public async Task Execute_WhenResourceIdsNull_DispatchesEmptyListAndReturnsOk()
    {
        // Arrange
        ExecuteBulkActionCommand? captured = null;
        _sender
            .Send(Arg.Do<ExecuteBulkActionCommand>(_ => captured = _), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BulkActionOutcome>>(
                new BulkActionOutcome("op-2", "delete", [])));
        var sut = new BulkController(_sender, _logger);

        // Act
        var result = await sut.Execute(
            "delete", new BulkActionRequest(null!), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Ok<BulkActionResponse>>();
        captured.Should().NotBeNull();
        captured!.ResourceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteBulkActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BulkActionOutcome>>(new Error("boom")));
        var sut = new BulkController(_sender, _logger);

        // Act
        var result = await sut.Execute(
            "delete", new BulkActionRequest(["a"]), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
