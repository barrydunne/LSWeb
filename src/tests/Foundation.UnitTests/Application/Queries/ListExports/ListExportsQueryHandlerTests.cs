using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListExports;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListExports;

public class ListExportsQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListExportsQueryHandler CreateSut()
        => new(_client, NullLogger<ListExportsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static StackExport BuildExport()
        => new(
            "shared-vpc-id",
            "vpc-12345",
            "arn:aws:cloudformation:eu-west-1:000000000000:stack/network/abc");

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsExports()
    {
        // Arrange
        IReadOnlyList<StackExport> exports = [BuildExport()];
        var success = Ok(exports);
        _client
            .ListExportsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(success));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListExportsQuery(),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Exports.Should().BeSameAs(exports);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListExportsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StackExport>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListExportsQuery(),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
