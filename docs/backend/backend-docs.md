# Backend — Estrutura e Decisões de Design

## Organização dos projetos

```
backend/
├── src/
│   ├── Domain/           # Entidades, enums, exceptions, interfaces
│   ├── Application/      # Services, DTOs, validators, DI registration
│   ├── Infrastructure/   # EF Core, repositories, DbContext
│   └── Api/              # Controllers, middleware, Program.cs
├── tests/
│   ├── UnitTests/        # Testes isolados com Moq
│   └── IntegrationTests/ # Testes de ponta a ponta com WebApplicationFactory
└── ControleGastos.sln
```

## Domain

### Entidades

Escolhi `Person` e `Transaction` como as duas entidades centrais. `Transaction` tem um FK para `Person` (One-to-Many), e mantive o relacionamento explícito com navigation property.

```csharp
public class Transaction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
}
```

**Por que `Guid` para Id?** Evita problemas de auto-increment em SQLite (que lida com GUID de forma diferente de SQL Server), e é mais seguro para expor em URLs da API.

**Por que `DateTime.UtcNow` no default?** Evita problemas de timezone. Todas as datas são armazenadas em UTC.

### Enums

```csharp
public enum TransactionType
{
    Receita = 0,
    Despesa = 1
}
```

Usei enum numérico explícito (0, 1) para evitar ambiguidades com serialização JSON.

### Exceções de Domínio

Criei 3 exceções hierárquicas:
- `DomainException` — base para todas as exceções de domínio
- `PersonNotFoundException` — quando tento operar com uma pessoa que não existe
- `MinorCannotCreateRevenueException` — regra de negócio: menores de 18 não criam receita

A hierarquia permite que o `ExceptionMiddleware` trate exceções de domínio de forma genérica (retornando 400) enquanto exceções específicas têm tratamento próprio (retornando 404 para pessoa não encontrada).

### Interfaces de Repository

Defini `IPersonRepository` e `ITransactionRepository` no Domain, mas as implementações ficam no Infrastructure. Isso mantém o Domain desacoplado do EF Core.

## Application

### Services

Cada service injeta sua interface de repositório e um `IValidator<T>`:

```csharp
public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IValidator<CreatePersonRequest> _validator;
    // ...
}
```

**Por que valido no service e não no controller?**
1. Os services são testáveis isoladamente — posso testar se a validação rejeita input inválido sem levantar um servidor HTTP
2. A regra de negócio (menores não podem criar receita) é validada junto com as regras de formato (nome não vazio, valor > 0) — tudo no mesmo fluxo
3. Controllers ficam finos — apenas adaptam HTTP para chamadas de serviço

### DTOs

Usei `record` types para DTOs:

```csharp
public record CreatePersonRequest(string Name, int Age);
public record PersonResponse(Guid Id, string Name, int Age, DateTime CreatedAt);
```

Records são imutáveis por natureza, têm equality por valor, e são mais concisos que classes. Para DTOs que são apenas containers de dados, isso é perfeito.

### FluentValidation

Criei um validator para cada request DTO:

```csharp
public class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
{
    public CreatePersonRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Age)
            .InclusiveBetween(1, 150).WithMessage("Idade deve ser entre 1 e 150 anos.");
    }
}
```

**Por que `IValidator<T>` injetado nos services e não um `ValidationFilter`?**
Eu inicialmente tinha考虑ado usar um Action Filter para validar antes do controller executar. Mas isso separaria a validação do fluxo de negócio — e a regra "menor não cria receita" precisa buscar a pessoa no banco para validar. Faz mais sentido ter tudo no service, onde eu tenho acesso aos repositórios.

### DI Registration

