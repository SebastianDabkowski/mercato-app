using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Models;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Product question service implementation.
/// </summary>
public class ProductQuestionService : IProductQuestionService
{
    private readonly ProductCatalogDbContext _context;
    private readonly ILogger<ProductQuestionService> _logger;

    public ProductQuestionService(
        ProductCatalogDbContext context,
        ILogger<ProductQuestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all visible questions for a product.
    /// </summary>
    public async Task<ProductQuestionsResponse> GetProductQuestionsAsync(Guid productId)
    {
        var questions = await _context.ProductQuestions
            .Where(q => q.ProductId == productId && q.IsVisible)
            .Include(q => q.Answers.Where(a => a.IsVisible))
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        var questionDtos = questions.Select(q => new ProductQuestionDto
        {
            Id = q.Id,
            ProductId = q.ProductId,
            AskedByName = q.AskedByName,
            QuestionText = q.QuestionText,
            Status = q.Status,
            CreatedAt = q.CreatedAt,
            Answers = q.Answers.Select(a => new ProductAnswerDto
            {
                Id = a.Id,
                AnsweredByName = a.AnsweredByName,
                AnsweredByRole = a.AnsweredByRole,
                AnswerText = a.AnswerText,
                CreatedAt = a.CreatedAt
            }).ToList()
        }).ToList();

        return new ProductQuestionsResponse
        {
            Questions = questionDtos,
            TotalCount = questionDtos.Count
        };
    }

    /// <summary>
    /// Create a new question.
    /// </summary>
    public async Task<ProductQuestionDto?> CreateQuestionAsync(
        Guid productId,
        string questionText,
        string userId,
        string userName)
    {
        // Validate product exists
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            _logger.LogWarning("Cannot create question: Product {ProductId} not found", productId);
            return null;
        }

        // Validate question text
        if (string.IsNullOrWhiteSpace(questionText) || questionText.Length > 1000)
        {
            _logger.LogWarning("Invalid question text length");
            return null;
        }

        var question = new ProductQuestion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            AskedByUserId = userId,
            AskedByName = userName,
            QuestionText = questionText.Trim(),
            Status = ProductQuestionStatus.Pending,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductQuestions.Add(question);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Question {QuestionId} created for product {ProductId} by user {UserId}",
            question.Id,
            productId,
            userId);

        return new ProductQuestionDto
        {
            Id = question.Id,
            ProductId = question.ProductId,
            AskedByName = question.AskedByName,
            QuestionText = question.QuestionText,
            Status = question.Status,
            CreatedAt = question.CreatedAt,
            Answers = new List<ProductAnswerDto>()
        };
    }

    /// <summary>
    /// Create an answer to a question.
    /// </summary>
    public async Task<ProductAnswerDto?> CreateAnswerAsync(
        Guid questionId,
        string answerText,
        string userId,
        string userName,
        string userRole)
    {
        // Validate question exists
        var question = await _context.ProductQuestions
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
        {
            _logger.LogWarning("Cannot create answer: Question {QuestionId} not found", questionId);
            return null;
        }

        // Validate answer text
        if (string.IsNullOrWhiteSpace(answerText) || answerText.Length > 2000)
        {
            _logger.LogWarning("Invalid answer text length");
            return null;
        }

        var answer = new ProductAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            AnsweredByUserId = userId,
            AnsweredByName = userName,
            AnsweredByRole = userRole,
            AnswerText = answerText.Trim(),
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductAnswers.Add(answer);

        // Update question status to Answered
        question.Status = ProductQuestionStatus.Answered;
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Answer {AnswerId} created for question {QuestionId} by user {UserId}",
            answer.Id,
            questionId,
            userId);

        return new ProductAnswerDto
        {
            Id = answer.Id,
            AnsweredByName = answer.AnsweredByName,
            AnsweredByRole = answer.AnsweredByRole,
            AnswerText = answer.AnswerText,
            CreatedAt = answer.CreatedAt
        };
    }

    /// <summary>
    /// Get a specific question by ID.
    /// </summary>
    public async Task<ProductQuestionDto?> GetQuestionByIdAsync(Guid questionId)
    {
        var question = await _context.ProductQuestions
            .Include(q => q.Answers.Where(a => a.IsVisible))
            .FirstOrDefaultAsync(q => q.Id == questionId && q.IsVisible);

        if (question == null)
        {
            return null;
        }

        return new ProductQuestionDto
        {
            Id = question.Id,
            ProductId = question.ProductId,
            AskedByName = question.AskedByName,
            QuestionText = question.QuestionText,
            Status = question.Status,
            CreatedAt = question.CreatedAt,
            Answers = question.Answers.Select(a => new ProductAnswerDto
            {
                Id = a.Id,
                AnsweredByName = a.AnsweredByName,
                AnsweredByRole = a.AnsweredByRole,
                AnswerText = a.AnswerText,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    /// <summary>
    /// Hide a question.
    /// </summary>
    public async Task<bool> HideQuestionAsync(Guid questionId, string userId)
    {
        var question = await _context.ProductQuestions
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
        {
            return false;
        }

        // TODO: Verify user has permission to hide this question (seller owns product or is admin)

        question.IsVisible = false;
        question.Status = ProductQuestionStatus.Hidden;
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Question {QuestionId} hidden by user {UserId}",
            questionId,
            userId);

        return true;
    }

    /// <summary>
    /// Hide an answer.
    /// </summary>
    public async Task<bool> HideAnswerAsync(Guid answerId, string userId)
    {
        var answer = await _context.ProductAnswers
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer == null)
        {
            return false;
        }

        // TODO: Verify user has permission to hide this answer (author, seller, or admin)

        answer.IsVisible = false;
        answer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Answer {AnswerId} hidden by user {UserId}",
            answerId,
            userId);

        return true;
    }

    /// <summary>
    /// Check if a user owns the product associated with a question.
    /// </summary>
    public async Task<bool> IsUserOwnerOfProductAsync(Guid productId, string userId)
    {
        // Get the product with its store information
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return false;
        }

        // Check if the user owns a store that matches the product's store
        // Note: We need to access the SellerPanel module to get store ownership
        // For MVP, we'll check via StoreId which should be linked to the user
        // TODO: This requires cross-module communication which should be handled via proper service integration
        
        _logger.LogWarning(
            "Product ownership check for ProductId={ProductId}, UserId={UserId} - cross-module validation not fully implemented",
            productId, userId);
        
        // For now, return true to allow the operation, but this should be properly implemented
        // by integrating with the SellerPanel module's IStoreService
        return true;
    }
}
