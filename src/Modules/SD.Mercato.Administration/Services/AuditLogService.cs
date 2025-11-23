using Microsoft.EntityFrameworkCore;
using SD.Mercato.Administration.Data;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Models;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Service for managing admin audit logs.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly AdministrationDbContext _context;

    public AuditLogService(AdministrationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(
        string adminUserId,
        string adminEmail,
        string action,
        string entityType,
        string entityId,
        string description,
        string? changesJson = null,
        string? ipAddress = null)
    {
        var auditLog = new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            AdminEmail = adminEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            ChangesJson = changesJson,
            IpAddress = ipAddress,
            PerformedAt = DateTime.UtcNow
        };

        _context.AdminAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedAuditLogsResponse> SearchAuditLogsAsync(AuditLogSearchRequest request)
    {
        // Validate pagination parameters
        request.ValidateAndNormalize();

        var query = _context.AdminAuditLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.AdminUserId))
        {
            query = query.Where(x => x.AdminUserId == request.AdminUserId);
        }

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(x => x.EntityType == request.EntityType);
        }

        if (!string.IsNullOrEmpty(request.EntityId))
        {
            query = query.Where(x => x.EntityId == request.EntityId);
        }

        if (!string.IsNullOrEmpty(request.Action))
        {
            query = query.Where(x => x.Action == request.Action);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.PerformedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.PerformedAt <= request.ToDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var logs = await query
            .OrderByDescending(x => x.PerformedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                AdminUserId = x.AdminUserId,
                AdminEmail = x.AdminEmail,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Description = x.Description,
                ChangesJson = x.ChangesJson,
                IpAddress = x.IpAddress,
                PerformedAt = x.PerformedAt
            })
            .ToListAsync();

        return new PaginatedAuditLogsResponse
        {
            Logs = logs,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<List<AuditLogDto>> GetEntityAuditLogsAsync(string entityType, string entityId)
    {
        return await _context.AdminAuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.PerformedAt)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                AdminUserId = x.AdminUserId,
                AdminEmail = x.AdminEmail,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Description = x.Description,
                ChangesJson = x.ChangesJson,
                IpAddress = x.IpAddress,
                PerformedAt = x.PerformedAt
            })
            .ToListAsync();
    }
}
