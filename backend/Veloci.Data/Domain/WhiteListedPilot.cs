namespace Veloci.Data.Domain;

public class WhiteListedPilot
{
    public WhiteListedPilot()
    {
    }

    public WhiteListedPilot(string name)
    {
        AddedOn = DateTime.UtcNow;
        PilotName = name;
    }

    public Guid Id { get; set; }
    public DateTime AddedOn { get; set; }
    public string PilotName { get; set; }
}
