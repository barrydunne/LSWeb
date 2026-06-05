using Foundation.Application.Commands.DeleteSchedule;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSchedule;

public class DeleteScheduleCommandValidatorTests
{
    private readonly DeleteScheduleCommandValidator _sut =
        new(NullLogger<DeleteScheduleCommandValidator>.Instance);

    private static DeleteScheduleCommand Valid(
        string name = "daily-job",
        string groupName = "default")
        => new(name, groupName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteScheduleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(groupName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteScheduleCommand.GroupName));
    }
}
