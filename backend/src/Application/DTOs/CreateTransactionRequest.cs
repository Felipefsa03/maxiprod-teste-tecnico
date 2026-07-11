using Domain.Enums;

namespace Application.DTOs;

public record CreateTransactionRequest(
    string Description,
    decimal Amount,
    TransactionType Type,
    Guid PersonId
);
