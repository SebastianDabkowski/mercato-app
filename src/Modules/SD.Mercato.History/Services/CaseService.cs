using Microsoft.EntityFrameworkCore;
using SD.Mercato.History.Data;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Models;
using SD.Mercato.Notification.Services;
using SD.Mercato.Notification.Models;
using SD.Mercato.SellerPanel.Services;
using Microsoft.Extensions.Logging;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service implementation for case management (returns and complaints).
/// </summary>
public class CaseService : ICaseService
{
    private readonly HistoryDbContext _context;
    private readonly INotificationService? _notificationService;
    private readonly IStoreService? _storeService;
    private readonly ILogger<CaseService>? _logger;

    public CaseService(
        HistoryDbContext context,
        INotificationService? notificationService = null,
        IStoreService? storeService = null,
        ILogger<CaseService>? logger = null)
    {
        _context = context;
        _notificationService = notificationService;
        _storeService = storeService;
        _logger = logger;
    }

    public async Task<CreateCaseResponseDto> CreateReturnRequestAsync(string buyerId, CreateReturnRequestDto request)
    {
        // Validate SubOrder exists and belongs to buyer
        var subOrder = await _context.SubOrders
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.SubOrderId);

        if (subOrder == null)
        {
            throw new InvalidOperationException("SubOrder not found");
        }

        if (subOrder.Order?.UserId != buyerId)
        {
            throw new UnauthorizedAccessException("SubOrder does not belong to this buyer");
        }

        // TODO: Should we allow returns only for Delivered status, or also Completed?
        // TODO: What is the time limit for returns (e.g., 30 days after delivery)?
        if (subOrder.Status != SubOrderStatus.Delivered && subOrder.Status != SubOrderStatus.Shipped)
        {
            throw new InvalidOperationException($"Cannot create return for SubOrder with status '{subOrder.Status}'. Must be Shipped or Delivered.");
        }

        // Validate SubOrderItem if specified
        if (request.SubOrderItemId.HasValue)
        {
            var item = await _context.SubOrderItems
                .FirstOrDefaultAsync(i => i.Id == request.SubOrderItemId.Value && i.SubOrderId == request.SubOrderId);

            if (item == null)
            {
                throw new InvalidOperationException("SubOrderItem not found or does not belong to this SubOrder");
            }
        }

        // Generate case number
        var caseNumber = GenerateNextCaseNumberAsync();

        // Create the case
        var caseEntity = new Case
        {
            Id = Guid.NewGuid(),
            CaseNumber = caseNumber,
            CaseType = CaseTypes.Return,
            Status = CaseStatuses.New,
            BuyerId = buyerId,
            OrderId = subOrder.OrderId,
            SubOrderId = subOrder.Id,
            StoreId = subOrder.StoreId,
            SubOrderItemId = request.SubOrderItemId,
            Reason = request.Reason,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync();

        // Send notification to seller
        if (_notificationService != null && _storeService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var store = await _storeService.GetStoreByIdAsync(subOrder.StoreId);
                    if (store != null && !string.IsNullOrEmpty(store.ContactEmail))
                    {
                        await _notificationService.SendEmailNotificationAsync(
                            recipientUserId: store.OwnerUserId,
                            recipientEmail: store.ContactEmail,
                            eventType: NotificationEventTypes.CaseCreated,
                            subject: $"New Return Request - {caseEntity.CaseNumber}",
                            templateName: "CaseCreated",
                            templateData: new Dictionary<string, string>
                            {
                                { "SellerName", store.DisplayName },
                                { "CaseNumber", caseEntity.CaseNumber },
                                { "CaseType", caseEntity.CaseType },
                                { "OrderNumber", subOrder.SubOrderNumber },
                                { "Reason", caseEntity.Reason }
                            },
                            relatedEntityId: caseEntity.Id,
                            relatedEntityType: "Case"
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to send case notification for CaseId={CaseId}", caseEntity.Id);
                }
            });
        }

