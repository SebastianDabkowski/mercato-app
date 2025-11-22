using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.DTOs;

/// <summary>
/// Request to create a return case.
/// </summary>
public record CreateReturnRequestDto
{
    /// <summary>
    /// SubOrder ID to create return for.
    /// </summary>
    public required Guid SubOrderId { get; init; }

    /// <summary>
    /// Optional: Specific item ID if return is for a single item.
    /// </summary>
    public Guid? SubOrderItemId { get; init; }

    /// <summary>
    /// Reason for return (e.g., "Defective", "Wrong item", "Not as described").
    /// </summary>
    [MaxLength(100)]
    public required string Reason { get; init; }

    /// <summary>
    /// Detailed description from buyer.
    /// </summary>
    [MaxLength(2000)]
    public required string Description { get; init; }
}

/// <summary>
/// Request to create a complaint case.
/// </summary>
public record CreateComplaintRequestDto
{
    /// <summary>
    /// SubOrder ID to create complaint for.
    /// </summary>
    public required Guid SubOrderId { get; init; }

    /// <summary>
    /// Optional: Specific item ID if complaint is for a single item.
    /// </summary>
    public Guid? SubOrderItemId { get; init; }

    /// <summary>
    /// Reason for complaint (e.g., "Late delivery", "Poor quality", "Damaged packaging").
    /// </summary>
    [MaxLength(100)]
    public required string Reason { get; init; }

    /// <summary>
    /// Detailed description from buyer.
    /// </summary>
    [MaxLength(2000)]
    public required string Description { get; init; }
}

/// <summary>
/// Request to update case status.
/// </summary>
public record UpdateCaseStatusRequestDto
{
    /// <summary>
    /// New status for the case.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Optional resolution notes when closing the case.
    /// </summary>
    [MaxLength(1000)]
    public string? Resolution { get; init; }
}

/// <summary>
/// Request to add a message to a case.
/// </summary>
public record AddCaseMessageRequestDto
{
    /// <summary>
    /// Message content.
    /// </summary>
    [MaxLength(2000)]
    public required string Message { get; init; }
}

/// <summary>
/// DTO for Case entity.
/// </summary>
public record CaseDto
{
    public Guid Id { get; init; }
    public string CaseNumber { get; init; } = string.Empty;
    public string CaseType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string BuyerId { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid SubOrderId { get; init; }
    public string SubOrderNumber { get; init; } = string.Empty;
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public Guid? SubOrderItemId { get; init; }
    public string? ProductTitle { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Resolution { get; init; }
    public List<CaseMessageDto> Messages { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

/// <summary>
/// DTO for CaseMessage entity.
/// </summary>
public record CaseMessageDto
{
    public Guid Id { get; init; }
    public Guid CaseId { get; init; }
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string SenderRole { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Response for case creation.
/// </summary>
public record CreateCaseResponseDto
{
    public Guid CaseId { get; init; }
    public string CaseNumber { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response for listing cases.
/// </summary>
public record CaseListResponseDto
{
    public List<CaseDto> Cases { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Filter request for case listing.
/// </summary>
public record CaseFilterRequestDto
{
    public string? Status { get; init; }
    public string? CaseType { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
