using Foundation.Application.Queries.GetParameterValue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetParameterValue;

public class GetParameterValueQueryValidatorTests
{
    private readonly GetParameterValueQueryValidator _sut =
        new(NullLogger<GetParameterValueQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new GetParameterValueQuery("/app/config/key", false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new GetParameterValueQuery(string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(GetParameterValueQuery.Name));
    }
}
