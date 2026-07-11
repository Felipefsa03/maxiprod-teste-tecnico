using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;

    public ReportService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TotalsResponse> GetTotalsAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();

        var totalReceitas = transactions
            .Where(t => t.Type == TransactionType.Receita)
            .Sum(t => t.Amount);

        var totalDespesas = transactions
            .Where(t => t.Type == TransactionType.Despesa)
            .Sum(t => t.Amount);

        return new TotalsResponse(totalReceitas, totalDespesas, totalReceitas - totalDespesas);
    }
}
