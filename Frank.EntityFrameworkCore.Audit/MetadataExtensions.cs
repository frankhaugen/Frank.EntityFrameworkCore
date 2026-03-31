using Microsoft.EntityFrameworkCore.Metadata;

namespace Frank.EntityFrameworkCore.Audit;

internal static class MetadataExtensions
{
    public static string GetTableName(this IEntityType entityType)
    {
        var annotation = entityType.FindAnnotation("Relational:TableName");
        return annotation?.Value?.ToString() ?? entityType.GetTableName();
    }
}