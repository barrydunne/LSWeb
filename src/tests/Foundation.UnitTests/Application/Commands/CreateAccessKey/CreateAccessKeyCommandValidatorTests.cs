using Foundation.Application.Commands.CreateAccessKey;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateAccessKey;

public class CreateAccessKeyCommandValidatorTests
{
    private readonly CreateAccessKeyCommandValidator _sut =
        new(NullLogger<CreateAccessKeyCommandValidator>.Instance);

    private static CreateAccessKeyCommand Valid(string userName = "alice")
        => new(userName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(userName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateAccessKeyCommand.UserName));
    }
}
