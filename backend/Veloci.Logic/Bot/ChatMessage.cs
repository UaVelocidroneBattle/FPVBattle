namespace Veloci.Logic.Bot;

public class ChatMessage
{
    public ChatMessage(ChatMessageType type, string text)
    {
        Type = type;
        Text = text;
    }

    public ChatMessageType Type { get; set; }
    public string Text { get; set; }
}

public enum ChatMessageType
{
    NobodyFlying,
    FirstResult,
    OnlyOneFlew,
    VoteReminder
}
