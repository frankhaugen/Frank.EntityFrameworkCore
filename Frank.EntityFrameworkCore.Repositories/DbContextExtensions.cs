using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Frank.EntityFrameworkCore.Repositories;

internal static class DbContextExtensions
{
    public static async Task AddAtomicAsync<T>(this DbContext context, T entity) where T : class
    {
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task AddAtomicAsync<T>(this DbContext context, IEnumerable<T> entities) where T : class
    {
        await context.Set<T>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task UpdateAtomicAsync<T>(this DbContext context, T entity) where T : class
    {
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task UpdateAtomicAsync<T>(this DbContext context, IEnumerable<T> entities) where T : class
    {
        context.Set<T>().UpdateRange(entities);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task RemoveAtomicAsync<T>(this DbContext context, T entity) where T : class
    {
        context.Set<T>().Entry(entity).State = EntityState.Deleted;
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task RemoveAtomicAsync<T>(this DbContext context, IEnumerable<T> entities) where T : class
    {
        context.Set<T>().RemoveRange(entities);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task UpdateAtomicAsync<T>(this DbContext context, Expression<Func<T,bool>> predicate, Action<T> action) where T : class
    {
        var entities = context.Set<T>().Where(predicate);
        foreach (var entity in entities)
        {
            action(entity);
        }
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task UpdateAtomicAsync<T>(this DbContext context, Expression<Func<T,bool>> keyPredicate, T entity) where T : class
    {
        var existingEntity = context.Set<T>().Single(keyPredicate);
        context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static async Task RemoveAtomicAsync<T>(this DbContext context, Expression<Func<T,bool>> predicate) where T : class
    {
        var entities = context.Set<T>().Where(predicate);
        context.Set<T>().RemoveRange(entities);
        await context.SaveChangesAsync();
        context.ClearChanges();
    }
    
    public static void ClearChanges(this DbContext context)
    {
        context.ChangeTracker.Clear();
    }
}