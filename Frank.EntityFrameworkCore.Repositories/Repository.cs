using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Frank.EntityFrameworkCore.Repositories;

public class Repository<TContext, T> : IRepository<T> 
    where TContext : DbContext 
    where T : class
{
    private readonly TContext _context;

    public Repository(TContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity) 
        => await _context.AddAtomicAsync(entity);

    public async Task AddAsync(IEnumerable<T> entities) 
        => await _context.AddAtomicAsync(entities);

    public async Task<T> FindAsync(Expression<Func<T,bool>> predicate)
        => await Task.FromResult(_context.Set<T>().FirstOrDefault(predicate)) ?? throw new KeyNotFoundException();

    public async Task RemoveAsync(T entity) 
        => await _context.RemoveAtomicAsync(entity);
    
    public async Task RemoveAsync(Expression<Func<T,bool>> predicate) 
        => await _context.RemoveAtomicAsync(predicate);
    
    public Task UpdateAsync(T entity) 
        => _context.UpdateAtomicAsync(entity);

    public async Task UpdateAsync(Expression<Func<T,bool>> predicate, T entity) 
        => await _context.UpdateAtomicAsync(predicate, entity);

    public async Task UpdateAsync(Expression<Func<T,bool>> predicate, Action<T> action) 
        => await _context.UpdateAtomicAsync(predicate, action);
    
    public IQueryable<T> AsQueryable() 
        => _context.Set<T>();
    
    public async Task<long> CountAsync() 
        => await _context.Set<T>().LongCountAsync();

    public async Task<long> CountAsync(Expression<Func<T,bool>> predicate) 
        => await _context.Set<T>().LongCountAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T,bool>> predicate) 
        => await _context.Set<T>().AnyAsync(predicate);

    public async Task<bool> AllAsync(Expression<Func<T,bool>> predicate) 
        => await _context.Set<T>().AllAsync(predicate);
    
    public async Task UpsertEntityAsync(T entity, Expression<Func<T, bool>> matchOn, Action<T> updateAction, CancellationToken cancellationToken)
    {
        var existing = _context.Set<T>().FirstOrDefault(matchOn);
        if (existing == null)
        {
            _context.Set<T>().Add(entity);
        }
        else
        {
            updateAction(existing);
            _context.Entry(existing).CurrentValues.SetValues(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ChangeTracker.Clear();
    }

    public IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, bool descending, int skip, int take, CancellationToken cancellationToken = default) 
        => _context.Set<T>().Where(predicate)
            .AsEnumerable()
            .OrderBy(orderBy)
            .AsQueryable()
            .AsAsyncEnumerable();
    
    public IAsyncEnumerable<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _context.Set<T>().Where(predicate).AsAsyncEnumerable();

    public IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, int skip, int take, CancellationToken cancellationToken = default) =>
        _context.Set<T>().Where(predicate)
            .AsEnumerable()
            .Skip(skip)
            .Take(take)
            .AsQueryable()
            .AsAsyncEnumerable();

    public IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, CancellationToken cancellationToken = default) =>
        _context.Set<T>().Where(predicate)
            .AsEnumerable()
            .OrderBy(orderBy)
            .AsQueryable()
            .AsAsyncEnumerable();

    public IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, int skip, int take, CancellationToken cancellationToken = default) =>
        _context.Set<T>().Where(predicate)
            .AsEnumerable()
            .OrderBy(orderBy)
            .Skip(skip)
            .Take(take)
            .AsQueryable()
            .AsAsyncEnumerable();

    public IAsyncEnumerable<T> GetAsync(Expression<Func<T,bool>> predicate, Func<T, object> orderBy, bool descending, CancellationToken cancellationToken = default) =>
        _context.Set<T>().Where(predicate)
            .AsEnumerable()
            .OrderBy(orderBy)
            .AsQueryable()
            .AsAsyncEnumerable();

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) 
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task DiscardChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        await Task.CompletedTask;
    }
}
