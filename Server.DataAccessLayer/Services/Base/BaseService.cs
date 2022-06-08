using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer.Context;
using Server.Database.Models._Base;

namespace Server.DataAccessLayer.Services.Base;

public abstract class BaseService<T> where T : ModelBase
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public BaseService(IDbContextFactory<DatabaseContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public virtual async Task<T> Add(T entity)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var entityEntry = await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }

    public virtual async Task AddRange(IEnumerable<T> entities)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Set<T>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task<T> Update(T entity)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (entity is ModelBase entityBase)
        {
            entityBase.LastUsage = DateTime.Now;
        }

        dbContext.Entry(entity).State = EntityState.Modified;

        var entityEntry = dbContext.Set<T>().Update(entity);

        await dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public virtual async Task UpdateRange(IEnumerable<T> entities)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        foreach (var entity in entities)
        {
            if (entity is ModelBase @base)
            {
                @base.LastUsage = DateTime.Now;
            }

            dbContext.Entry(entity).State = EntityState.Modified;
        }


        dbContext.Set<T>().UpdateRange(entities);
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task<List<T>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().ToListAsync();
    }

    public virtual async Task<T?> GetByKey(object key)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().FindAsync(key);
    }

    public virtual async Task<T> Remove(T entity)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Entry(entity).State = EntityState.Deleted;
        var entityEntry = dbContext.Set<T>().Remove(entity);
        await dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }

    public virtual async Task RemoveRange(IEnumerable<T> entities)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var entityBases = entities as T[] ?? entities.ToArray();

        foreach (var entity in entityBases)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;
        }

        dbContext.Set<T>().RemoveRange(entityBases);
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task<T?> Find(Expression<Func<T, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().FirstOrDefaultAsync(expression);
    }

    public virtual async Task<bool> Has(Expression<Func<T, bool>> expression)
    {
        return await Find(expression) != null;
    }

    public virtual async Task<List<T>> Where(Expression<Func<T, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().Where(expression).ToListAsync();
    }

    public virtual async Task<bool> Exists(Expression<Func<T, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.Set<T>().AnyAsync(expression);
    }
}