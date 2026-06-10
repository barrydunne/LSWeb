using Foundation.Domain.ApiGateway;

namespace Foundation.UnitTests.Domain.ApiGateway;

public class RestMethodTestInvocationSpecificationTests
{
    private static RestMethodTestInvocationSpecification Create(
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string>? queryStringParameters = null,
        IReadOnlyDictionary<string, string>? stageVariables = null)
        => new(
            "api-1",
            "res-1",
            "GET",
            "/orders?id=1",
            headers ?? new Dictionary<string, string> { ["h"] = "1" },
            queryStringParameters ?? new Dictionary<string, string> { ["q"] = "1" },
            "body",
            stageVariables ?? new Dictionary<string, string> { ["s"] = "1" });

    [Fact]
    public void ExposesAllProperties()
    {
        var spec = Create();

        spec.RestApiId.Should().Be("api-1");
        spec.ResourceId.Should().Be("res-1");
        spec.HttpMethod.Should().Be("GET");
        spec.PathWithQueryString.Should().Be("/orders?id=1");
        spec.Headers.Should().ContainKey("h");
        spec.QueryStringParameters.Should().ContainKey("q");
        spec.Body.Should().Be("body");
        spec.StageVariables.Should().ContainKey("s");
    }

    [Fact]
    public void SupportsValueEquality()
    {
        var spec = Create();
        var same = spec with { };

        spec.Should().Be(same);
        spec.GetHashCode().Should().Be(same.GetHashCode());
        spec.ToString().Should().Contain("GET");
        (spec with { RestApiId = "other" }).Should().NotBe(spec);
        (spec with { ResourceId = "other" }).Should().NotBe(spec);
        (spec with { HttpMethod = "POST" }).Should().NotBe(spec);
        (spec with { PathWithQueryString = "/other" }).Should().NotBe(spec);
        (spec with { Headers = new Dictionary<string, string>() }).Should().NotBe(spec);
        (spec with { QueryStringParameters = new Dictionary<string, string>() }).Should().NotBe(spec);
        (spec with { Body = "other" }).Should().NotBe(spec);
        (spec with { StageVariables = new Dictionary<string, string>() }).Should().NotBe(spec);
    }
}
