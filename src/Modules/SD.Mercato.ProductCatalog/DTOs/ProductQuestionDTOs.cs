namespace SD.Mercato.ProductCatalog.DTOs;

/// <summary>
/// Request to create a product question.
/// </summary>
public record CreateProductQuestionRequest
{
    public required Guid ProductId { get; init; }
    public required string QuestionText { get; init; }
}

/// <summary>
/// Request to create an answer to a product question.
/// </summary>
public record CreateProductAnswerRequest
{
    public required Guid QuestionId { get; init; }
    public required string AnswerText { get; init; }
}

/// <summary>
/// DTO for a product question.
/// </summary>
public record ProductQuestionDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string AskedByName { get; init; }
    public required string QuestionText { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public List<ProductAnswerDto> Answers { get; init; } = new();
}

/// <summary>
/// DTO for a product answer.
/// </summary>
public record ProductAnswerDto
{
    public required Guid Id { get; init; }
    public required string AnsweredByName { get; init; }
    public required string AnsweredByRole { get; init; }
    public required string AnswerText { get; init; }
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Response containing a list of product questions.
/// </summary>
public record ProductQuestionsResponse
{
    public required List<ProductQuestionDto> Questions { get; init; }
    public required int TotalCount { get; init; }
}
