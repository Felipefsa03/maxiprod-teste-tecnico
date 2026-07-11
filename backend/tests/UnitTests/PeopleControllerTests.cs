using FluentAssertions;
using Api.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests;

public class PeopleControllerTests
{
    private readonly Mock<IPersonService> _personServiceMock;
    private readonly PeopleController _controller;

    public PeopleControllerTests()
    {
        _personServiceMock = new Mock<IPersonService>();
        _controller = new PeopleController(_personServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPersons()
    {
        var persons = new List<PersonResponse>
        {
            new(Guid.NewGuid(), "João", 25, DateTime.UtcNow),
            new(Guid.NewGuid(), "Maria", 30, DateTime.UtcNow)
        };
        _personServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(persons);

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPersons = okResult.Value.Should().BeAssignableTo<IEnumerable<PersonResponse>>().Subject;
        returnedPersons.Count().Should().Be(2);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var request = new CreatePersonRequest("João", 25);
        var person = new PersonResponse(Guid.NewGuid(), "João", 25, DateTime.UtcNow);
        _personServiceMock.Setup(x => x.CreateAsync(request)).ReturnsAsync(person);

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();
        _personServiceMock.Setup(x => x.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }
}
