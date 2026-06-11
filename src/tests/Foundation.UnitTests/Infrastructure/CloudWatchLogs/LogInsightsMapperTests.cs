using Amazon.CloudWatchLogs.Model;
using Foundation.Infrastructure.CloudWatchLogs;

namespace Foundation.UnitTests.Infrastructure.CloudWatchLogs;

public class LogInsightsMapperTests
{
    [Fact]
    public void ToResult_MapsStatusRowsAndStatistics()
    {
        // Arrange
        var response = new GetQueryResultsResponse
        {
            Results =
            [
                [
                    new ResultField { Field = "@timestamp", Value = "2024-01-01" },
                    new ResultField { Field = "@message", Value = "hello" },
                ],
            ],
            Statistics = new QueryStatistics { RecordsMatched = 2.6, RecordsScanned = 10.4 },
        };

        // Act
        var result = LogInsightsMapper.ToResult("Complete", response);

        // Assert
        result.Status.Should().Be("Complete");
        result.Rows.Should().ContainSingle();
        result.Rows[0].Fields.Should().HaveCount(2);
        result.Rows[0].Fields[0].Field.Should().Be("@timestamp");
        result.Rows[0].Fields[0].Value.Should().Be("2024-01-01");
        result.Rows[0].Fields[1].Field.Should().Be("@message");
        result.Rows[0].Fields[1].Value.Should().Be("hello");
        result.RecordsMatched.Should().Be(3);
        result.RecordsScanned.Should().Be(10);
    }

    [Fact]
    public void ToResult_WhenResultsAndStatisticsNull_AppliesDefaults()
    {
        // Arrange
        var response = new GetQueryResultsResponse();

        // Act
        var result = LogInsightsMapper.ToResult("Timeout", response);

        // Assert
        result.Status.Should().Be("Timeout");
        result.Rows.Should().BeEmpty();
        result.RecordsMatched.Should().Be(0);
        result.RecordsScanned.Should().Be(0);
    }

    [Fact]
    public void ToResult_WhenFieldValuesNull_AppliesDefaults()
    {
        // Arrange
        var response = new GetQueryResultsResponse
        {
            Results = [[new ResultField()]],
        };

        // Act
        var result = LogInsightsMapper.ToResult("Complete", response);

        // Assert
        result.Rows.Should().ContainSingle();
        result.Rows[0].Fields[0].Field.Should().BeEmpty();
        result.Rows[0].Fields[0].Value.Should().BeEmpty();
    }

    [Fact]
    public void ToResult_WhenRowFieldsNull_ProducesEmptyRow()
    {
        // Arrange
        var response = new GetQueryResultsResponse
        {
            Results = [null!],
        };

        // Act
        var result = LogInsightsMapper.ToResult("Complete", response);

        // Assert
        result.Rows.Should().ContainSingle();
        result.Rows[0].Fields.Should().BeEmpty();
    }
}
