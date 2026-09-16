using GymBrain.Application.Auth.Commands.Register;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Tests.Auth.Commands.Register;

public class RegisterUserCommandHandlerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_OnlyUniqueEmailCreatesUserAndToken(bool duplicate)
    {
        using var db = new AuthTestDb();
        var hasher = new TestPasswordHasher();
        const string email = "test@example.com";
        if (duplicate)
        {
            db.Users.Add(new User(email, "existing-hash", ExperienceLevel.Beginner));
            await db.SaveChangesAsync();
        }
        var jwt = new TestJwtService();
        var handler = new RegisterUserCommandHandler(db, hasher, jwt);
        var result = await handler.Handle(new RegisterUserCommand(email, "password", "Coach"), CancellationToken.None);
        Assert.Equal(!duplicate, result.IsSuccess);
        Assert.Equal(duplicate ? 0 : 1, jwt.Calls);
        Assert.Equal(1, await db.Users.CountAsync());
        if (duplicate)
        {
            Assert.Equal("Validation", result.Error!.Code);
            Assert.Equal("existing-hash", (await db.Users.SingleAsync()).PasswordHash);
        }
        else
        {
            var saved = await db.Users.SingleAsync();
            Assert.Equal(hasher.Hash("password"), saved.PasswordHash);
            Assert.Equal("Coach", saved.TonePersona);
            Assert.Equal(saved.Id, result.Value.UserId);
            Assert.Equal("test-token", result.Value.Token);
        }
    }
}
