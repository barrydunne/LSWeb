using Foundation.Application.Queries.GetParameterHistory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetParameterHistory;

public class GetParameterHistoryQueryValidatorTests
{
    private readonly GetParameterHistoryQueryValidator _sut =
        new(NullLogger<GetParameterHistoryQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new GetParameterHistoryQuery("/app/config/key", false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new GetParameterHistoryQuery(string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(GetParameterHistoryQuery.Name));
    }
}
