namespace Veloci.Data.Domain;

public class PilotNameHistoryRow
{
    public Guid Id { get; set; } = Guid.Empty;
    public string OldName { get; set; }
    public string NewName { get; set; }
    public DateTime ChangedOn { get; set; }
}
