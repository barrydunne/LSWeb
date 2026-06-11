using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CertificateManager;
using Foundation.Application.Commands.RequestCertificate;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.CertificateManager;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RequestCertificate;

public class RequestCertificateCommandHandlerTests
{
    private readonly ICertificateManagerClient _client = Substitute.For<ICertificateManagerClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private const string Arn = "arn:aws:acm:eu-west-1:000000000000:certificate/new";

    private static RequestCertificateCommand BuildCommand()
        => new("example.com", "DNS", ["www.example.com"]);

    private RequestCertificateCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<RequestCertificateCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenRequestSucceeds_PublishesSuccessRefreshesSearchAndReturnsArn()
    {
        // Arrange
        _client
            .RequestCertificateAsync(Arg.Any<CertificateRequestSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(Arn));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Arn);
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenRequestFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .RequestCertificateAsync(Arg.Any<CertificateRequestSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("request boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("request boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .RequestCertificateAsync(Arg.Any<CertificateRequestSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(Arn));
        var command = new RequestCertificateCommand("example.com", "EMAIL", ["a.example.com", "b.example.com"]);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).RequestCertificateAsync(
            Arg.Is<CertificateRequestSpecification>(spec =>
                spec.DomainName == "example.com"
                && spec.ValidationMethod == "EMAIL"
                && spec.SubjectAlternativeNames.Count == 2
                && spec.SubjectAlternativeNames[0] == "a.example.com"
                && spec.SubjectAlternativeNames[1] == "b.example.com"),
            Arg.Any<CancellationToken>());
    }
}
