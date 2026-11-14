namespace Veloci.Web.Controllers.Api.Teams;

public static class TeamModelMapper
{
    /// <summary>A LIKE pattern matching names that contain the text; "\" is the escape character.</summary>
    public static string ContainsPattern(string text)
    {
        var escaped = text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        return $"%{escaped}%";
    }
}
