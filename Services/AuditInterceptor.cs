using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using academia.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace academia.Services
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditInterceptor> _logger;
        private List<AuditEntry> _auditEntries = new();

        // Tablas a excluir de la auditoría
        private static readonly HashSet<string> TablasExcluidas = new()
        {
            "audit_log" // No auditar la tabla de auditoría
        };

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<AuditInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context != null)
            {
                PrepararAuditEntries(eventData.Context);
            }
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                PrepararAuditEntries(eventData.Context);
            }
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (eventData.Context != null)
            {
                GuardarAuditLogs(eventData.Context);
            }
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                await GuardarAuditLogsAsync(eventData.Context);
            }
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void PrepararAuditEntries(DbContext context)
        {
            _auditEntries.Clear();
            context.ChangeTracker.DetectChanges();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // Ignorar entidades sin cambios o la tabla de auditoría
                if (entry.State == EntityState.Detached || 
                    entry.State == EntityState.Unchanged ||
                    entry.Entity is AuditLog)
                {
                    continue;
                }

                var tableName = entry.Metadata.GetTableName();
                if (tableName != null && TablasExcluidas.Contains(tableName))
                {
                    continue;
                }

                var auditEntry = new AuditEntry
                {
                    TableName = tableName ?? entry.Entity.GetType().Name,
                    EntityEntry = entry
                };

                // Obtener información del contexto HTTP
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    auditEntry.IpAddress = ObtenerIpAddress(httpContext);
                    auditEntry.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                    auditEntry.Endpoint = httpContext.Request.Path.ToString();
                    auditEntry.MetodoHttp = httpContext.Request.Method;

                    var userIdClaim = httpContext.User.FindFirst("id")?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        auditEntry.UserId = userId;
                    }
                    auditEntry.UserName = httpContext.User.FindFirst("nombre")?.Value;
                    auditEntry.UserRole = httpContext.User.FindFirst("rol")?.Value;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.TipoOperacion = TipoOperacionAudit.Insert;
                        foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                                auditEntry.TemporaryProperties.Add(property);
                            }
                            auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        break;

                    case EntityState.Deleted:
                        auditEntry.TipoOperacion = TipoOperacionAudit.Delete;
                        foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.KeyValues[property.Metadata.Name] = property.OriginalValue;
                            }
                            auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                        }
                        break;

                    case EntityState.Modified:
                        auditEntry.TipoOperacion = TipoOperacionAudit.Update;
                        foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                                continue;
                            }

                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(property.Metadata.Name);
                                auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                                auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                            }
                        }
                        break;
                }

                _auditEntries.Add(auditEntry);
            }
        }

        private void GuardarAuditLogs(DbContext context)
        {
            if (!_auditEntries.Any()) return;

            try
            {
                foreach (var auditEntry in _auditEntries)
                {
                    // Actualizar valores de claves temporales (para INSERTs)
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    var auditLog = auditEntry.ToAuditLog();
                    context.Set<AuditLog>().Add(auditLog);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar logs de auditoría");
            }
            finally
            {
                _auditEntries.Clear();
            }
        }

        private async Task GuardarAuditLogsAsync(DbContext context)
        {
            if (!_auditEntries.Any()) return;

            try
            {
                foreach (var auditEntry in _auditEntries)
                {
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    var auditLog = auditEntry.ToAuditLog();
                    context.Set<AuditLog>().Add(auditLog);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar logs de auditoría");
            }
            finally
            {
                _auditEntries.Clear();
            }
        }

        private static string? ObtenerIpAddress(HttpContext httpContext)
        {
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }

    public class AuditEntry
    {
        public EntityEntry EntityEntry { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public string TipoOperacion { get; set; } = null!;
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<string> ChangedColumns { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        
        // Información del contexto
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Endpoint { get; set; }
        public string? MetodoHttp { get; set; }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public AuditLog ToAuditLog()
        {
            var keyValue = KeyValues.Count > 0 
                ? string.Join(",", KeyValues.Values.Select(v => v?.ToString() ?? "null"))
                : null;

            return new AuditLog
            {
                TablaAfectada = TableName,
                TipoOperacion = TipoOperacion,
                IdRegistro = keyValue,
                DatosAnteriores = OldValues.Count > 0 ? SerializeToJson(OldValues) : null,
                DatosNuevos = NewValues.Count > 0 ? SerializeToJson(NewValues) : null,
                CamposModificados = ChangedColumns.Count > 0 ? JsonSerializer.Serialize(ChangedColumns) : null,
                IdUsuario = UserId,
                NombreUsuario = UserName,
                RolUsuario = UserRole,
                IpAddress = IpAddress,
                UserAgent = UserAgent?.Length > 500 ? UserAgent[..500] : UserAgent,
                Endpoint = Endpoint,
                MetodoHttp = MetodoHttp,
                FechaOperacion = DateTime.UtcNow,
                Exitoso = true
            };
        }

        private static string SerializeToJson(Dictionary<string, object?> values)
        {
            // Convertir valores que no son serializables
            var safeValues = new Dictionary<string, object?>();
            foreach (var kvp in values)
            {
                var value = kvp.Value;
                if (value == null)
                {
                    safeValues[kvp.Key] = null;
                }
                else if (value is DateTime dt)
                {
                    safeValues[kvp.Key] = dt.ToString("O");
                }
                else if (value.GetType().IsPrimitive || value is string || value is decimal)
                {
                    safeValues[kvp.Key] = value;
                }
                else
                {
                    safeValues[kvp.Key] = value.ToString();
                }
            }
            return JsonSerializer.Serialize(safeValues, JsonOptions);
        }
    }
}
