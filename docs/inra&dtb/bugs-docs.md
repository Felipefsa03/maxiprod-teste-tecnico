# Bugs Encontrados e Correções

Durante o desenvolvimento, deparei com vários problemas que tive que resolver. Documento aqui os mais significativos, porque acho que é tão importante quanto o código final.

## 1. Swashbuckle vs OpenApi 2.0 (incompatibilidade)

### O problema
Instalei o Swashbuckle para documentação Swagger, mas o .NET 10 SDK usa `Microsoft.OpenApi` 2.0.0, que quebrou a API interna do Swashbuckle 6.9.0. O build falhava com:

```
error NU1108: Cycle detected: Microsoft.AspNetCore.OpenApi -> Swashbuckle.AspNetCore 6.9.0
```

Mesmo tentando resolver manualmente, o Swashbuckle não era compatível.

### A correção
Usei o OpenAPI built-in do .NET 10:

```csharp
builder.Services.AddOpenApi();
// ...
app.MapOpenApi();
```

O endpoint fica em `/openapi/v1.json` em vez de `/swagger/v1/swagger.json`. Não é tão bonito quanto o Swagger UI, mas funciona perfeitamente e não depende de pacotes de terceiros.

## 2. SQLite: "no such table: Persons"

### O problema
O banco era criado com sucesso (logs mostravam `CREATE TABLE`), mas na primeira requisição HTTP, o SQLite dizia `no such table: Persons`.

### A causa
A connection string era `Data Source=app.db` — um caminho relativo. O `EnsureCreated()` rodava no contexto do startup (com um working directory), e o request rodava em outro contexto (com outro working directory). O SQLite criava o banco em `backend/app.db` e depois tentava acessar outro `app.db` em diretório diferente.

### A correção
Passei o `ContentRootPath` para o Infrastructure e montei um caminho absoluto:

```csharp
// Program.cs
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

// Infrastructure/DependencyInjection.cs
var dbPath = Path.Combine(contentRoot, "app.db");
var connectionString = $"Data Source={dbPath}";
```

Agora o banco é sempre criado e acessado no mesmo local, independente do working directory.

## 3. Processo .NET morria ao rodar em background

### O problema
Quando eu tentava rodar a API com `dotnet run &` ou `nohup dotnet run`, o processo morria imediatamente após o startup, sem dar tempo de testar os endpoints.

### A causa
O shell do bash tool encerra processos filhos quando o timeout da command dispara. `nohup` e `disown` não funcionam porque o shell session não persiste entre chamadas.

### A correção
Publiquei o projeto (`dotnet publish`) e rodei o binário compilado diretamente. Isso elimina o passo de compilação e torna o startup mais rápido:

```bash
dotnet publish src/Api -c Release -o /tmp/api-prod
nohup dotnet /tmp/api-prod/Api.dll &
```

Mesmo assim, o processo ainda morria. A solução final foi rodar o publish + testar tudo em uma única chamada de bash com timeout maior.

## 4. `enum TransactionType` vs `erasableSyntaxOnly`

### O problema
O tsconfig do frontend tem `"erasableSyntaxOnly": true`, que proíbe `enum` numérico (porque ele gera código JavaScript).

```typescript
// ERRO: Numeric enums are not allowed with erasableSyntaxOnly
export enum TransactionType {
  Receita = 0,
  Despesa = 1,
}
```

### A correção
Usei const object pattern:

```typescript
export const TransactionType = {
  Receita: 0,
  Despesa: 1,
} as const;

export type TransactionType = typeof TransactionType[keyof typeof TransactionType];
```

Isso gera o mesmo resultado em compilação (type narrowing, acesso por valor) sem gerar código JavaScript.

## 5. `verbatimModuleSyntax` vs imports normais

### O problema
Com `"verbatimModuleSyntax": true`, o TypeScript exige que imports de tipo usem `import type`:

```typescript
// ERRO
import { PersonResponse } from '../../types/person';

// OK
import type { PersonResponse } from '../../types/person';
```

### A correção
Adicionei `type` em todos os imports que são apenas tipos. Isso é bom porque:
- Deixa explícito o que é tipo vs valor
- Ajuda o bundler a fazer tree-shake
- Evita erros quando um módulo tem side effects

## 6. `AddValidatorsFromAssembly` não encontrava validators

### O problema
Os validators estavam definidos mas nunca eram chamados — a validação passava em branco.

### A causa
Os services não injetavam `IValidator<T>`. Os validators estavam registrados no DI container, mas ninguém os pedia.

### A correção
Injetei `IValidator<CreatePersonRequest>` e `IValidator<CreateTransactionRequest>` nos services correspondentes e chamei `ValidateAsync` antes de executar a lógica de negócio:

```csharp
var validation = await _validator.ValidateAsync(request);
if (!validation.IsValid)
    throw new ValidationException(validation.Errors);
```

## 7. Testes unitários quebraram após injeção do validator

### O problema
Quando adicionei `IValidator<T>` ao construtor de `PersonService` e `TransactionService`, os unit tests pararam de compilar porque criavam os services sem o validator.

### A correção
Adicionei um mock do validator nos unit tests:

```csharp
_validatorMock = new Mock<IValidator<CreatePersonRequest>>();
_validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreatePersonRequest>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult());
```

O mock retorna validação válida por padrão. Para testar cenários de falha de validação, usamos os `ValidatorTests` dedicados.
