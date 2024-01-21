using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Frank.EntityFrameworkCore.Repositories;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        var entityTypes = typeof(TContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && 
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments().First());

        foreach (var type in entityTypes)
        {
            services.AddScoped(typeof(IRepository<>).MakeGenericType(type), 
                typeof(Repository<,>).MakeGenericType(typeof(TContext), type));
        }
    }
}