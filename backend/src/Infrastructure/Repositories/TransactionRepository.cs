using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Person)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByPersonIdAsync(Guid personId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Person)
            .Where(t => t.PersonId == personId)
            .ToListAsync();
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteRangeByPersonIdAsync(Guid personId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.PersonId == personId)
            .ToListAsync();

        _context.Transactions.RemoveRange(transactions);
        await _context.SaveChangesAsync();
    }
}
