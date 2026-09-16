using GymBrain.Application.Auth.Commands.Login;
using GymBrain.Domain.Common;
using GymBrain.Domain.Entities;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GymBrain.Application.Tests.Auth.Commands.Login;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtTokenService>();

        _handler = new LoginUserCommandHandler(
            _dbContextMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var userId = Guid.NewGuid();
        var user = new User(email, "hashedPassword", Domain.Enums.ExperienceLevel.Beginner)
        {
            Id = userId
        };

        var command = new LoginUserCommand(email, password);

        _dbContextMock.Setup(db => db.Users.FirstOrDefaultAsync(
                u => u.Email == email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.Verify(password, user.PasswordHash))
            .Returns(true);

        _jwtServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("fake-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("fake-token", result.Value.Token);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailureResult()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password123";
        var command = new LoginUserCommand(email, password);

        _dbContextMock.Setup(db => db.Users.FirstOrDefaultAsync(
                u => u.Email == email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<Error>(result.Error);
        Assert.Equal("Invalid credentials.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        var userId = Guid.NewGuid();
        var user = new User(email, "hashedPassword", Domain.Enums.ExperienceLevel.Beginner)
        {
            Id = userId
        };

        var command = new LoginUserCommand(email, password);

        _dbContextMock.Setup(db => db.Users.FirstOrDefaultAsync(
                u => u.Email == email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.Verify(password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<Error>(result.Error);
        Assert.Equal("Invalid credentials.", result.Error.Message);
    }
}