using System.ComponentModel.DataAnnotations;
using Frank.EntityFrameworkCore.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Frank.EntityFrameworkCore.Tests.Audit;

[JetBrains.Annotations.TestSubject(typeof(ServiceCollectionExtensions))]
public class EntityFrameworkAuditTests
{
    private readonly ITestOutputHelper _outputHelper;

    public EntityFrameworkAuditTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void AddEntityFrameworkAudit()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestOutputHelper>(_outputHelper);
        services.AddEntityFrameworkAudit<TestAuditWriter, TestContext>(options =>
        {
            options.UseSqlite("Data Source=MyDb2.db").EnableSensitiveDataLogging().EnableDetailedErrors();
        });
        var provider = services.BuildServiceProvider();
        var writer = provider.GetRequiredService<IAuditWriter>();
        var context = provider.GetRequiredService<TestContext>();
        Assert.NotNull(writer);
        Assert.NotNull(context);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        var newEntity = new TestEntity
        {
            Name = "Test",
        };
        
        context.TestEntities.Add(newEntity);
        context.SaveChanges();
        
        newEntity.Name = "Test2";
        context.SaveChanges();
        
        context.TestEntities.Remove(newEntity);
        context.SaveChanges();
    }
    
    public class TestAuditWriter : IAuditWriter
    {
        private readonly ITestOutputHelper _outputHelper;
        
        public TestAuditWriter(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
        
        /// <inheritdoc />
        public async Task WriteAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            _outputHelper.WriteLine(auditEntry);
            
            await Task.CompletedTask;
        }
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
