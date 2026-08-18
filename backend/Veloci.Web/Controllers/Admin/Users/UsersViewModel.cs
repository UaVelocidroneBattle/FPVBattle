namespace Veloci.Web.Controllers.Admin.Users;

public class UsersViewModel
{
    public List<UserRow> Users { get; set; } = [];
}

public class UserRow
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime RegisteredOn { get; set; }
    public string? PilotName { get; set; }
    public string? CountryName { get; set; }
    public int? DayStreak { get; set; }
    public int? Freezies { get; set; }
}
