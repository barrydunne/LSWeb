using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SqsResourceSourceTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private SqsResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsSqs()
        => CreateSut().ServiceKey.Should().Be("sqs");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsQueuesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<SqsQueue> queues =
        [
            new("orders", "http://localhost:4566/000000000000/orders", 3, 1, 0),
            new("invoices", "http://localhost:4566/000000000000/invoices", 0, 0, 0),
        ];
        _client
            .ListQueuesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(queues)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().HaveCount(2);
        entries[0].ServiceKey.Should().Be("sqs");
        entries[0].ResourceId.Should().Be("orders");
        entries[0].DisplayName.Should().Be("orders");
        entries[0].Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListQueuesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SqsQueue>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
