using Foundation.Application.Commands.DeleteScheduleGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteScheduleGroup;

public class DeleteScheduleGroupCommandValidatorTests
{
    private readonly DeleteScheduleGroupCommandValidator _sut =
        new(NullLogger<DeleteScheduleGroupCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteScheduleGroupCommand("reports"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteScheduleGroupCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteScheduleGroupCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameIsDefault_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteScheduleGroupCommand("default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(DeleteScheduleGroupCommand.Name)
            && _.ErrorMessage == "The default schedule group cannot be deleted.");
    }
}