        return new CreateCaseResponseDto
        {
            CaseId = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            Message = "Return request created successfully"
        };
    }

    public async Task<CreateCaseResponseDto> CreateComplaintAsync(string buyerId, CreateComplaintRequestDto request)
    {
        // Validate SubOrder exists and belongs to buyer
        var subOrder = await _context.SubOrders
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.SubOrderId);

        if (subOrder == null)
        {
            throw new InvalidOperationException("SubOrder not found");
        }

        if (subOrder.Order?.UserId != buyerId)
        {
            throw new UnauthorizedAccessException("SubOrder does not belong to this buyer");
        }

        // Validate SubOrderItem if specified
        if (request.SubOrderItemId.HasValue)
        {
            var item = await _context.SubOrderItems
                .FirstOrDefaultAsync(i => i.Id == request.SubOrderItemId.Value && i.SubOrderId == request.SubOrderId);

            if (item == null)
            {
                throw new InvalidOperationException("SubOrderItem not found or does not belong to this SubOrder");
            }
        }

        // Generate case number
        var caseNumber = GenerateNextCaseNumberAsync();

        // Create the case
        var caseEntity = new Case
        {
            Id = Guid.NewGuid(),
            CaseNumber = caseNumber,
            CaseType = CaseTypes.Complaint,
            Status = CaseStatuses.New,
            BuyerId = buyerId,
            OrderId = subOrder.OrderId,
            SubOrderId = subOrder.Id,
            StoreId = subOrder.StoreId,
            SubOrderItemId = request.SubOrderItemId,
            Reason = request.Reason,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync();

        // Send notification to seller
        if (_notificationService != null && _storeService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var store = await _storeService.GetStoreByIdAsync(subOrder.StoreId);
                    if (store != null && !string.IsNullOrEmpty(store.ContactEmail))
                    {
                        await _notificationService.SendEmailNotificationAsync(
                            recipientUserId: store.OwnerUserId,
                            recipientEmail: store.ContactEmail,
                            eventType: NotificationEventTypes.CaseCreated,
                            subject: $"New Complaint - {caseEntity.CaseNumber}",
                            templateName: "CaseCreated",
                            templateData: new Dictionary<string, string>
                            {
                                { "SellerName", store.DisplayName },
                                { "CaseNumber", caseEntity.CaseNumber },
                                { "CaseType", caseEntity.CaseType },
                                { "OrderNumber", subOrder.SubOrderNumber },
                                { "Reason", caseEntity.Reason }
                            },
                            relatedEntityId: caseEntity.Id,
                            relatedEntityType: "Case"
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to send case notification for CaseId={CaseId}", caseEntity.Id);
                }
            });
        }

        return new CreateCaseResponseDto
        {
            CaseId = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            Message = "Complaint created successfully"
        };
    }

    public async Task<CaseListResponseDto> GetCasesForBuyerAsync(string buyerId, CaseFilterRequestDto filter)
    {
        var query = _context.Cases
            .Include(c => c.Order)
            .Include(c => c.SubOrder)
            .Include(c => c.SubOrderItem)
            .Include(c => c.Messages)
            .Where(c => c.BuyerId == buyerId);

        return await ApplyFiltersAndPaginateAsync(query, filter);
    }

    public async Task<CaseListResponseDto> GetCasesForSellerAsync(Guid storeId, CaseFilterRequestDto filter)
    {
        var query = _context.Cases
            .Include(c => c.Order)
            .Include(c => c.SubOrder)
            .Include(c => c.SubOrderItem)
            .Include(c => c.Messages)
            .Where(c => c.StoreId == storeId);

        return await ApplyFiltersAndPaginateAsync(query, filter);
    }

    public async Task<CaseListResponseDto> GetAllCasesAsync(CaseFilterRequestDto filter)
    {
        var query = _context.Cases
            .Include(c => c.Order)
            .Include(c => c.SubOrder)
            .Include(c => c.SubOrderItem)
            .Include(c => c.Messages);

        return await ApplyFiltersAndPaginateAsync(query, filter);
    }

    public async Task<CaseDto?> GetCaseByIdAsync(Guid caseId)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Order)
            .Include(c => c.SubOrder)
            .Include(c => c.SubOrderItem)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseEntity == null)
        {
            return null;
        }

        return MapToCaseDto(caseEntity);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateCaseStatusAsync(Guid caseId, UpdateCaseStatusRequestDto request)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
        {
            return (false, "Case not found");
        }

        // Validate status transition
        if (!CaseStatuses.ValidStatuses.Contains(request.Status))
        {
            return (false, $"Invalid status. Valid values are: {string.Join(", ", CaseStatuses.ValidStatuses)}");
        }

        // TODO: Should we enforce status workflow? (e.g., New -> In Review -> Accepted/Rejected -> Resolved)
        // For MVP, we allow any status transition

        caseEntity.Status = request.Status;
        caseEntity.UpdatedAt = DateTime.UtcNow;

        if (request.Status == CaseStatuses.Resolved)
        {
            caseEntity.ResolvedAt = DateTime.UtcNow;
            caseEntity.Resolution = request.Resolution;
        }

        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> AddCaseMessageAsync(Guid caseId, string senderId, string senderName, string senderRole, AddCaseMessageRequestDto request)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
        {
            return (false, "Case not found");
        }

        var message = new CaseMessage
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            SenderId = senderId,
            SenderName = senderName,
            SenderRole = senderRole,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        _context.CaseMessages.Add(message);
        
        // Update case UpdatedAt timestamp
        caseEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    private async Task<CaseListResponseDto> ApplyFiltersAndPaginateAsync(IQueryable<Case> query, CaseFilterRequestDto filter)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(c => c.Status == filter.Status);
        }

        if (!string.IsNullOrEmpty(filter.CaseType))
        {
            query = query.Where(c => c.CaseType == filter.CaseType);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply ordering and pagination
        var cases = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        return new CaseListResponseDto
        {
            Cases = cases.Select(MapToCaseDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }

    private CaseDto MapToCaseDto(Case caseEntity)
    {
        return new CaseDto
        {
            Id = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            CaseType = caseEntity.CaseType,
            Status = caseEntity.Status,
            BuyerId = caseEntity.BuyerId,
            OrderId = caseEntity.OrderId,
            OrderNumber = caseEntity.Order?.OrderNumber ?? string.Empty,
            SubOrderId = caseEntity.SubOrderId,
            SubOrderNumber = caseEntity.SubOrder?.SubOrderNumber ?? string.Empty,
            StoreId = caseEntity.StoreId,
            StoreName = caseEntity.SubOrder?.StoreName ?? string.Empty,
            SubOrderItemId = caseEntity.SubOrderItemId,
            ProductTitle = caseEntity.SubOrderItem?.ProductTitle,
            Reason = caseEntity.Reason,
            Description = caseEntity.Description,
            Resolution = caseEntity.Resolution,
            Messages = caseEntity.Messages?.Select(m => new CaseMessageDto
            {
                Id = m.Id,
                CaseId = m.CaseId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                SenderRole = m.SenderRole,
                Message = m.Message,
                CreatedAt = m.CreatedAt
            }).OrderBy(m => m.CreatedAt).ToList() ?? new List<CaseMessageDto>(),
            CreatedAt = caseEntity.CreatedAt,
            UpdatedAt = caseEntity.UpdatedAt,
            ResolvedAt = caseEntity.ResolvedAt
        };
    }

    private static string GenerateNextCaseNumberAsync()
    {
        // Generate unique case number: CASE-YYYY-XXXXXXXXXXXX
        // Using year + GUID part for uniqueness (similar to OrderNumber pattern)
        // This avoids race conditions that could occur with sequential numbering
        var today = DateTime.UtcNow;
        var year = today.Year;
        var guidPart = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        return $"CASE-{year}-{guidPart}";
    }
}
