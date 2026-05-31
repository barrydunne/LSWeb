using Foundation.Application.Commands.CreatePolicyVersion;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreatePolicyVersion;

public class CreatePolicyVersionCommandValidatorTests
{
    private const string Arn = "arn:aws:iam::000000000000:policy/deploy-policy";
    private const string ValidDocument = "{\"Version\":\"2012-10-17\",\"Statement\":[]}";

    private readonly CreatePolicyVersionCommandValidator _sut =
        new(NullLogger<CreatePolicyVersionCommandValidator>.Instance);

    private static CreatePolicyVersionCommand Valid(
        string policyArn = Arn,
        string document = ValidDocument,
        bool setAsDefault = true)
        => new(policyArn, document, setAsDefault);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyVersionCommand.PolicyArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(document: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyVersionCommand.PolicyDocument));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public async Task ValidateAsync_WhenDocumentNotJsonObject_ReturnsErrorForPolicyDocument(string document)
    {
        var result = await _sut.ValidateAsync(Valid(document: document), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyVersionCommand.PolicyDocument));
    }
}
