namespace Veloci.Logic.Bot;

public class ChatMessages
{
    protected readonly List<ChatMessage> Messages = [];
    private static readonly Random Random = new ();

    public ChatMessage GetRandomByType(ChatMessageType messageType)
    {
        var msgs = Messages.Where(m => m.Type == messageType).ToList();
        var r = Random.Next(msgs.Count);
        return msgs[r];
    }

    public ChatMessage? GetRandomByTypeWithProbability(ChatMessageType messageType)
    {
        if (!CalculateProbability())
            return null;

        var msgs = Messages.Where(m => m.Type == messageType).ToList();
        var r = Random.Next(msgs.Count);
        return msgs[r];
    }

    private static bool CalculateProbability()
    {
        const int probabilityPercentage = 25;
        var chance = Random.Next(1, 101);
        return chance <= probabilityPercentage;
    }
}
