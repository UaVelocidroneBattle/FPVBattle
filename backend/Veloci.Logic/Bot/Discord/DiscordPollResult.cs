using System;
using System.Collections.Generic;
using System.Linq;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Represents the result of a Discord poll with vote counts for each option
/// </summary>
public class DiscordPollResult
{
    /// <summary>
    /// Total number of voters
    /// </summary>
    public int TotalVoterCount { get; set; }
    
    /// <summary>
    /// Individual vote counts for each poll option
    /// </summary>
    public Dictionary<string, int> OptionVoterCounts { get; set; } = new();
    
    /// <summary>
    /// Whether the poll is completed/stopped
    /// </summary>
    public bool IsCompleted { get; set; }
    
    public DiscordPollResult()
    {
    }
    
    public DiscordPollResult(int totalVoterCount, Dictionary<string, int> optionVoterCounts, bool isCompleted)
    {
        TotalVoterCount = totalVoterCount;
        OptionVoterCounts = optionVoterCounts ?? new Dictionary<string, int>();
        IsCompleted = isCompleted;
    }
}