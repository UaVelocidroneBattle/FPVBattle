namespace Veloci.Logic.Services;

/// <summary>
/// Client interface for interacting with Patreon API v2.
/// Provides methods to retrieve campaign and member data using OAuth authentication.
/// </summary>
public interface IPatreonApiClient
{
    /// <summary>
    /// Retrieves all campaigns associated with the authenticated user.
    /// A campaign represents a creator's Patreon page with details like name, summary, and patron count.
    /// </summary>
    Task<PatreonCampaign[]> GetCampaignsAsync();
    
    /// <summary>
    /// Retrieves all members (patrons) for a specific campaign.
    /// Members represent users who support a campaign with pledges and subscriptions.
    /// </summary>
    /// <param name="campaignId">The unique identifier of the campaign</param>
    Task<PatreonMembersResponse> GetCampaignMembersAsync(string campaignId);
}

#region Patreon API Response DTOs

/// <summary>
/// Root response object for campaigns endpoint following JSON:API specification.
/// Contains a collection of campaign resources in the 'data' array.
/// </summary>
public class PatreonCampaignsResponse
{
    public List<PatreonCampaign> Data { get; set; } = new();
}

/// <summary>
/// Represents a Patreon campaign resource.
/// A campaign is a creator's Patreon page containing their content, goals, and patron information.
/// Each campaign has a unique ID and follows JSON:API resource format.
/// </summary>
public class PatreonCampaign
{
    /// <summary>The unique identifier for this campaign</summary>
    public string Id { get; set; } = null!;
    
    /// <summary>Resource type identifier, always "campaign" for campaign resources</summary>
    public string Type { get; set; } = null!;
}

/// <summary>
/// Root response object for campaign members endpoint following JSON:API specification.
/// Contains member data, related resources, and pagination links.
/// </summary>
public class PatreonMembersResponse
{
    /// <summary>Array of member resources representing patrons supporting the campaign</summary>
    public List<PatreonMember> Data { get; set; } = new();
    
    /// <summary>
    /// Related resources referenced by member relationships (users, tiers, etc.).
    /// Used to avoid duplication in JSON:API responses by including referenced data once.
    /// </summary>
    public List<PatreonIncluded> Included { get; set; } = new();
    
    /// <summary>Pagination links for retrieving additional pages of members</summary>
    public PatreonLinks? Links { get; set; }
}

/// <summary>
/// Represents a member resource (patron) who supports a campaign.
/// In API v2, member replaces the deprecated "pledge" resource and contains
/// information about a user's membership to a specific campaign.
/// </summary>
public class PatreonMember
{
    /// <summary>The unique identifier for this member relationship</summary>
    public string Id { get; set; } = null!;
    
    /// <summary>Resource type identifier, always "member" for member resources</summary>
    public string Type { get; set; } = null!;
    
    /// <summary>Member-specific attributes like pledge amount, status, and dates</summary>
    public PatreonMemberAttributes? Attributes { get; set; }
    
    /// <summary>References to related resources like the patron user and entitled tiers</summary>
    public PatreonMemberRelationships? Relationships { get; set; }
}

/// <summary>
/// Contains member-specific data attributes for a patron's campaign membership.
/// These attributes describe the financial and temporal aspects of the patronage.
/// </summary>
public class PatreonMemberAttributes
{
    /// <summary>Full name of the patron (may be null if user chooses to remain anonymous)</summary>
    public string? FullName { get; set; }
    
    /// <summary>Email address of the patron (requires appropriate OAuth scope)</summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Current status of the patron's membership.
    /// Common values: "active_patron", "declined_patron", "former_patron"
    /// </summary>
    public string? PatronStatus { get; set; }
    
    /// <summary>
    /// Amount in cents the patron is currently entitled to based on their active pledge.
    /// Used to determine tier access and benefits.
    /// </summary>
    public int? CurrentlyEntitledAmountCents { get; set; }
    
    /// <summary>Date when the patron's relationship with this campaign began</summary>
    public DateTime? PledgeRelationshipStart { get; set; }
}

/// <summary>
/// Contains references to related resources for a member following JSON:API relationships pattern.
/// These relationships link to other resources that provide additional context about the member.
/// </summary>
public class PatreonMemberRelationships
{
    /// <summary>Reference to the user resource representing the patron</summary>
    public PatreonRelationshipData? User { get; set; }
    
    /// <summary>References to tier resources the patron is currently entitled to access</summary>
    public PatreonRelationshipDataList? CurrentlyEntitledTiers { get; set; }
}

/// <summary>
/// JSON:API relationship wrapper for a single related resource reference.
/// Contains the resource identifier that can be resolved from the 'included' array.
/// </summary>
public class PatreonRelationshipData
{
    /// <summary>Resource identifier pointing to related data in the 'included' section</summary>
    public PatreonRelationshipItem? Data { get; set; }
}

/// <summary>
/// JSON:API relationship wrapper for multiple related resource references.
/// Contains an array of resource identifiers that can be resolved from the 'included' array.
/// </summary>
public class PatreonRelationshipDataList
{
    /// <summary>Array of resource identifiers pointing to related data in the 'included' section</summary>
    public List<PatreonRelationshipItem>? Data { get; set; }
}

/// <summary>
/// JSON:API resource identifier containing only the type and ID of a related resource.
/// Used in relationships to reference other resources without duplicating their full data.
/// </summary>
public class PatreonRelationshipItem
{
    /// <summary>The unique identifier of the referenced resource</summary>
    public string Id { get; set; } = null!;
    
    /// <summary>The type of the referenced resource (e.g., "user", "tier")</summary>
    public string Type { get; set; } = null!;
}

/// <summary>
/// Represents additional related resources included in the response to avoid duplication.
/// These are full resource objects referenced by member relationships (users, tiers, etc.).
/// Part of JSON:API's compound document structure.
/// </summary>
public class PatreonIncluded
{
    /// <summary>The unique identifier for this included resource</summary>
    public string Id { get; set; } = null!;
    
    /// <summary>The type of this included resource (e.g., "user", "tier")</summary>
    public string Type { get; set; } = null!;
    
    /// <summary>Attributes specific to this included resource type</summary>
    public PatreonIncludedAttributes? Attributes { get; set; }
}

/// <summary>
/// Contains attributes for included resources like users and tiers.
/// The available attributes depend on the resource type and requested fields.
/// </summary>
public class PatreonIncludedAttributes
{
    /// <summary>Full name (typically for user resources)</summary>
    public string? FullName { get; set; }
    
    /// <summary>Email address (typically for user resources, requires OAuth scope)</summary>
    public string? Email { get; set; }
    
    /// <summary>Title or name (typically for tier resources)</summary>
    public string? Title { get; set; }
}

/// <summary>
/// Specialized included resource representing a Patreon tier/reward level.
/// Inherits from PatreonIncluded but can be extended with tier-specific properties.
/// </summary>
public class PatreonTier : PatreonIncluded
{
}

/// <summary>
/// Contains pagination links for navigating through paginated API responses.
/// Allows retrieval of additional pages when the response contains more data than the page limit.
/// </summary>
public class PatreonLinks
{
    /// <summary>URL for the next page of results, null if this is the last page</summary>
    public string? Next { get; set; }
}

#endregion
