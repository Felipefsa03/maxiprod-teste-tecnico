# Estratégia de Testes

## Filosofia

Eu divido meus testes em duas categorias: **unit tests** (que testam units de lógica isoladamente) e **integration tests** (que testam o fluxo completo passando por HTTP, controller, service, repository e banco). Não escrevi testes E2E porque para um teste técnico, o custo de setup de um browser headless não se justifica.

## Ferramentas

- **xUnit** — framework de testes (padrão da comunidade .NET)
- **Moq** — mock framework para isolar dependências nos unit tests
- **FluentAssertions** — assertions mais legíveis (`result.Should().NotBeNull()` em vez de `Assert.NotNull(result)`)
- **WebApplicationFactory** — para testes de integração, spin up a API em memória

## Unit Tests (25 testes)

### PersonServiceTests

Testo o `PersonService` isoladamente, mockando o `IPersonRepository` e o `IValidator<CreatePersonRequest>`:

```csharp
_validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreatePersonRequest>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult());
```

**Por que mocko o validator?** Porque o unit test do service testa a lógica de orquestração (criar pessoa, persistir), não as regras de validação. As regras de validação são testadas isoladamente nos `ValidatorTests`.

Cenários testados:
- `CreateAsync_ShouldReturnPersonResponse` — happy path
- `DeleteAsync_WhenPersonExists_ShouldDelete` — delete com sucesso
- `DeleteAsync_WhenPersonNotExists_ShouldThrowException` — exception quando pessoa não existe

### TransactionServiceTests

Testo as regras de negócio de transação, que são as mais complexas:

Cenários testados:
- `CreateAsync_WhenAdultCreatesRevenue_ShouldSucceed` — adulto cria receita
- `CreateAsync_WhenMinorCreatesRevenue_ShouldThrowException` — menor NÃO pode criar receita
- `CreateAsync_WhenMinorCreatesExpense_ShouldSucceed` — menor PODE criar despesa
- `CreateAsync_WhenPersonNotFound_ShouldThrowException` — pessoa não existe

**Por que esse último é o teste mais importante?** Porque testa a regra de negócio central do desafio. Se eu remover esse teste e quebrar a regra, o teste não vai falhar.

### ValidatorTests

Testo cada regra de validação individualmente:

```csharp
[Fact]
public async Task Should_Fail_When_Name_Is_Empty()
{
    var result = await validator.ValidateAsync(new CreatePersonRequest("", 25));
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Name");
}
```

Isso garante que cada regra de validação funciona isoladamente. Se eu adicionar uma nova regra (ex: nome não pode ter caracteres especiais), eu adiciono um teste específico para ela.

### PeopleControllerTests

Testo o controller isoladamente, mockando o service:

```csharp
[Fact]
public async Task Create_ShouldReturnCreatedAtAction()
{
    var result = await _controller.Create(request);
    var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
}
```

Isso valida que o controller retorna o status code correto (201 Created) e o formato de resposta correto.

## Integration Tests (7 testes)

### CustomWebApplicationFactory

Crio uma factory customizada que substitui o banco real por SQLite em memória:

```csharp
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
            // Remove o registro existente do DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Registra com SQLite em memória
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            // Cria as tabelas
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
```

**Por que SQLite em memória e não um mock?** Porque quero testar que o EF Core mapeia corretamente as entidades, que as constraints funcionam, e que o cascade delete funciona. Um mock não testaria isso. SQLite em memória é rápido e isolado — cada teste começa com um banco limpo (porque a connection é compartilhada mas os dados não persistem entre requests no mesmo teste).

### Cenários de integração testados

1. **CreatePerson_ShouldReturnCreated** — POST /api/people retorna 201 com a pessoa criada
2. **GetAllPeople_ShouldReturnOk** — GET /api/people retorna 200
3. **CreateTransaction_WhenValid_ShouldReturnCreated** — cria pessoa + transação, retorna 201
4. **CreateTransaction_WhenMinorAndRevenue_ShouldReturnBadRequest** — menor tenta criar receita, retorna 400
5. **GetTotals_ShouldReturnOk** — GET /api/reports/totals retorna 200
6. **DeletePerson_ShouldReturnNoContent** — DELETE retorna 204
7. **DeletePerson_WhenNotFound_ShouldReturnNotFound** — DELETE de ID inexistente retorna 404

**Por que não testo validação nos integration tests?** Porque os unit tests de validator já cobrem isso. Os integration tests focam no fluxo HTTP completo — garantir que o request chega ao controller, o controller chama o service, o service chama o repository, e o banco responde corretamente.

## Rodando os testes

```bash
cd backend
dotnet test
```

Todos os 32 testes devem passar (25 unit + 7 integration).
