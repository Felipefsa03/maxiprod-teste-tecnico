using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using FluentAssertions;

namespace UnitTests;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IValidator<CreateTransactionRequest>> _validatorMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _validatorMock = new Mock<IValidator<CreateTransactionRequest>>();
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _transactionService = new TransactionService(
            _transactionRepositoryMock.Object,
            _personRepositoryMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenAdultCreatesRevenue_ShouldSucceed()
    {
        var personId = Guid.NewGuid();
        var person = new Person { Id = personId, Name = "João", Age = 25 };
        var request = new CreateTransactionRequest("Salário", 5000m, TransactionType.Receita, personId);

        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync(person);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        var result = await _transactionService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Description.Should().Be("Salário");
        result.Amount.Should().Be(5000m);
        result.Type.Should().Be(TransactionType.Receita);
    }

    [Fact]
    public async Task CreateAsync_WhenMinorCreatesRevenue_ShouldThrowException()
    {
        var personId = Guid.NewGuid();
        var person = new Person { Id = personId, Name = "Pedro", Age = 16 };
        var request = new CreateTransactionRequest("Mesada", 100m, TransactionType.Receita, personId);

        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync(person);

        var act = () => _transactionService.CreateAsync(request);

        await act.Should().ThrowAsync<MinorCannotCreateRevenueException>();
    }

    [Fact]
    public async Task CreateAsync_WhenMinorCreatesExpense_ShouldSucceed()
    {
        var personId = Guid.NewGuid();
        var person = new Person { Id = personId, Name = "Pedro", Age = 16 };
        var request = new CreateTransactionRequest("Lanche", 20m, TransactionType.Despesa, personId);

        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync(person);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        var result = await _transactionService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Type.Should().Be(TransactionType.Despesa);
    }

    [Fact]
    public async Task CreateAsync_WhenPersonNotFound_ShouldThrowException()
    {
        var personId = Guid.NewGuid();
        var request = new CreateTransactionRequest("Teste", 100m, TransactionType.Despesa, personId);

        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync((Person?)null);

        var act = () => _transactionService.CreateAsync(request);

        await act.Should().ThrowAsync<PersonNotFoundException>();
    }
}
