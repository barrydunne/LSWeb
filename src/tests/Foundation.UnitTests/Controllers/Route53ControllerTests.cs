using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateRoute53HostedZone;
using Foundation.Application.Commands.DeleteRoute53Record;
using Foundation.Application.Commands.UpsertRoute53Record;
using Foundation.Application.Queries.ListHostedZones;
using Foundation.Application.Queries.ListRoute53Records;
using Foundation.Domain.Route53;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class Route53ControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<Route53Controller> _logger =
        Substitute.For<ILogger<Route53Controller>>();

    private Route53Controller CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListHostedZones_WhenQuerySucceeds_ReturnsOkWithHostedZones()
    {
        // Arrange
        IReadOnlyList<HostedZone> hostedZones =
        [
            new("/hostedzone/Z123", "example.com.", 4, true),
        ];
        _sender
            .Send(Arg.Any<ListHostedZonesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHostedZonesQueryResult>>(
                new ListHostedZonesQueryResult(hostedZones)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListHostedZones(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HostedZoneListResponse>>().Subject;
        var zone = ok.Value!.HostedZones.Should().ContainSingle().Subject;
        zone.Id.Should().Be("/hostedzone/Z123");
        zone.Name.Should().Be("example.com.");
        zone.RecordCount.Should().Be(4);
        zone.PrivateZone.Should().BeTrue();
    }

    [Fact]
    public async Task ListHostedZones_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHostedZonesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHostedZonesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListHostedZones(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateHostedZone_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRoute53HostedZoneCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateHostedZone(
            new HostedZoneCreateRequest("example.com", "demo"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/route53/hostedzones");
        await _sender.Received(1).Send(
            Arg.Is<CreateRoute53HostedZoneCommand>(command =>
                command.Name == "example.com" && command.Comment == "demo"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHostedZone_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRoute53HostedZoneCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateHostedZone(
            new HostedZoneCreateRequest("example.com", null), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRecords_WhenQuerySucceeds_ReturnsOkWithRecords()
    {
        // Arrange
        IReadOnlyList<Route53Record> records = [new("www.example.com.", "A", 300, ["1.2.3.4"])];
        _sender
            .Send(Arg.Any<ListRoute53RecordsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRoute53RecordsQueryResult>>(
                new ListRoute53RecordsQueryResult(records)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRecords("/hostedzone/Z1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<Route53RecordListResponse>>().Subject;
        var record = ok.Value!.Records.Should().ContainSingle().Subject;
        record.Name.Should().Be("www.example.com.");
        record.Type.Should().Be("A");
        record.Ttl.Should().Be(300);
        record.Values.Should().Equal("1.2.3.4");
    }

    [Fact]
    public async Task ListRecords_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRoute53RecordsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRoute53RecordsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRecords("/hostedzone/Z1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpsertRecord_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpsertRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpsertRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpsertRoute53RecordCommand>(command =>
                command.HostedZoneId == "/hostedzone/Z1" && command.Name == "www.example.com."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertRecord_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpsertRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpsertRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRecord_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteRecord_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpsertRecord_WhenValuesNull_SendsEmptyValues()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpsertRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpsertRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, null!),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpsertRoute53RecordCommand>(command => command.Values.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRecord_WhenValuesNull_SendsEmptyValues()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRoute53RecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRecord(
            "/hostedzone/Z1",
            new Route53RecordRequest("www.example.com.", "A", 300, null!),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRoute53RecordCommand>(command => command.Values.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
