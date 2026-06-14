using Foundation.Application.Commands.UpsertRoute53Record;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpsertRoute53Record;

public class UpsertRoute53RecordCommandValidatorTests
{
    private readonly UpsertRoute53RecordCommandValidator _sut =
        new(NullLogger<UpsertRoute53RecordCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeUnknown_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "SRV", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpsertRoute53RecordCommand.Type));
    }

    [Fact]
    public async Task ValidateAsync_WhenTtlNotPositive_ReturnsErrorForTtl()
    {
        var result = await _sut.ValidateAsync(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 0, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpsertRoute53RecordCommand.Ttl));
    }

    [Fact]
    public async Task ValidateAsync_WhenValuesEmpty_ReturnsErrorForValues()
    {
        var result = await _sut.ValidateAsync(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, []),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpsertRoute53RecordCommand.Values));
    }

    [Fact]
    public async Task ValidateAsync_WhenValueBlank_ReturnsErrorForValues()
    {
        var result = await _sut.ValidateAsync(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, ["   "]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpsertRoute53RecordCommand.Values));
    }
}
