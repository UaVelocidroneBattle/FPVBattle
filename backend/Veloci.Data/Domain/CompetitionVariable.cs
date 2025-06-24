namespace Veloci.Data.Domain;

public class CompetitionVariables
{
    public const string DiscordTimeUpdatesThreadName = "Time updates";
    public const string DiscordLeaderboardMessageId = "DiscordLeaderboardMessageId";
}

public class CompetitionVariable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CompetitionId { get; set; }
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public ulong? ULongValue { get; set; }
    public double? DoubleValue { get; set; }
    public bool? BoolValue { get; set; }

    public void UpdateValue(object value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "The value cannot be null");

        switch (value)
        {
            case string stringValue:
                StringValue = stringValue;
                break;
            case bool boolValue:
                BoolValue = boolValue;
                break;
            case ulong ulongValue:
                ULongValue = ulongValue;
                break;
            case int intValue:
                IntValue = intValue;
                break;
            case double doubleValue:
                DoubleValue = doubleValue;
                break;
            default:
                throw new ArgumentException($"Unsupported value type: {value.GetType().Name}", nameof(value));
        }
    }
}
