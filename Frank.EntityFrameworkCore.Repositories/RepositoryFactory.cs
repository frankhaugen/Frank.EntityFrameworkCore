using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Frank.EntityFrameworkCore.Repositories;

public class RepositoryFactory<TContext>(IServiceProvider serviceProvider)
    where TContext : DbContext
{
     private readonly ConcurrentDictionary<Type, object> _repositories = new();
    
    public IRepository<T> GetRepository<T>() where T : class
    {
        var context = serviceProvider.GetRequiredService<TContext>();
        return (IRepository<T>)_repositories.GetOrAdd(typeof(T), new Repository<TContext, T>(context));
    }
}