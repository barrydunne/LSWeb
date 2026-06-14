using Foundation.Application.Commands.DeleteRoute53Record;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteRoute53Record;

public class DeleteRoute53RecordCommandValidatorTests
{
    private readonly DeleteRoute53RecordCommandValidator _sut =
        new(NullLogger<DeleteRoute53RecordCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenZoneEmpty_ReturnsErrorForHostedZoneId()
    {
        var result = await _sut.ValidateAsync(
            new DeleteRoute53RecordCommand(string.Empty, "www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRoute53RecordCommand.HostedZoneId));
    }

    [Fact]
    public async Task ValidateAsync_WhenValuesEmpty_ReturnsErrorForValues()
    {
        var result = await _sut.ValidateAsync(
            new DeleteRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, []),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRoute53RecordCommand.Values));
    }
}
