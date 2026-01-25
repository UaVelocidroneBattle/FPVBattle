using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class HelpCommand : ITelegramCommand
{
    private readonly IServiceProvider _serviceProvider;

    public HelpCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string[] Keywords => ["/help", "/?"];

    public string Description => "`/help` або `/?` - Список усіх доступних команд";

    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        // Help command is global - works in any chat regardless of cup binding
        var descriptions = GetCommandDescriptions();
        return string.Join(Environment.NewLine, descriptions.Select(CommandRow));
    }

    public bool RemoveMessageAfterDelay => true;
    public bool Public => true;

    private string CommandRow(string description)
    {
        return $"▪️ {description}";
    }

    private IEnumerable<string> GetCommandDescriptions()
    {
        return _serviceProvider
            .GetServices<ITelegramCommand>()
            .Where(c => c.Public)
            .Select(c => c.Description);
    }
}
