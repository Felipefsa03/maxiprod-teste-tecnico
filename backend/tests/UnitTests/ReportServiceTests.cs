using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;
using FluentAssertions;

namespace UnitTests;

public class ReportServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _reportService = new ReportService(_transactionRepositoryMock.Object);
    }

    [Fact]
    public async Task GetTotalsAsync_ShouldCalculateCorrectTotals()
    {
        var personId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new() { Id = Guid.NewGuid(), Amount = 5000m, Type = TransactionType.Receita, PersonId = personId, Description = "Salário" },
            new() { Id = Guid.NewGuid(), Amount = 1000m, Type = TransactionType.Despesa, PersonId = personId, Description = "Aluguel" },
            new() { Id = Guid.NewGuid(), Amount = 500m, Type = TransactionType.Despesa, PersonId = personId, Description = "Mercado" },
            new() { Id = Guid.NewGuid(), Amount = 2000m, Type = TransactionType.Receita, PersonId = personId, Description = "Freelance" }
        };

        _transactionRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(transactions);

        var result = await _reportService.GetTotalsAsync();

        result.TotalReceitas.Should().Be(7000m);
        result.TotalDespesas.Should().Be(1500m);
        result.Saldo.Should().Be(5500m);
    }

    [Fact]
    public async Task GetTotalsAsync_WhenNoTransactions_ShouldReturnZeros()
    {
        _transactionRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Transaction>());

        var result = await _reportService.GetTotalsAsync();

        result.TotalReceitas.Should().Be(0m);
        result.TotalDespesas.Should().Be(0m);
        result.Saldo.Should().Be(0m);
    }
}
