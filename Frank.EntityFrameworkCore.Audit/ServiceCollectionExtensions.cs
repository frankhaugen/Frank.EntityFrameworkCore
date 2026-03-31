using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Frank.EntityFrameworkCore.Audit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkAudit<TWriter, TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction) where TWriter : class, IAuditWriter where TContext : DbContext
    {
        services.AddScoped<IAuditWriter, TWriter>();
        services.AddScoped<AuditInterceptor>(sp => new AuditInterceptor(sp.GetRequiredService<IAuditWriter>()));
        services.AddDbContext<TContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            optionsAction.Invoke(options);
        });
        return services;
    }
}