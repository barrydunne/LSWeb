using Foundation.Application.Commands.DeleteLogGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLogGroup;

public class DeleteLogGroupCommandValidatorTests
{
    private readonly DeleteLogGroupCommandValidator _sut =
        new(NullLogger<DeleteLogGroupCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValidName_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLogGroupCommand("/app/orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLogGroupCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLogGroupCommand.LogGroupName));
    }
}
