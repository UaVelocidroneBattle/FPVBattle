namespace Veloci.Data.Domain;

public class League
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Pilot> Pilots { get; set; }
}

public static class LeagueExtensions
{
    extension(IQueryable<League> all)
    {
        public League? FindByOrder(int order)
        {
            return all.FirstOrDefault(x => x.Order == order);
        }
    }
}
