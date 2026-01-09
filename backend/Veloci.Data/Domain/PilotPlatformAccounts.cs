namespace Veloci.Data.Domain;

public class PilotPlatformAccount
{
    public Guid Id { get; set; } = Guid.Empty;
    public virtual Pilot Pilot { get; set; }
    public int PilotId { get; set; }
    public PlatformNames PlatformName { get; set; }
    public required string Username { get; set; }
}

public enum PlatformNames
{
    Telegram,
    Discord
}
