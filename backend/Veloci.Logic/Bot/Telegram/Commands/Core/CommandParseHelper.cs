using System.Text;

namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public class CommandParseHelper
{
    /// <summary>
    /// If parameters come in multiple parts due to spaces, this method reconstructs them into single values.
    /// E.g. "(John", "Doe)", => "John Doe"
    /// </summary>
    public static List<string> ReconstructBracketedValues(string[] parameters)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var insideBrackets = false;

        foreach (var param in parameters)
        {
            if (param.StartsWith('('))
            {
                insideBrackets = true;
                current.Clear();
                current.Append(param);
            }
            else if (insideBrackets)
            {
                current.Append(' ').Append(param);
            }

            if (param.EndsWith(')') && insideBrackets)
            {
                var normalized = current.ToString()
                    .Trim()
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty);

                result.Add(normalized);
                insideBrackets = false;
            }
            else if (!insideBrackets && !param.StartsWith('('))
            {
                result.Add(param); // for non-bracketed values like platform name
            }
        }

        return result;
    }
}
