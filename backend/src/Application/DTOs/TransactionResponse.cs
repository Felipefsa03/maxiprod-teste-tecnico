using Domain.Enums;

namespace Application.DTOs;

public record TransactionResponse(
    Guid Id,
    string Description,
    decimal Amount,
    TransactionType Type,
    Guid PersonId,
    string PersonName,
    DateTime CreatedAt
);
