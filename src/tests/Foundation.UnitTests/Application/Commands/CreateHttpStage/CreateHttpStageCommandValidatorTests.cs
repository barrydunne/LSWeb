using Foundation.Application.Commands.CreateHttpStage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpStage;

public class CreateHttpStageCommandValidatorTests
{
    private readonly CreateHttpStageCommandValidator _sut =
        new(NullLogger<CreateHttpStageCommandValidator>.Instance);

    private static CreateHttpStageCommand Valid(
        string apiId = "abc123",
        string stageName = "dev",
        int? burstLimit = 100,
        double? rateLimit = 50.0)
        => new(
            apiId,
            stageName,
            true,
            "Development stage",
            burstLimit,
            rateLimit,
            new Dictionary<string, string>());

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenThrottlingLimitsNull_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(burstLimit: null, rateLimit: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenApiIdEmpty_ReturnsErrorForApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(apiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameEmpty_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.StageName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameTooLong_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.StageName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameHasInvalidCharacters_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.StageName));
    }

    [Fact]
    public async Task ValidateAsync_WhenBurstLimitNegative_ReturnsErrorForBurstLimit()
    {
        var result = await _sut.ValidateAsync(
            Valid(burstLimit: -1), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.DefaultRouteThrottlingBurstLimit));
    }

    [Fact]
    public async Task ValidateAsync_WhenRateLimitNegative_ReturnsErrorForRateLimit()
    {
        var result = await _sut.ValidateAsync(
            Valid(rateLimit: -1.0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpStageCommand.DefaultRouteThrottlingRateLimit));
    }
}
