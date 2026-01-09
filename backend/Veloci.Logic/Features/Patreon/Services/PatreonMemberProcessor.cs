using Veloci.Data.Domain;
using Veloci.Logic.Features.Patreon.Exceptions;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
///     Efficiently processes PatreonMembersResponse data into PatreonSupporter objects.
///     Uses optimized lookups to avoid O(n*m) complexity when matching related data.
/// </summary>
public class PatreonMemberProcessor
{
    private readonly Dictionary<string, PatreonIncluded> _tierLookup;
    private readonly Dictionary<string, PatreonIncluded> _userLookup;

    /// <summary>
    ///     Initializes the processor with optimized lookups for included data.
    /// </summary>
    /// <param name="response">The PatreonMembersResponse containing members and included data</param>
    public PatreonMemberProcessor(PatreonMembersResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        // Create efficient lookups for O(1) access to related data
        _userLookup = CreateLookup(response.Included, "user");
        _tierLookup = CreateLookup(response.Included, "tier");
    }

    /// <summary>
    ///     Processes all members in the response and converts them to PatreonSupporter objects.
    /// </summary>
    /// <param name="response">The PatreonMembersResponse to process</param>
    /// <returns>Array of PatreonSupporter objects</returns>
    public PatreonSupporter[] ProcessMembers(PatreonMembersResponse response)
    {
        if (response.Data == null || response.Data.Count == 0)
        {
            return [];
        }

        var supporters = new List<PatreonSupporter>();

        foreach (var member in response.Data)
        {
            try
            {
                var supporter = MapToSupporter(member);
                supporters.Add(supporter);
            }
            catch (Exception)
            {
                throw new PatreonMemberParsingException($"Failed to parse member data for member ID: {member.Id}")
                {
                    MemberId = member.Id
                };
            }
        }

        return supporters.ToArray();
    }

    /// <summary>
    ///     Maps a single PatreonMember to a PatreonSupporter using efficient lookups.
    /// </summary>
    /// <param name="member">The member to convert</param>
    /// <returns>Converted PatreonSupporter object</returns>
    public PatreonSupporter MapToSupporter(PatreonMember member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        var user = GetRelatedUser(member);
        var tier = GetRelatedTier(member);

        return new PatreonSupporter
        {
            PatreonId = member.Id,
            Name = ExtractName(member, user),
            Email = ExtractEmail(member, user),
            TierName = tier?.Attributes?.Title,
            Amount = ConvertAmountToDecimal(member.Attributes?.CurrentlyEntitledAmountCents),
            Status = member.Attributes?.PatronStatus ?? "unknown",
            FirstSupportedAt = member.Attributes?.PledgeRelationshipStart ?? DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Retrieves the user data related to a member using efficient lookup.
    /// </summary>
    /// <param name="member">The member whose user data to retrieve</param>
    /// <returns>User data if found, null otherwise</returns>
    private PatreonIncluded? GetRelatedUser(PatreonMember member)
    {
        var userId = member.Relationships?.User?.Data?.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        _userLookup.TryGetValue(userId, out var user);
        return user;
    }

    /// <summary>
    ///     Retrieves the tier data related to a member using efficient lookup.
    ///     Returns the first entitled tier if multiple tiers exist.
    /// </summary>
    /// <param name="member">The member whose tier data to retrieve</param>
    /// <returns>Tier data if found, null otherwise</returns>
    private PatreonIncluded? GetRelatedTier(PatreonMember member)
    {
        var tierIds = member.Relationships?.CurrentlyEntitledTiers?.Data?.Select(t => t.Id) ??
                      Enumerable.Empty<string>();

        foreach (var tierId in tierIds)
        {
            if (_tierLookup.TryGetValue(tierId, out var tier))
            {
                return tier;
            }
        }

        return null;
    }

    /// <summary>
    ///     Extracts the name from member or user data, preferring member data.
    /// </summary>
    /// <param name="member">The member data</param>
    /// <param name="user">The user data (fallback)</param>
    /// <returns>The extracted name or "Unknown" if none found</returns>
    private static string ExtractName(PatreonMember member, PatreonIncluded? user)
    {
        return member.Attributes?.FullName
               ?? user?.Attributes?.FullName
               ?? "Unknown";
    }

    /// <summary>
    ///     Extracts the email from member or user data, preferring member data.
    /// </summary>
    /// <param name="member">The member data</param>
    /// <param name="user">The user data (fallback)</param>
    /// <returns>The extracted email or null if none found</returns>
    private static string? ExtractEmail(PatreonMember member, PatreonIncluded? user)
    {
        return member.Attributes?.Email ?? user?.Attributes?.Email;
    }

    /// <summary>
    ///     Converts cents amount to decimal dollars.
    /// </summary>
    /// <param name="amountCents">Amount in cents</param>
    /// <returns>Amount in dollars as decimal, or null if input is null</returns>
    private static decimal? ConvertAmountToDecimal(int? amountCents)
    {
        return amountCents.HasValue ? amountCents.Value / 100m : null;
    }

    /// <summary>
    ///     Creates an efficient lookup dictionary for included data of a specific type.
    /// </summary>
    /// <param name="included">The included data collection</param>
    /// <param name="type">The type to filter by (e.g., "user", "tier")</param>
    /// <returns>Dictionary keyed by ID for O(1) lookup</returns>
    private static Dictionary<string, PatreonIncluded> CreateLookup(List<PatreonIncluded>? included, string type)
    {
        if (included == null || included.Count == 0)
        {
            return new Dictionary<string, PatreonIncluded>();
        }

        return included
            .Where(item => item.Type == type && !string.IsNullOrEmpty(item.Id))
            .ToDictionary(item => item.Id, item => item);
    }
}
