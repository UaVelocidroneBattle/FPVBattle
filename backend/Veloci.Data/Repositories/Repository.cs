using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Veloci.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = DbContext.Set<T>();
    }

    public virtual IQueryable<T> GetAll()
    {
        return DbSet;
    }

    public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
    {
        return GetAll().Where(predicate);
    }

    public virtual ValueTask<T?> FindAsync(object id)
    {
        return DbSet.FindAsync(id);
    }

    /// <summary>
    /// Stages the entry for insertion. Does not persist - call <see cref="SaveChangesAsync()"/>
    /// explicitly once all changes for the unit of work are staged.
    /// </summary>
    public virtual async Task AddAsync(T entry)
    {
        await DbSet.AddAsync(entry);
    }

    /// <summary>
    /// Stages the entries for insertion. Does not persist - call <see cref="SaveChangesAsync()"/>
    /// explicitly once all changes for the unit of work are staged.
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entries)
    {
        await DbSet.AddRangeAsync(entries);
    }

    /// <summary>
    /// Stages the entry for update. Does not persist - call <see cref="SaveChangesAsync()"/>
    /// explicitly once all changes for the unit of work are staged.
    /// </summary>
    public virtual Task UpdateAsync(T entry)
    {
        DbSet.Update(entry);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages the entry for removal. Does not persist - call <see cref="SaveChangesAsync()"/>
    /// explicitly once all changes for the unit of work are staged.
    /// </summary>
    public virtual async Task RemoveAsync(object id)
    {
        var dbEntry = await FindAsync(id);

        if (dbEntry is null)
            throw new NullReferenceException($"Record with id: {id} does not exist");

        CheckIfDetached(dbEntry);
        DbSet.Remove(dbEntry);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync()
    {
        await DbContext.SaveChangesAsync();
    }

    private void CheckIfDetached(T entity)
    {
        if (DbContext.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }
    }
}
