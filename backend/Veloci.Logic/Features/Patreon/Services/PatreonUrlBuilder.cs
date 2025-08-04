using System.Web;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
///     Builds Patreon API URLs with proper encoding, field selection, and relationship inclusion.
///     Provides a fluent interface for constructing complex API queries with type safety.
/// </summary>
public class PatreonUrlBuilder
{
    private readonly Dictionary<string, List<string>> _fields = new();
    private readonly List<string> _includes = [];
    private string _baseEndpoint = "";

    /// <summary>
    ///     Sets the base endpoint for campaign members with proper URL encoding of campaign ID.
    /// </summary>
    /// <param name="campaignId">The campaign ID to fetch members for</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder ForCampaignMembers(string campaignId)
    {
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            throw new ArgumentException("Campaign ID cannot be null or empty", nameof(campaignId));
        }

        _baseEndpoint = $"campaigns/{HttpUtility.UrlEncode(campaignId)}/members";
        return this;
    }

    /// <summary>
    ///     Adds relationship includes to the API request to fetch related data.
    /// </summary>
    /// <param name="relationships">Relationship names to include (e.g., "user", "currently_entitled_tiers")</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder IncludeRelationships(params string[] relationships)
    {
        if (relationships == null || relationships.Length == 0)
        {
            return this;
        }

        foreach (var relationship in relationships.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            if (!_includes.Contains(relationship))
            {
                _includes.Add(relationship);
            }
        }

        return this;
    }

    /// <summary>
    ///     Specifies which fields to include for member resources.
    /// </summary>
    /// <param name="fields">Field names for member resources</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder WithMemberFields(params string[] fields)
    {
        return WithFields("member", fields);
    }

    /// <summary>
    ///     Specifies which fields to include for user resources.
    /// </summary>
    /// <param name="fields">Field names for user resources</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder WithUserFields(params string[] fields)
    {
        return WithFields("user", fields);
    }

    /// <summary>
    ///     Specifies which fields to include for tier resources.
    /// </summary>
    /// <param name="fields">Field names for tier resources</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder WithTierFields(params string[] fields)
    {
        return WithFields("tier", fields);
    }

    /// <summary>
    ///     Adds fields for a specific resource type.
    /// </summary>
    /// <param name="resourceType">The resource type (e.g., "member", "user", "tier")</param>
    /// <param name="fields">Field names to include</param>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder WithFields(string resourceType, params string[] fields)
    {
        if (string.IsNullOrWhiteSpace(resourceType) || fields == null || fields.Length == 0)
        {
            return this;
        }

        if (!_fields.ContainsKey(resourceType))
        {
            _fields[resourceType] = new List<string>();
        }

        foreach (var field in fields.Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            if (!_fields[resourceType].Contains(field))
            {
                _fields[resourceType].Add(field);
            }
        }

        return this;
    }

    /// <summary>
    ///     Builds the complete URL with all specified parameters, includes, and fields.
    /// </summary>
    /// <returns>The complete API endpoint URL</returns>
    /// <exception cref="InvalidOperationException">Thrown when no base endpoint has been set</exception>
    public string Build()
    {
        if (string.IsNullOrEmpty(_baseEndpoint))
        {
            throw new InvalidOperationException("Base endpoint must be set before building URL");
        }

        var queryParams = new List<string>();

        // Add includes parameter
        if (_includes.Count > 0)
        {
            var includesValue = string.Join(",", _includes);
            queryParams.Add($"include={HttpUtility.UrlEncode(includesValue)}");
        }

        // Add field parameters
        foreach (var kvp in _fields.Where(f => f.Value.Count > 0))
        {
            var fieldsValue = string.Join(",", kvp.Value);
            queryParams.Add($"fields[{HttpUtility.UrlEncode(kvp.Key)}]={HttpUtility.UrlEncode(fieldsValue)}");
        }

        // Combine base endpoint with query parameters
        if (queryParams.Count == 0)
        {
            return _baseEndpoint;
        }

        return $"{_baseEndpoint}?{string.Join("&", queryParams)}";
    }

    /// <summary>
    ///     Creates a pre-configured builder for standard campaign member requests.
    ///     Includes commonly needed relationships and fields for member processing.
    /// </summary>
    /// <param name="campaignId">The campaign ID to fetch members for</param>
    /// <returns>Configured builder instance</returns>
    public static PatreonUrlBuilder CreateStandardMemberRequest(string campaignId)
    {
        return new PatreonUrlBuilder()
            .ForCampaignMembers(campaignId)
            .IncludeRelationships("currently_entitled_tiers", "user")
            .WithMemberFields("full_name", "email", "patron_status", "currently_entitled_amount_cents",
                "pledge_relationship_start")
            .WithUserFields("full_name", "email")
            .WithTierFields("title");
    }

    /// <summary>
    ///     Resets the builder to its initial state for reuse.
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public PatreonUrlBuilder Reset()
    {
        _fields.Clear();
        _includes.Clear();
        _baseEndpoint = "";
        return this;
    }
}
