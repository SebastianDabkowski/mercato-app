using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for managing product questions and answers.
/// </summary>
[ApiController]
[Route("api/products/{productId:guid}/questions")]
public class ProductQuestionsController : ControllerBase
{
    private readonly IProductQuestionService _questionService;
    private readonly ILogger<ProductQuestionsController> _logger;

    public ProductQuestionsController(
        IProductQuestionService questionService,
        ILogger<ProductQuestionsController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all questions for a product.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProductQuestionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductQuestionsResponse>> GetProductQuestions(Guid productId)
    {
        var questions = await _questionService.GetProductQuestionsAsync(productId);
        return Ok(questions);
    }

    /// <summary>
    /// Create a new question for a product.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductQuestionDto>> CreateQuestion(
        Guid productId,
        [FromBody] CreateProductQuestionRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

        if (request.ProductId != productId)
        {
            return BadRequest(new { message = "Product ID in URL does not match request body" });
        }

        if (string.IsNullOrWhiteSpace(request.QuestionText) || request.QuestionText.Length > 1000)
        {
            return BadRequest(new { message = "Question text must be between 1 and 1000 characters" });
        }

        var question = await _questionService.CreateQuestionAsync(
            productId,
            request.QuestionText,
            userId,
            userName);

        if (question == null)
        {
            return BadRequest(new { message = "Failed to create question. Product may not exist." });
        }

        return CreatedAtAction(
            nameof(GetQuestionById),
            new { productId, questionId = question.Id },
            question);
    }

    /// <summary>
    /// Get a specific question by ID.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductQuestionDto>> GetQuestionById(Guid productId, Guid questionId)
    {
        var question = await _questionService.GetQuestionByIdAsync(questionId);

        if (question == null || question.ProductId != productId)
        {
            return NotFound(new { message = "Question not found" });
        }

        return Ok(question);
    }

    /// <summary>
    /// Create an answer to a question.
    /// </summary>
    [HttpPost("{questionId:guid}/answers")]
    [Authorize(Roles = "Seller,Administrator")]
    [ProducesResponseType(typeof(ProductAnswerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductAnswerDto>> CreateAnswer(
        Guid productId,
        Guid questionId,
        [FromBody] CreateProductAnswerRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

        if (request.QuestionId != questionId)
        {
            return BadRequest(new { message = "Question ID in URL does not match request body" });
        }

        if (string.IsNullOrWhiteSpace(request.AnswerText) || request.AnswerText.Length > 2000)
        {
            return BadRequest(new { message = "Answer text must be between 1 and 2000 characters" });
        }

        // TODO: Verify that seller owns the product or user is admin

        var answer = await _questionService.CreateAnswerAsync(
            questionId,
            request.AnswerText,
            userId,
            userName,
            userRole);

        if (answer == null)
        {
            return BadRequest(new { message = "Failed to create answer. Question may not exist." });
        }

        return CreatedAtAction(
            nameof(GetQuestionById),
            new { productId, questionId },
            answer);
    }

    /// <summary>
    /// Hide a question (admin/seller only).
    /// </summary>
    [HttpDelete("{questionId:guid}")]
    [Authorize(Roles = "Seller,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> HideQuestion(Guid productId, Guid questionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // TODO: Verify that seller owns the product or user is admin

        var success = await _questionService.HideQuestionAsync(questionId, userId);

        if (!success)
        {
            return NotFound(new { message = "Question not found" });
        }

        return NoContent();
    }
}
