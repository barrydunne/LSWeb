using Foundation.Application.Commands.CreateRoute53HostedZone;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRoute53HostedZone;

public class CreateRoute53HostedZoneCommandValidatorTests
{
    private readonly CreateRoute53HostedZoneCommandValidator _sut =
        new(NullLogger<CreateRoute53HostedZoneCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateRoute53HostedZoneCommand("example.com", "demo"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new CreateRoute53HostedZoneCommand(string.Empty, null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoute53HostedZoneCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameNotQualified_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new CreateRoute53HostedZoneCommand("example", null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoute53HostedZoneCommand.Name));
    }
}
