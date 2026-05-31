using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetAccountSummary;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetAccountSummary;

public class GetAccountSummaryQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetAccountSummaryQueryHandler CreateSut()
        => new(_client, NullLogger<GetAccountSummaryQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSummary()
    {
        // Arrange
        var summary = new IamAccountSummary(
            new Dictionary<string, int> { ["Users"] = 3, ["Roles"] = 7 });
        _client
            .GetAccountSummaryAsync(Arg.Any<CancellationToken>())
            .Returns(Ok(summary));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetAccountSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Summary.Entries.Should().BeEquivalentTo(summary.Entries);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetAccountSummaryAsync(Arg.Any<CancellationToken>())
            .Returns(new Error("summary boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetAccountSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("summary boom");
    }
}
