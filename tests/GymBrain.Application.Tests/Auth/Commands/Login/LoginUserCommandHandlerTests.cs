using GymBrain.Application.Auth.Commands.Login;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;

namespace GymBrain.Application.Tests.Auth.Commands.Login;

public class LoginUserCommandHandlerTests
{
    [Theory]
    [InlineData(true, "password", true)]
    [InlineData(true, "wrong-password", false)]
    [InlineData(false, "password", false)]
    public async Task Handle_OnlyValidCredentialsIssueToken(bool userExists, string password, bool succeeds)
    {
        using var db = new AuthTestDb();
        var hasher = new TestPasswordHasher();
        var user = new User("test@example.com", hasher.Hash("password"), ExperienceLevel.Beginner);
        if (userExists) { db.Users.Add(user); await db.SaveChangesAsync(); }
        var jwt = new TestJwtService();
        var handler = new LoginUserCommandHandler(db, hasher, jwt);
        var result = await handler.Handle(new LoginUserCommand(user.Email, password), CancellationToken.None);
        Assert.Equal(succeeds, result.IsSuccess);
        Assert.Equal(succeeds ? 1 : 0, jwt.Calls);
        if (succeeds)
        {
            Assert.Equal(user.Id, result.Value.UserId);
            Assert.Equal("test-token", result.Value.Token);
        }
        else
        {
            Assert.Equal("Unauthorized", result.Error!.Code);
            Assert.Equal("Invalid credentials.", result.Error.Message);
        }
    }
}
