namespace Frank.EntityFrameworkCore.Audit;

public interface IAuditWriter
{
    Task WriteAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default);
}