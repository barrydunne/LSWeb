using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.GetStackTemplate;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetStackTemplate;

public class GetStackTemplateQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private GetStackTemplateQueryHandler CreateSut()
        => new(_client, NullLogger<GetStackTemplateQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTemplate()
    {
        // Arrange
        const string stackName = "orders-stack";
        var template = new CloudFormationStackTemplate(
            "{\"Resources\":{}}",
            "json");
        _client
            .GetTemplateAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(template)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStackTemplateQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Template.Should().Be(template);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string stackName = "orders-stack";
        _client
            .GetTemplateAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CloudFormationStackTemplate>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStackTemplateQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
