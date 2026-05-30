using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Commands.RegisterUser;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;
using AppValidationException = FamilyCare.Application.Common.Exceptions.ValidationException;

namespace FamilyCare.Application.Tests.Identity.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();

    private RegisterUserCommandHandler CreateSut()
        => new(_userRepoMock.Object, _hasherMock.Object);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUserAndReturnResponse()
    {
        // Arrange
        var hash = TestData.AnyPasswordHash();
        _userRepoMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(hash);

        var command = new RegisterUserCommand(
            "vinicius@familycare.test",
            "StrongPass1!",
            SupportedLanguage.PortugueseBrazil);

        var sut = CreateSut();

        // Act
        var response = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("vinicius@familycare.test", response.Email);
        Assert.NotEqual(UserId.Empty, response.UserId);

        _hasherMock.Verify(h => h.Hash("StrongPass1!"), Times.Once);
        _userRepoMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Email.Value == "vinicius@familycare.test"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RegisterUserCommand(
            "existing@familycare.test",
            "StrongPass1!",
            SupportedLanguage.EnglishCanada);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => sut.Handle(command, CancellationToken.None));
        Assert.Equal("identity.email_already_registered", ex.Code);

        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _userRepoMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidEmailFormat_ShouldThrowFromValueObject()
    {
        // Arrange — even before reaching the repo, Email.Create() fails.
        var command = new RegisterUserCommand(
            "not-an-email",
            "StrongPass1!",
            SupportedLanguage.EnglishCanada);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidEntityStateException>(
            () => sut.Handle(command, CancellationToken.None));

        _userRepoMock.Verify(
            r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Theory]
    [InlineData("vinicius@familycare.test", "StrongPass1!", true)]
    [InlineData("ok@a.co", "Aa1aaaaa", true)]
    public void Validate_WithValidInput_ShouldPass(string email, string password, bool isValid)
    {
        var result = _validator.Validate(new RegisterUserCommand(
            email, password, SupportedLanguage.EnglishCanada));

        Assert.Equal(isValid, result.IsValid);
    }

    [Theory]
    [InlineData("")]                       // empty (NotEmpty fails)
    [InlineData("not-an-email")]           // no @ at all
    [InlineData("two@@signs.com")]         // multiple @ signs
    public void Validate_WithBadEmail_ShouldFail(string email)
    {
        var result = _validator.Validate(new RegisterUserCommand(
            email, "StrongPass1!", SupportedLanguage.EnglishCanada));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("short1A")]             // too short
    [InlineData("nouppercase1!")]       // no uppercase
    [InlineData("NOLOWERCASE1!")]       // no lowercase
    [InlineData("NoDigitsHere!")]       // no digit
    public void Validate_WithWeakPassword_ShouldFail(string password)
    {
        var result = _validator.Validate(new RegisterUserCommand(
            "user@test.com", password, SupportedLanguage.EnglishCanada));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }
}