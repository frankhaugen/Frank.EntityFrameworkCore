using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Frank.EntityFrameworkCore.Audit;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IAuditWriter _auditWriter;

    public AuditInterceptor(IAuditWriter auditWriter)
    {
        _auditWriter = auditWriter;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in eventData.Context?.ChangeTracker.Entries() ?? [])
        {
            var keyValues = string.Join(", ", entry.Metadata.FindPrimaryKey()?.Properties.Select(x => $"{x.Name}: {entry.Property(x.Name).CurrentValue}").ToList() ?? []);
            
            if (entry.State == EntityState.Added)
            {
                auditEntries.Add(new AuditEntry
                {
                    TableName = entry.Metadata.GetTableName(),
                    Key = keyValues,
                    Action = EntityState.Added,
                    NewValues = Serialize(entry.CurrentValues) ?? "Failed to serialize",
                    DateTime = DateTime.UtcNow
                });
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditEntries.Add(new AuditEntry
                {
                    TableName = entry.Metadata.GetTableName(),
                    Key = keyValues,
                    Action = EntityState.Deleted,
                    OldValues = Serialize(entry.OriginalValues) ?? "Failed to serialize",
                    DateTime = DateTime.UtcNow
                });
            }
            else if (entry.State == EntityState.Modified)
            {
                auditEntries.Add(new AuditEntry
                {
                    TableName = entry.Metadata.GetTableName(),
                    Key = keyValues,
                    Action = EntityState.Modified,
                    OldValues = Serialize(entry.OriginalValues),
                    NewValues = Serialize(entry.CurrentValues) ?? "Failed to serialize",
                    DateTime = DateTime.UtcNow
                });
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            _auditWriter.WriteAsync(auditEntry);
        }

        return base.SavingChanges(eventData, result);
    }
    
    private static string? Serialize(object? obj)
    {
        if (obj == null)
            return null;
        
        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() }, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReferenceHandler = ReferenceHandler.IgnoreCycles });
        }
        catch (Exception e)
        {
            return $"Failed to serialize: {e.Message}";
        }
    }
    
    private class PropertyValuesJsonConverter : JsonConverter<PropertyValues>
    {
        public override PropertyValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, PropertyValues value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var property in value.Properties)
            {
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, property.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}