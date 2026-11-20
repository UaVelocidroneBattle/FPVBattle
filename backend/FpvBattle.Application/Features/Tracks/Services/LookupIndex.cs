using FpvBattle.Application.Features.Tracks.Models;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Services;

/// <summary>
/// The stored rows of one lookup kind keyed by their external id, remembering which ids the current sync returned.
/// </summary>
internal sealed class LookupIndex<T> where T : class, ISoftDeletable
{
    private readonly Dictionary<int, T> _rows;
    private readonly HashSet<int> _returnedIds = [];

    public LookupIndex(IEnumerable<T> rows, Func<T, int> id)
    {
        _rows = rows.ToDictionary(id);
        AvailableBefore = _rows.Values.Count(row => !row.IsDeleted);
    }

    public int AvailableBefore { get; }

    public LookupSyncCounts Counts { get; } = new();

    public bool WasReturned(int id) => _returnedIds.Contains(id);

    public T? Find(int id) => _rows.GetValueOrDefault(id);

    public void Added(int id, T row)
    {
        _rows[id] = row;
        _returnedIds.Add(id);
        Counts.Added++;
    }

    public void Returned(int id, T row, bool changed)
    {
        _returnedIds.Add(id);

        if (row.IsDeleted)
        {
            row.IsDeleted = false;
            Counts.Restored++;
        }
        else if (changed)
        {
            Counts.Updated++;
        }
    }

    public IReadOnlyList<T> AvailableButNotReturned() => _rows
        .Where(pair => !pair.Value.IsDeleted && !_returnedIds.Contains(pair.Key))
        .Select(pair => pair.Value)
        .ToList();

    public void Deleted(T row)
    {
        row.IsDeleted = true;
        Counts.Deleted++;
    }
}
