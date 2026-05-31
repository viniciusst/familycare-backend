using FamilyCare.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyCare.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record TestRequest(string Value) : IRequest<string>;

    private readonly Mock<ILogger<LoggingBehavior<TestRequest, string>>> _loggerMock = new();

    public LoggingBehaviorTests()
    {
        // Source-generated LoggerMessage delegates short-circuit if IsEnabled returns false.
        // We want logging to actually flow through so we can verify the invocations.
        _loggerMock
            .Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
    }

    [Fact]
    public async Task Handle_WhenHandlerSucceeds_ShouldLogStartAndEnd()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>(_loggerMock.Object);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        // Act
        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);

        // Two Information-level logs: "Handling X" and "Handled X in Yms".
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));

        // No Error-level log on the happy path.
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>(_loggerMock.Object);
        var boom = new InvalidOperationException("boom");
        RequestHandlerDelegate<string> next = () => throw boom;

        // Act + Assert
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(new TestRequest("x"), next, CancellationToken.None));

        Assert.Same(boom, thrown);

        // One Info log ("Handling X") before the exception, then one Error log.
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                boom,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}