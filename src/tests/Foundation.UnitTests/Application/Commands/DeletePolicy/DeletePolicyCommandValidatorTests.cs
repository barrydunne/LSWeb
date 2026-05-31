using Foundation.Application.Commands.DeletePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeletePolicy;

public class DeletePolicyCommandValidatorTests
{
    private const string Arn = "arn:aws:iam::000000000000:policy/deploy-policy";

    private readonly DeletePolicyCommandValidator _sut =
        new(NullLogger<DeletePolicyCommandValidator>.Instance);

    private static DeletePolicyCommand Valid(string policyArn = Arn)
        => new(policyArn);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeletePolicyCommand.PolicyArn));
    }
}
