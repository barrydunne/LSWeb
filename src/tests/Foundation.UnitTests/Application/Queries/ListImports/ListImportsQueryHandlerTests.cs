using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListImports;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListImports;

public class ListImportsQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListImportsQueryHandler CreateSut()
        => new(_client, NullLogger<ListImportsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsImportingStacks()
    {
        // Arrange
        IReadOnlyList<string> stacks = ["orders-stack", "billing-stack"];
        var success = Ok(stacks);
        _client
            .ListImportsAsync("shared-vpc-id", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(success));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListImportsQuery("shared-vpc-id"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImportingStackNames.Should().BeSameAs(stacks);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListImportsAsync("shared-vpc-id", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<string>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListImportsQuery("shared-vpc-id"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
