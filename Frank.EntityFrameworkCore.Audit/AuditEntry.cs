using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Frank.EntityFrameworkCore.Audit;

public sealed class AuditEntry
{
    [Key]
    public long Id { get; set; }
    public string TableName { get; set; }
    public EntityState Action { get; set; }
    public string Key { get; set; }
    public string? OldValues { get; set; }
    public string NewValues { get; set; }
    public DateTime DateTime { get; set; }
}