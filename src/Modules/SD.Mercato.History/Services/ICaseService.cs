using SD.Mercato.History.DTOs;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service interface for case management (returns and complaints).
/// </summary>
public interface ICaseService
{
    /// <summary>
    /// Create a return request for a SubOrder.
    /// </summary>
    Task<CreateCaseResponseDto> CreateReturnRequestAsync(string buyerId, CreateReturnRequestDto request);

    /// <summary>
    /// Create a complaint for a SubOrder.
    /// </summary>
    Task<CreateCaseResponseDto> CreateComplaintAsync(string buyerId, CreateComplaintRequestDto request);

    /// <summary>
    /// Get all cases for a buyer.
    /// </summary>
    Task<CaseListResponseDto> GetCasesForBuyerAsync(string buyerId, CaseFilterRequestDto filter);

    /// <summary>
    /// Get all cases for a seller's store.
    /// </summary>
    Task<CaseListResponseDto> GetCasesForSellerAsync(Guid storeId, CaseFilterRequestDto filter);

    /// <summary>
    /// Get all cases (admin only).
    /// </summary>
    Task<CaseListResponseDto> GetAllCasesAsync(CaseFilterRequestDto filter);

    /// <summary>
    /// Get case details by ID.
    /// </summary>
    Task<CaseDto?> GetCaseByIdAsync(Guid caseId);

    /// <summary>
    /// Update case status (seller or admin).
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> UpdateCaseStatusAsync(Guid caseId, UpdateCaseStatusRequestDto request);

    /// <summary>
    /// Add a message to a case.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> AddCaseMessageAsync(Guid caseId, string senderId, string senderName, string senderRole, AddCaseMessageRequestDto request);
}
