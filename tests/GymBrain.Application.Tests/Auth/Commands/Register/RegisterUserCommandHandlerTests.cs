using GymBrain.Application.Auth.Commands.Register;
using GymBrain.Domain.Common;
using GymBrain.Domain.Entities;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GymBrain.Application.Tests.Auth.Commands.Register;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtTokenService>();

        _handler = new RegisterUserCommandHandler(
            _dbContextMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UniqueEmail_ReturnsSuccessResult()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "password123";
        var command = new RegisterUserCommand(email, password, null);

        _dbContextMock.Setup(db => db.Users.AnyAsync(
                u => u.Email == email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dbContextMock.Setup(db => db.Add(It.IsAny<User>()));
        _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHasherMock.Setup(p => p.Hash(password))
            .Returns("hashedPassword");

        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("fake-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.UserId); // UserId should be set after SaveChanges
        Assert.Equal("fake-token", result.Value.Token);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailureResult()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "password123";
        var command = new RegisterUserCommand(email, password, null);

        _dbContextMock.Setup(db => db.Users.AnyAsync(
                u => u.Email == email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<Error>(result.Error);
        Assert.Equal("A user with this email already exists.", result.Error.Message);
    }
}