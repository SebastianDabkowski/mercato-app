using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Reports.Data;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Models;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Implementation of commission configuration service.
/// </summary>
public class CommissionConfigService : ICommissionConfigService
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<CommissionConfigService> _logger;

    public CommissionConfigService(
        ReportsDbContext context,
        ILogger<CommissionConfigService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GlobalCommissionConfigDto?> GetActiveConfigAsync()
    {
        _logger.LogInformation("Getting active global commission configuration");

        var config = await _context.GlobalCommissionConfigs
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            _logger.LogWarning("No active global commission configuration found");
            return null;
        }

        return new GlobalCommissionConfigDto
        {
            Id = config.Id,
            DefaultCommissionRate = config.DefaultCommissionRate,
            Notes = config.Notes,
            LastModifiedBy = config.LastModifiedBy,
            UpdatedAt = config.UpdatedAt
        };
    }

    public async Task<GlobalCommissionConfigDto> UpdateConfigAsync(UpdateCommissionConfigRequest request)
    {
        _logger.LogInformation("Updating global commission configuration: Rate={Rate}, ModifiedBy={ModifiedBy}",
            request.DefaultCommissionRate, request.ModifiedBy);

        // Validate commission rate
        if (request.DefaultCommissionRate < 0 || request.DefaultCommissionRate > 1)
        {
            throw new ArgumentException("Commission rate must be between 0 and 1 (0% to 100%)");
        }

        // Get active config
        var config = await _context.GlobalCommissionConfigs
            .Where(c => c.IsActive)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            // Create new config if none exists
            config = new GlobalCommissionConfig
            {
                Id = Guid.NewGuid(),
                DefaultCommissionRate = request.DefaultCommissionRate,
                Notes = request.Notes,
                IsActive = true,
                LastModifiedBy = request.ModifiedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GlobalCommissionConfigs.Add(config);
        }
        else
        {
            // Update existing config
            config.DefaultCommissionRate = request.DefaultCommissionRate;
            config.Notes = request.Notes;
            config.LastModifiedBy = request.ModifiedBy;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Global commission configuration updated: {ConfigId}", config.Id);

        return new GlobalCommissionConfigDto
        {
            Id = config.Id,
            DefaultCommissionRate = config.DefaultCommissionRate,
            Notes = config.Notes,
            LastModifiedBy = config.LastModifiedBy,
            UpdatedAt = config.UpdatedAt
        };
    }
}
