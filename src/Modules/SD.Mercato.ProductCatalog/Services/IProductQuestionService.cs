using SD.Mercato.ProductCatalog.DTOs;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Service for managing product questions and answers.
/// </summary>
public interface IProductQuestionService
{
    /// <summary>
    /// Get all questions for a specific product.
    /// </summary>
    Task<ProductQuestionsResponse> GetProductQuestionsAsync(Guid productId);

    /// <summary>
    /// Create a new question for a product.
    /// </summary>
    Task<ProductQuestionDto?> CreateQuestionAsync(
        Guid productId,
        string questionText,
        string userId,
        string userName);

    /// <summary>
    /// Create an answer to a question.
    /// </summary>
    Task<ProductAnswerDto?> CreateAnswerAsync(
        Guid questionId,
        string answerText,
        string userId,
        string userName,
        string userRole);

    /// <summary>
    /// Get a specific question by ID with answers.
    /// </summary>
    Task<ProductQuestionDto?> GetQuestionByIdAsync(Guid questionId);

    /// <summary>
    /// Hide a question (admin/seller only).
    /// </summary>
    Task<bool> HideQuestionAsync(Guid questionId, string userId);

    /// <summary>
    /// Hide an answer (admin/seller only).
    /// </summary>
    Task<bool> HideAnswerAsync(Guid answerId, string userId);

    /// <summary>
    /// Check if a user owns the product associated with a question.
    /// </summary>
    Task<bool> IsUserOwnerOfProductAsync(Guid productId, string userId);
}
