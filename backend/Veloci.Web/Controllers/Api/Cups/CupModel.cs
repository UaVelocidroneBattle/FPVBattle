namespace Veloci.Web.Controllers.Api.Cups;

/// <summary>
/// A cup as the dashboard knows it: what to call it and what id to pass back.
/// </summary>
public class CupModel
{
    public required string Id { get; set; }

    public required string Name { get; set; }
}
