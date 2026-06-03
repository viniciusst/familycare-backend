using FamilyCare.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyCare.Application.Tests.Behaviors;

public class UnitOfWorkBehaviorTests
{
    // A "Query" — does NOT implement ICommand. Should pass through without commit.
    public sealed record TestQuery(string Value) : IRequest<string>;

    // A "Command" — implements ICommand<TResponse>. Should trigger commit.
    public sealed record TestCommand(string Value) : ICommand<string>;

    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock = new();
    private readonly Mock<ILogger<UnitOfWorkBehavior<TestCommand, string>>> _logCommandMock = new();
    private readonly Mock<ILogger<UnitOfWorkBehavior<TestQuery, string>>> _logQueryMock = new();

    [Fact]
    public async Task Handle_ForCommand_ShouldCallSaveAndDispatchEvents()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(
            _uowMock.Object, _dispatcherMock.Object, _logCommandMock.Object);

        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        // Act
        var result = await behavior.Handle(new TestCommand("x"), next, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(
            d => d.DispatchAndClearEventsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ForQuery_ShouldPassThroughWithoutCommit()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestQuery, string>(
            _uowMock.Object, _dispatcherMock.Object, _logQueryMock.Object);

        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        // Act
        var result = await behavior.Handle(new TestQuery("x"), next, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _dispatcherMock.Verify(
            d => d.DispatchAndClearEventsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ForCommand_WhenHandlerThrows_ShouldNotCommit()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(
            _uowMock.Object, _dispatcherMock.Object, _logCommandMock.Object);

        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("boom");

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(new TestCommand("x"), next, CancellationToken.None));

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _dispatcherMock.Verify(
            d => d.DispatchAndClearEventsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
