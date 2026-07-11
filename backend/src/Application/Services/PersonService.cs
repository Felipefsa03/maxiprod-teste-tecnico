using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IValidator<CreatePersonRequest> _validator;

    public PersonService(IPersonRepository personRepository, IValidator<CreatePersonRequest> validator)
    {
        _personRepository = personRepository;
        _validator = validator;
    }

    public async Task<IEnumerable<PersonResponse>> GetAllAsync()
    {
        var persons = await _personRepository.GetAllAsync();
        return persons.Select(p => new PersonResponse(p.Id, p.Name, p.Age, p.CreatedAt));
    }

    public async Task<PersonResponse> CreateAsync(CreatePersonRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new FluentValidation.ValidationException(validation.Errors);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Age = request.Age
        };

        await _personRepository.AddAsync(person);

        return new PersonResponse(person.Id, person.Name, person.Age, person.CreatedAt);
    }

    public async Task DeleteAsync(Guid id)
    {
        var person = await _personRepository.GetByIdAsync(id)
            ?? throw new PersonNotFoundException(id);

        await _personRepository.DeleteAsync(person);
    }
}
