namespace Veloci.Web.Controllers.UserPilots;

public class UserPilotsViewModel
{
    public List<UserPilotRow> Users { get; set; } = [];

    public IEnumerable<UserPilotRow> UnlinkedUsers => Users.Where(u => u.PilotName is null);
}

public class UserPilotRow
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? PilotName { get; set; }
}
