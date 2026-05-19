using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaTestEventTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var testEvent = new LambdaTestEvent("My Event", "{\"key\":\"value\"}");

        // Assert
        testEvent.Name.Should().Be("My Event");
        testEvent.Payload.Should().Be("{\"key\":\"value\"}");
    }
}
