using Foundation.Application.Commands.DeleteAccessKey;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteAccessKey;

public class DeleteAccessKeyCommandValidatorTests
{
    private readonly DeleteAccessKeyCommandValidator _sut =
        new(NullLogger<DeleteAccessKeyCommandValidator>.Instance);

    private static DeleteAccessKeyCommand Valid(string userName = "alice", string accessKeyId = "AKIAEXAMPLE")
        => new(userName, accessKeyId);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteAccessKeyCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAccessKeyIdEmpty_ReturnsErrorForAccessKeyId()
    {
        var result = await _sut.ValidateAsync(
            Valid(accessKeyId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteAccessKeyCommand.AccessKeyId));
    }
}
