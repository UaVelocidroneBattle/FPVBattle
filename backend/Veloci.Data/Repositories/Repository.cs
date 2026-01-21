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

    public virtual async Task AddAsync(T entry)
    {
        await DbSet.AddAsync(entry);
        await SaveChangesAsync();
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entries)
    {
        await DbSet.AddRangeAsync(entries);
        await SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entry)
    {
        DbSet.Update(entry);
        await DbContext.SaveChangesAsync();
    }

    public virtual async Task RemoveAsync(object id)
    {
        var dbEntry = await FindAsync(id);

        if (dbEntry is null)
            throw new NullReferenceException($"Record with id: {id} does not exist");

        CheckIfDetached(dbEntry);
        DbSet.Remove(dbEntry);
        await DbContext.SaveChangesAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entries)
    {
        DbSet.RemoveRange(entries);
        await SaveChangesAsync();
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