```csharp
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

O FluentValidation registra automaticamente todos os validators encontrados no assembly. Não preciso registrar cada um manualmente.

## Infrastructure

### AppDbContext

Configurei tudo via Fluent API no `OnModelCreating`:

```csharp
entity.HasOne(e => e.Person)
    .WithMany(p => p.Transactions)
    .HasForeignKey(e => e.PersonId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Por que Cascade Delete?** A regra de negócio diz: "deletar uma pessoa deve deletar suas transações." Em vez de implementar isso na mão (buscar transações, deletar uma a uma), eu deixo o banco cuidar disso. É mais eficiente e atomicamente seguro.

### Repositories

Implementações simples que traduzem as operações para EF Core:

```csharp
public async Task<Person?> GetByIdAsync(Guid id)
{
    return await _context.Persons.FindAsync(id);
}
```

Não usei Specification Pattern ou Query Objects porque para 3-4 operações simples por repositório, o custo de abstração não se justifica.

## Api

### Program.cs — Composição

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
```

Cada camada se auto-registra via métodos de extensão. O `Program.cs` apenas chama esses métodos e configura o pipeline HTTP. Essa separação permite que cada camada defina suas próprias dependências sem o Api precisar conhecer detalhes internos.

### ExceptionMiddleware

Middleware global que traduz exceções em respostas HTTP adequadas:

```csharp
var (statusCode, message) = exception switch
{
    ValidationException validationEx =>
        (HttpStatusCode.BadRequest, string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
    PersonNotFoundException =>
        (HttpStatusCode.NotFound, exception.Message),
    DomainException domainEx =>
        (HttpStatusCode.BadRequest, domainEx.Message),
    _ =>
        (HttpStatusCode.InternalServerError, "Erro interno do servidor.")
};
```

**Por que middleware e não `Try-Catch` em cada controller?**
DRY. Eu teria que repetir o mesmo try-catch em 3 controllers (People, Transactions, Reports). Com o middleware, o tratamento é centralizado e consistente. Se eu adicionar um novo controller, o tratamento de erros já funciona automaticamente.

### Controllers

Controllers finos — apenas recebem a request, delegam ao service, e retornam a resposta HTTP:

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePersonRequest request)
{
    var person = await _personService.CreateAsync(request);
    return CreatedAtAction(nameof(GetAll), new { id = person.Id }, person);
}
```

**Por que não uso `[FromBody]` com validação automática (ModelState)?**
Porque a validação é feita via FluentValidation dentro do service, não via atributos `[Required]` nos DTOs. Se eu usasse o ModelState, teria que duplicar as regras nos DTOs (atributos) e nos validators (FluentValidation). Manter uma fonte de verdade (FluentValidation) é mais limpo.

### OpenAPI (Swagger)

Usei o OpenAPI built-in do .NET 10 (`AddOpenApi` / `MapOpenApi`) em vez de Swashbuckle. Motivo: o pacote `Microsoft.OpenApi` 2.0.0 (que vem com .NET 10) é incompatível com o Swashbuckle 6.9.0. Swashbuckle depende de uma API interna que mudou no OpenApi 2.0. O endpoint fica em `/openapi/v1.json`.

### CORS

Configurei `AllowAnyOrigin/Method/Header` para desenvolvimento. Em produção, eu restringiria para o domínio do frontend.

### Connection String — Bug que eu descobri

O `appsettings.json` tinha `Data Source=app.db`, que é um caminho relativo. O problema é que `Data Source=app.db` resolve relativamente ao **working directory do processo**, não ao diretório do arquivo de configuração. Quando eu executei `dotnet run --project src/Api` do diretório `backend/`, o banco era criado em `backend/app.db`. Mas o `EnsureCreated()` e o request usavam caminhos diferentes dependendo de como o processo era iniciado.

**Solução:** Passei o `ContentRootPath` para o Infrastructure, que monta um caminho absoluto:

```csharp
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

// Na Infrastructure:
var dbPath = Path.Combine(contentRoot, "app.db");
var connectionString = $"Data Source={dbPath}";
```

Isso garante que o banco sempre seja criado e acessado no mesmo local, independente de onde o processo seja executado.
