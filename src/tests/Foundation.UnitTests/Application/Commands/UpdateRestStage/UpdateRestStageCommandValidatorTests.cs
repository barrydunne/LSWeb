using Foundation.Application.Commands.UpdateRestStage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateRestStage;

public class UpdateRestStageCommandValidatorTests
{
    private readonly UpdateRestStageCommandValidator _sut =
        new(NullLogger<UpdateRestStageCommandValidator>.Instance);

    private static UpdateRestStageCommand Valid(
        string restApiId = "api-1",
        string stageName = "dev")
        => new(restApiId, stageName, null, new Dictionary<string, string>());

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRestApiIdEmpty_ReturnsErrorForRestApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(restApiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRestStageCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameEmpty_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRestStageCommand.StageName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameTooLong_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRestStageCommand.StageName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStageNameHasInvalidCharacters_ReturnsErrorForStageName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stageName: "bad name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRestStageCommand.StageName));
    }
}
