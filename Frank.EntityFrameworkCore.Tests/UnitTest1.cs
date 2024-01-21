using Frank.EntityFrameworkCore.Repositories;
using Frank.PulseFlow;
using Frank.Testing.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Frank.EntityFrameworkCore.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _outputHelper;

    public UnitTest1(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task Test1()
    {
        var serviceProvider = CreateServiceProvider();
        var repository = serviceProvider.GetRequiredService<IRepository<TestEntity>>();
        var entity = new TestEntity { Name = "Test" };
        await repository.AddAsync(entity);
        var count = await repository.CountAsync();
        _outputHelper.WriteLine($"Count: {count}");
        await foreach (var entity2 in repository.GetAsync(e => e.Id == entity.Id))
        {
            Assert.Equal(entity.Name, entity2.Name);
        }
        await repository.RemoveAsync(entity);
        count = await repository.CountAsync();
        _outputHelper.WriteLine($"Count: {count}");
    }
    
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTestLogging(_outputHelper);
        services.AddRepositories<TestContext>();
        services.AddScoped(CreateContext);
        var serviceProvider = services.BuildServiceProvider();
        
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider;
    }
    
    private static TestContext CreateContext(IServiceProvider serviceProvider)
    {
        var context = new DbContextBuilder<TestContext>()
            .WithLoggerProvider(serviceProvider.GetRequiredService<ILoggerProvider>())
            .WithOptions(options => options.UseSqlite("Data Source=MyDb.db"))
            .Build();
        
        context.Database.EnsureCreated();
        return context;
    }
    
    private class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
    }
}

internal class DbContextBuilder<T> where T : DbContext
{
    private readonly DbContextOptionsBuilder<T> _optionsBuilder = new();

    public DbContextBuilder<T> WithOptions(Action<DbContextOptionsBuilder<T>> options)
    {
        options(_optionsBuilder);
        return this;
    }

    public DbContextBuilder<T> WithLoggerProvider(ILoggerProvider loggerProvider)
    {
        _optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddProvider(loggerProvider)));
        return this;
    }

    public T Build() => (T)Activator.CreateInstance(typeof(T), _optionsBuilder.Options)!;
}

