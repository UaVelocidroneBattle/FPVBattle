namespace Veloci.Logic.Services;

public interface IPatreonApiClient
{
    Task<PatreonCampaign[]> GetCampaignsAsync();
    Task<PatreonMembersResponse> GetCampaignMembersAsync(string campaignId);
}

// DTOs for Patreon API responses (moved from PatreonService)
public class PatreonCampaignsResponse
{
    public List<PatreonCampaign> Data { get; set; } = new();
}

public class PatreonCampaign
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
}

public class PatreonMembersResponse
{
    public List<PatreonMember> Data { get; set; } = new();
    public List<PatreonIncluded> Included { get; set; } = new();
    public PatreonLinks? Links { get; set; }
}

public class PatreonMember
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public PatreonMemberAttributes? Attributes { get; set; }
    public PatreonMemberRelationships? Relationships { get; set; }
}

public class PatreonMemberAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PatronStatus { get; set; }
    public int? CurrentlyEntitledAmountCents { get; set; }
    public DateTime? PledgeRelationshipStart { get; set; }
}

public class PatreonMemberRelationships
{
    public PatreonRelationshipData? User { get; set; }
    public PatreonRelationshipDataList? CurrentlyEntitledTiers { get; set; }
}

public class PatreonRelationshipData
{
    public PatreonRelationshipItem? Data { get; set; }
}

public class PatreonRelationshipDataList
{
    public List<PatreonRelationshipItem>? Data { get; set; }
}

public class PatreonRelationshipItem
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
}

public class PatreonIncluded
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public PatreonIncludedAttributes? Attributes { get; set; }
}

public class PatreonIncludedAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
}

public class PatreonTier : PatreonIncluded
{
}

public class PatreonLinks
{
    public string? Next { get; set; }
}
