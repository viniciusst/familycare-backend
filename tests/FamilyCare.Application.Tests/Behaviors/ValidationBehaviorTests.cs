using FamilyCare.Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using AppValidationException = FamilyCare.Application.Common.Exceptions.ValidationException;

namespace FamilyCare.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
            Enumerable.Empty<IValidator<TestRequest>>());

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        // Act
        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_WhenAllValidatorsPass_ShouldCallNext()
    {
        // Arrange
        var passingValidator = new Mock<IValidator<TestRequest>>();
        passingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { passingValidator.Object });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        // Act
        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldThrowValidationException()
    {
        // Arrange
        var failingValidator = new Mock<IValidator<TestRequest>>();
        failingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Value must not be empty"),
            }));

        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { failingValidator.Object });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("should-not-reach");
        };

        // Act + Assert
        await Assert.ThrowsAsync<AppValidationException>(
            () => behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateFailures()
    {
        // Arrange
        var failing1 = new Mock<IValidator<TestRequest>>();
        failing1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Reason A"),
            }));

        var failing2 = new Mock<IValidator<TestRequest>>();
        failing2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Reason B"),
            }));

        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { failing1.Object, failing2.Object });

        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        // Act
        var ex = await Assert.ThrowsAsync<AppValidationException>(
            () => behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        // Assert — both failures present
        Assert.Equal(2, ex.Errors.Sum(kvp => kvp.Value.Length));
    }
}
