using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Services;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for admin audit log endpoints.
/// </summary>
[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Administrator")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogService auditLogService,
        ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Search and retrieve audit logs with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedAuditLogsResponse>> SearchAuditLogs([FromQuery] AuditLogSearchRequest request)
    {
        var result = await _auditLogService.SearchAuditLogsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get audit logs for a specific entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLogDto>>> GetEntityAuditLogs(string entityType, string entityId)
    {
        var logs = await _auditLogService.GetEntityAuditLogsAsync(entityType, entityId);
        return Ok(logs);
    }
}
