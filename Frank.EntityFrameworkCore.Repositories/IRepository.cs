using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Frank.EntityFrameworkCore.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> AsQueryable();

    Task<long> CountAsync();
    Task<long> CountAsync(Expression<Func<T,bool>> predicate);
    
    Task<bool> AnyAsync(Expression<Func<T,bool>> predicate);
    Task<bool> AllAsync(Expression<Func<T,bool>> predicate);
    
    IAsyncEnumerable<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, int skip, int take, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, int skip, int take, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, bool descending, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, bool descending, int skip, int take, CancellationToken cancellationToken = default);
    
    Task<T> FindAsync(Expression<Func<T,bool>> predicate);
    
    Task AddAsync(T entity);
    Task AddAsync(IEnumerable<T> entities);
    
    Task RemoveAsync(T entity);
    Task RemoveAsync(Expression<Func<T,bool>> predicate);
    
    Task UpdateAsync(T entity);
    Task UpdateAsync(Expression<Func<T,bool>> predicate, T entity);
    Task UpdateAsync(Expression<Func<T,bool>> predicate, Action<T> action);
    
    Task UpsertEntityAsync(T entity, Expression<Func<T, bool>> matchOn, Action<T> updateAction, CancellationToken cancellationToken);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task DiscardChangesAsync(CancellationToken cancellationToken = default);
}