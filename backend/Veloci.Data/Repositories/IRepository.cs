using System.Linq.Expressions;

namespace Veloci.Data.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();

    IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);

    ValueTask<T?> FindAsync(object id);

    Task AddAsync(T entry);

    Task AddRangeAsync(IEnumerable<T> entries);

    Task UpdateAsync(T entry);

    Task RemoveAsync(object id);

    Task RemoveRangeAsync(IEnumerable<T> entries);

    Task SaveChangesAsync();

    Task SaveChangesAsync(CancellationToken ct);
}
