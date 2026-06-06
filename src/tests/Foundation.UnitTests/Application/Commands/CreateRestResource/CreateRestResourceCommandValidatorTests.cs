using Foundation.Application.Commands.CreateRestResource;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRestResource;

public class CreateRestResourceCommandValidatorTests
{
    private readonly CreateRestResourceCommandValidator _sut =
        new(NullLogger<CreateRestResourceCommandValidator>.Instance);

    private static CreateRestResourceCommand Valid(
        string restApiId = "api-1",
        string parentId = "res-1",
        string pathPart = "items")
        => new(restApiId, parentId, pathPart);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestResourceCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenParentIdEmpty_ReturnsErrorForParentId()
    {
        var result = await _sut.ValidateAsync(
            Valid(parentId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestResourceCommand.ParentId));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathPartEmpty_ReturnsErrorForPathPart()
    {
        var result = await _sut.ValidateAsync(
            Valid(pathPart: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestResourceCommand.PathPart));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathPartTooLong_ReturnsErrorForPathPart()
    {
        var result = await _sut.ValidateAsync(
            Valid(pathPart: new string('a', 513)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestResourceCommand.PathPart));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathPartContainsSlash_ReturnsErrorForPathPart()
    {
        var result = await _sut.ValidateAsync(
            Valid(pathPart: "items/list"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestResourceCommand.PathPart));
    }
}
