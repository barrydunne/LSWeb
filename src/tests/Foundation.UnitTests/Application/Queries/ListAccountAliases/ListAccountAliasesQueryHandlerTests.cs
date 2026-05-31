using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.ListAccountAliases;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListAccountAliases;

public class ListAccountAliasesQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private ListAccountAliasesQueryHandler CreateSut()
        => new(_client, NullLogger<ListAccountAliasesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAliases()
    {
        // Arrange
        _client
            .ListAccountAliasesAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<string>>(["my-account"]));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListAccountAliasesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Aliases.Should().ContainSingle().Which.Should().Be("my-account");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListAccountAliasesAsync(Arg.Any<CancellationToken>())
            .Returns(new Error("alias boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListAccountAliasesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("alias boom");
    }
}
