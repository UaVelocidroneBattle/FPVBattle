namespace Veloci.Logic.Bot;

public class BotPoll
{
    public required string Question { get; set; }
    public required List<PollOption> Options { get; set; }
}

public class PollOption
{
    public PollOption(int points, string text)
    {
        Points = points;
        Text = text;
    }

    public int Points { get; set; }

    public string Text { get; set; }
}

public class BotPollResults
{
    // Vote counts per answer, indexed to match BotPoll.Options order
    public required IReadOnlyList<int> VoteCounts { get; init; }
}
