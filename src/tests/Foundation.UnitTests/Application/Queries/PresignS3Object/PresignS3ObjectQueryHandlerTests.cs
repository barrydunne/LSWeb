using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.PresignS3Object;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.PresignS3Object;

public class PresignS3ObjectQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private PresignS3ObjectQueryHandler CreateSut()
        => new(_client, NullLogger<PresignS3ObjectQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsUrlAndEffectiveExpiry()
    {
        // Arrange
        _client
            .GeneratePresignedUrlAsync("data", "notes/readme.txt", TimeSpan.FromSeconds(900), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("https://example.test/presigned"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PresignS3ObjectQuery("data", "notes/readme.txt", 900),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Be("https://example.test/presigned");
        result.Value.ExpirySeconds.Should().Be(900);
    }

    [Fact]
    public async Task Handle_WhenExpiryNotPositive_UsesDefaultExpiry()
    {
        // Arrange
        _client
            .GeneratePresignedUrlAsync("data", "a.txt", TimeSpan.FromSeconds(3600), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("https://example.test/default"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PresignS3ObjectQuery("data", "a.txt", 0),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpirySeconds.Should().Be(3600);
    }

    [Fact]
    public async Task Handle_WhenExpiryAboveMaximum_ClampsToMaximum()
    {
        // Arrange
        _client
            .GeneratePresignedUrlAsync("data", "a.txt", TimeSpan.FromSeconds(604800), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("https://example.test/max"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PresignS3ObjectQuery("data", "a.txt", 999999999),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpirySeconds.Should().Be(604800);
    }

    [Fact]
    public async Task Handle_WhenPresignFails_ReturnsError()
    {
        // Arrange
        _client
            .GeneratePresignedUrlAsync("data", "a.txt", Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("presign boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PresignS3ObjectQuery("data", "a.txt", 60),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("presign boom");
    }
}
