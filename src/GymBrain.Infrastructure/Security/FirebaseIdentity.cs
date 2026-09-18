using System.Security.Claims;
using GymBrain.Domain.Entities;
using GymBrain.Domain.Enums;
using GymBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Infrastructure.Security;

public static class FirebaseIdentity
{
    public static async Task<User?> ResolveAsync(GymBrainDbContext db, ClaimsPrincipal principal, bool allowLink, CancellationToken ct,
        string? legacyPassword = null, GymBrain.Application.Common.Interfaces.IPasswordHasher? hasher = null)
    {
        var uid = principal.FindFirstValue("sub");
        var email = principal.FindFirstValue("email")?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(uid) || uid.Length > 128 || string.IsNullOrWhiteSpace(email) || email.Length > 256 ||
            !string.Equals(principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase)) return null;

        var linked = await db.Users.SingleOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        if (linked != null) return linked;
        if (!allowLink) return null;

        // Never guess between legacy accounts differing only by email case.
        var candidates = await db.Users.Where(u => u.Email.ToLower() == email).Take(2).ToListAsync(ct);
        if (candidates.Count > 1 || candidates.Any(u => u.FirebaseUid != null)) return null;
        // Verified email alone must never grant access to an existing workout account.
        if (candidates.Count == 1 && (string.IsNullOrEmpty(legacyPassword) || hasher == null ||
            !hasher.Verify(legacyPassword, candidates[0].PasswordHash))) return null;
        var user = candidates.SingleOrDefault() ?? new User(email, "firebase-managed", ExperienceLevel.Beginner);
        if (candidates.Count == 0) db.Users.Add(user);
        user.LinkFirebase(uid);
        try { await db.SaveChangesAsync(ct); return user; }
        catch (DbUpdateException)
        {
            // A concurrent request may have linked/created this account first.
            // Only accept the same UID, never a conflicting email/identity.
            db.ChangeTracker.Clear();
            return await db.Users.SingleOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        }
    }
}
