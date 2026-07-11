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

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IValidator<CreatePersonRequest>> _validatorMock;
    private readonly PersonService _personService;

    public PersonServiceTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _validatorMock = new Mock<IValidator<CreatePersonRequest>>();
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreatePersonRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _personService = new PersonService(_personRepositoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnPersonResponse()
    {
        var request = new CreatePersonRequest("João", 25);
        _personRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync((Person p) => p);

        var result = await _personService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("João");
        result.Age.Should().Be(25);
        _personRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Person>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPersonExists_ShouldDelete()
    {
        var personId = Guid.NewGuid();
        var person = new Person { Id = personId, Name = "Maria", Age = 30 };
        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync(person);
        _personRepositoryMock.Setup(x => x.DeleteAsync(person))
            .Returns(Task.CompletedTask);

        await _personService.DeleteAsync(personId);

        _personRepositoryMock.Verify(x => x.DeleteAsync(person), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPersonNotExists_ShouldThrowException()
    {
        var personId = Guid.NewGuid();
        _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync((Person?)null);

        var act = () => _personService.DeleteAsync(personId);

        await act.Should().ThrowAsync<PersonNotFoundException>();
    }
}
