using Foundation.Application.Commands.DeleteGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteGroup;

public class DeleteGroupCommandValidatorTests
{
    private readonly DeleteGroupCommandValidator _sut =
        new(NullLogger<DeleteGroupCommandValidator>.Instance);

    private static DeleteGroupCommand Valid(string name = "developers")
        => new(name);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteGroupCommand.GroupName));
    }
}
