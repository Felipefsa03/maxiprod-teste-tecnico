using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IValidator<CreateTransactionRequest> _validator;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IPersonRepository personRepository,
        IValidator<CreateTransactionRequest> validator)
    {
        _transactionRepository = transactionRepository;
        _personRepository = personRepository;
        _validator = validator;
    }

    public async Task<IEnumerable<TransactionResponse>> GetAllAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return transactions.Select(t => new TransactionResponse(
            t.Id,
            t.Description,
            t.Amount,
            t.Type,
            t.PersonId,
            t.Person.Name,
            t.CreatedAt
        ));
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new FluentValidation.ValidationException(validation.Errors);

        var person = await _personRepository.GetByIdAsync(request.PersonId)
            ?? throw new PersonNotFoundException(request.PersonId);

        if (person.Age < 18 && request.Type == TransactionType.Receita)
            throw new MinorCannotCreateRevenueException();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            Amount = request.Amount,
            Type = request.Type,
            PersonId = request.PersonId
        };

        await _transactionRepository.AddAsync(transaction);

        return new TransactionResponse(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Type,
            transaction.PersonId,
            person.Name,
            transaction.CreatedAt
        );
    }
}
