using Foundation.Application.Commands.DeleteUser;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteUser;

public class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _sut =
        new(NullLogger<DeleteUserCommandValidator>.Instance);

    private static DeleteUserCommand Valid(string name = "alice")
        => new(name);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserCommand.UserName));
    }
}
