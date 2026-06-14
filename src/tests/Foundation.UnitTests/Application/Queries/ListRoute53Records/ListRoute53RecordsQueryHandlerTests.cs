using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListRoute53Records;
using Foundation.Application.Route53;
using Foundation.Domain.Route53;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRoute53Records;

public class ListRoute53RecordsQueryHandlerTests
{
    private readonly IRoute53Client _client = Substitute.For<IRoute53Client>();

    private ListRoute53RecordsQueryHandler CreateSut()
        => new(_client, NullLogger<ListRoute53RecordsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRecords()
    {
        // Arrange
        IReadOnlyList<Route53Record> records = [new("www.example.com.", "A", 300, ["1.2.3.4"])];
        _client
            .ListRecordsAsync("/hostedzone/Z1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<Route53Record>>(records)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRoute53RecordsQuery("/hostedzone/Z1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Records.Should().ContainSingle();
        result.Value.Records[0].Name.Should().Be("www.example.com.");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListRecordsAsync("/hostedzone/Z1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Route53Record>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRoute53RecordsQuery("/hostedzone/Z1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
