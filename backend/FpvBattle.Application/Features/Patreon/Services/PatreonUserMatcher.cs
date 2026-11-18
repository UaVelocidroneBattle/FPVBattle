using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Patreon.Services;

public class PatreonUserMatcher
{
    private readonly IApplicationDbContext _db;

    public PatreonUserMatcher(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyDictionary<string, ApplicationUser>> FindUsersByEmailAsync(
        IEnumerable<PatreonSupporter> supporters,
        CancellationToken ct = default)
    {
        var normalizedEmails = supporters
            .Select(s => s.NormalizedEmail)
            .OfType<string>()
            .Distinct()
            .ToList();

        if (normalizedEmails.Count == 0)
        {
            return new Dictionary<string, ApplicationUser>();
        }

        var users = await _db.Users
            .Where(u => u.NormalizedEmail != null && normalizedEmails.Contains(u.NormalizedEmail))
            .Include(u => u.Pilot)
            .ToListAsync(ct);

        return users.ToDictionary(u => u.NormalizedEmail!);
    }
}
