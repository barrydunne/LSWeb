using Foundation.Application.Commands.CreateScheduleGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateScheduleGroup;

public class CreateScheduleGroupCommandValidatorTests
{
    private readonly CreateScheduleGroupCommandValidator _sut =
        new(NullLogger<CreateScheduleGroupCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateScheduleGroupCommand("reports.daily-2"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new CreateScheduleGroupCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateScheduleGroupCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new CreateScheduleGroupCommand(new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateScheduleGroupCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new CreateScheduleGroupCommand("bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(CreateScheduleGroupCommand.Name)
            && _.ErrorMessage == "Schedule group names may only contain letters, digits, and the characters '-', '_', and '.'.");
    }
}
