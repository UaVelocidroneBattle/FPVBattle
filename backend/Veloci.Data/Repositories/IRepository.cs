using System.Linq.Expressions;

namespace Veloci.Data.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();

    IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);

    ValueTask<T?> FindAsync(object id);

    /// <summary>Stages the entry for insertion. Call <see cref="SaveChangesAsync()"/> to persist.</summary>
    Task AddAsync(T entry);

    /// <summary>Stages the entries for insertion. Call <see cref="SaveChangesAsync()"/> to persist.</summary>
    Task AddRangeAsync(IEnumerable<T> entries);

    /// <summary>Stages the entry for update. Call <see cref="SaveChangesAsync()"/> to persist.</summary>
    Task UpdateAsync(T entry);

    /// <summary>Stages the entry for removal. Call <see cref="SaveChangesAsync()"/> to persist.</summary>
    Task RemoveAsync(object id);

    Task SaveChangesAsync();

    Task SaveChangesAsync(CancellationToken ct);
}
