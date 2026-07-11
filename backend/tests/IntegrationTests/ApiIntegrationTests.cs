using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.DTOs;
using Domain.Enums;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnCreated()
    {
        var request = new CreatePersonRequest("João Silva", 25);

        var response = await _client.PostAsJsonAsync("/api/people", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var person = await response.Content.ReadFromJsonAsync<PersonResponse>();
        person.Should().NotBeNull();
        person!.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetAllPeople_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/people");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var persons = await response.Content.ReadFromJsonAsync<IEnumerable<PersonResponse>>();
        persons.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTransaction_WhenValid_ShouldReturnCreated()
    {
        var personRequest = new CreatePersonRequest("Maria", 30);
        var personResponse = await _client.PostAsJsonAsync("/api/people", personRequest);
        var person = await personResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var transactionRequest = new CreateTransactionRequest("Salário", 5000m, TransactionType.Receita, person!.Id);
        var response = await _client.PostAsJsonAsync("/api/transactions", transactionRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTransaction_WhenMinorAndRevenue_ShouldReturnBadRequest()
    {
        var personRequest = new CreatePersonRequest("Pedro", 16);
        var personResponse = await _client.PostAsJsonAsync("/api/people", personRequest);
        var person = await personResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var transactionRequest = new CreateTransactionRequest("Mesada", 100m, TransactionType.Receita, person!.Id);
        var response = await _client.PostAsJsonAsync("/api/transactions", transactionRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTotals_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/reports/totals");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var totals = await response.Content.ReadFromJsonAsync<TotalsResponse>();
        totals.Should().NotBeNull();
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnNoContent()
    {
        var personRequest = new CreatePersonRequest("ToDelete", 25);
        var personResponse = await _client.PostAsJsonAsync("/api/people", personRequest);
        var person = await personResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var response = await _client.DeleteAsync($"/api/people/{person!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePerson_WhenNotFound_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync($"/api/people/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
