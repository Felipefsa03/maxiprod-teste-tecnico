using Domain.Entities;

namespace Domain.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<IEnumerable<Transaction>> GetByPersonIdAsync(Guid personId);
    Task<Transaction> AddAsync(Transaction transaction);
    Task DeleteRangeByPersonIdAsync(Guid personId);
}
