using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ValidateTemplate;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ValidateTemplate;

public class ValidateTemplateQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ValidateTemplateQueryHandler CreateSut()
        => new(_client, NullLogger<ValidateTemplateQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsValidationAndForwardsRequest()
    {
        // Arrange
        const string templateBody = "{\"Resources\":{}}";
        var validation = new TemplateValidationResult(
            "An example template",
            "Requires IAM",
            ["CAPABILITY_IAM"],
            [new TemplateValidationParameter("Env", "dev", false, "Environment name")]);
        _client
            .ValidateTemplateAsync(templateBody, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(validation)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ValidateTemplateQuery(templateBody, null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Validation.Should().Be(validation);
        result.Value.Validation.Description.Should().Be("An example template");
        result.Value.Validation.CapabilitiesReason.Should().Be("Requires IAM");
        result.Value.Validation.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
        var parameter = result.Value.Validation.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.DefaultValue.Should().Be("dev");
        parameter.NoEcho.Should().BeFalse();
        parameter.Description.Should().Be("Environment name");
        await _client.Received(1).ValidateTemplateAsync(templateBody, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ValidateTemplateAsync(null, "https://example.s3.amazonaws.com/template.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<TemplateValidationResult>>(new Error("invalid template")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ValidateTemplateQuery(null, "https://example.s3.amazonaws.com/template.json"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("invalid template");
    }
}
