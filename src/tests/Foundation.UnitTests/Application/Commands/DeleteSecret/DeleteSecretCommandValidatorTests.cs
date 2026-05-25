using Foundation.Application.Commands.DeleteSecret;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSecret;

public class DeleteSecretCommandValidatorTests
{
    private readonly DeleteSecretCommandValidator _sut =
        new(NullLogger<DeleteSecretCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValidSecretId_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSecretCommand("db-password"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretIdEmpty_ReturnsErrorForSecretId()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSecretCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSecretCommand.SecretId));
    }
}
