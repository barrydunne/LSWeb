using Foundation.Application.Commands.UpdateAccessKeyStatus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateAccessKeyStatus;

public class UpdateAccessKeyStatusCommandValidatorTests
{
    private readonly UpdateAccessKeyStatusCommandValidator _sut =
        new(NullLogger<UpdateAccessKeyStatusCommandValidator>.Instance);

    private static UpdateAccessKeyStatusCommand Valid(
        string userName = "alice", string accessKeyId = "AKIAEXAMPLE", string status = "Active")
        => new(userName, accessKeyId, status);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    public async Task ValidateAsync_WhenStatusAllowed_IsValid(string status)
    {
        var result = await _sut.ValidateAsync(Valid(status: status), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(userName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccessKeyStatusCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAccessKeyIdEmpty_ReturnsErrorForAccessKeyId()
    {
        var result = await _sut.ValidateAsync(
            Valid(accessKeyId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccessKeyStatusCommand.AccessKeyId));
    }

    [Theory]
    [InlineData("Enabled")]
    [InlineData("active")]
    public async Task ValidateAsync_WhenStatusNotAllowed_ReturnsErrorForStatus(string status)
    {
        var result = await _sut.ValidateAsync(Valid(status: status), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccessKeyStatusCommand.Status));
    }
}
