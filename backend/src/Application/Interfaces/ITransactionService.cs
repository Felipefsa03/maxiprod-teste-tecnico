using Application.DTOs;

namespace Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionResponse>> GetAllAsync();
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request);
}
