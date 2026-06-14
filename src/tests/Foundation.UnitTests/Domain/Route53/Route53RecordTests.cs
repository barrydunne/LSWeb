using Foundation.Domain.Route53;

namespace Foundation.UnitTests.Domain.Route53;

public class Route53RecordTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var record = new Route53Record("www.example.com.", "A", 300, ["1.2.3.4", "5.6.7.8"]);

        // Assert
        record.Name.Should().Be("www.example.com.");
        record.Type.Should().Be("A");
        record.Ttl.Should().Be(300);
        record.Values.Should().Equal("1.2.3.4", "5.6.7.8");
    }
}
