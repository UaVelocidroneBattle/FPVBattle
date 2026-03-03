namespace Veloci.Logic.Bot.Discord;

public class DiscordChatMessages : ChatMessages
{
    public DiscordChatMessages()
    {
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "👀 Where is everyone?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🧐 Is anyone alive?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🫠 The track won't fly itself"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🙃 Maybe it's time?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🙄 What are we waiting for?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🤓 Fire up those simulators already"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "😴 Zzzz..."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "👀 Double-click the Velocidrone icon on your desktop, please"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "📺 Maybe grab another coffee? Catch a show? No rush, obviously."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🛋️ Need someone to bring your blanket too? Comfort first, flying can wait."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🧘‍♂️ No rush, as if meditation is more important than FPV."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🪞 Look in the mirror. That's a pilot who's too lazy to fly."));

        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "👀 Where is everyone else?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "😐 Only one result? Shame!"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🙄 What are the rest waiting for?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🥱 The rest decided watching is more fun than flying?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🤷‍♂️ Just one? Could be worse. Less embarrassment in the leaderboard."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🥇 Well then, automatic gold. Thanks to everyone else for their participation."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🫣 The rest decided to protect their self-esteem and simply didn't start."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🪦 Legends say there used to be more than one pilot here..."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🫡 Someone had to step up. Thank you, lone pilot."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "😌 At least one person wasn't lazy. The rest seem to be chilling."));

        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "👌 Don't forget to rate the track"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Rating tracks matters 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "So, how was the track? 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Your opinion matters 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Rate the track if you haven't yet 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Go vote 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Got 10 seconds for a quick survey? 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧐 Rating the track is no harder than liking something on TikTok. Give it a try."));
    }
}
