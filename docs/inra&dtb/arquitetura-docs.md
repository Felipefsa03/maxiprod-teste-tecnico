# Arquitetura Geral

## Por que eu escolhi esta arquitetura

Quando comecei este projeto, eu poderia ter ido de várias formas: um monolito simples sem separação nenhuma, uma Clean Architecture completa com 7+ camadas, ou algo no meio. Escolhi o meio, e aqui explico por quê.

## A decisão: Layered Architecture (não Clean Architecture)

Eu optei por uma **Layered Architecture** com 4 projetos (Domain, Application, Infrastructure, Api) em vez de uma Clean Architecture clássica com UseCases, Interfaces Adapters, e Frameworks & Drivers separados.

**Minha motivação:** Este é um teste técnico com um escopo relativamente simples — CRUD de pessoas e transações com uma regra de negócio pontual (menores não podem criar receita). Implementar uma Clean Architecture completa com abstrações como `IUnitOfWork`, `IRepository<T>` genérico com specs, `IMapper` genérico, `INotificationHandler`, etc., seria overengineering que não agrega valor ao problema proposto.

Eu queria demonstrar que **eu sei** separar responsabilidades e manter o domínio isolado, mas sem criar abstrações que ninguém vai usar.

## As 4 camadas e o porquê de cada uma

```
┌──────────────────────────────────────────────┐
│                   Api                        │  ← Controller, Middleware, DI composition
├──────────────────────────────────────────────┤
│               Application                    │  ← Services, DTOs, Validators, Interfaces
├──────────────────────────────────────────────┤
│              Infrastructure                  │  ← EF Core, Repositories, DbContext
├──────────────────────────────────────────────┤
│                Domain                        │  ← Entities, Enums, Exceptions, Repository Interfaces
└──────────────────────────────────────────────┘
```

### Domain (sem dependências externas)
Contém apenas entidades, enums, exceções e interfaces de repositório. Não referencia nenhum pacote NuGet — apenas `System.*`. Isso garante que o **coração do sistema** não seja afetado por mudanças de infraestrutura (trocar EF Core por Dapper, trocar SQLite por Postgres, etc.).

### Application (depende de Domain)
Onde mora a lógica de orquestração: services que orquestram repositórios e aplicam regras de negócio. DTOs para transferência de dados, FluentValidation para validação de input, e interfaces de serviço que o Api vai consumir.

Escolhi FluentValidation em vez de atributos `[Required]` nos DTOs porque:
1. Regras de negócio complexas (como validar idade, tipo de transação) são mais claras em uma classe dedicada
2. Fica mais fácil testar as regras de validação isoladamente
3. Não polui os DTOs com lógica

### Infrastructure (depende de Domain, Application)
A camada "suja" — onde eu lido com EF Core, SQLite, e implementações concretas dos repositórios. Ela referencia o Application para ter acesso às interfaces que precisa implementar, e o Domain para acessar as entidades.

### Api (depende de tudo)
O ponto de composição. Aqui eu registro todos os serviços no DI container, configuro middleware, e exponho os endpoints REST. Não contém lógica de negócio — apenas adapta HTTP para chamadas de serviço.

## Por que não adotei MediatR / CQRS?

Para este escopo, injetar serviços diretamente nos controllers é mais simples e direto. MediatR faria sentido se eu tivesse muitas operações com cross-cutting concerns (logging, caching, transaction management) que se repetem. Aqui, cada controller tem 2-3 endpoints — MediatR seria uma camada indireta desnecessária.

## Por que SQLite?

Porque o objetivo é **rodar sem setup externo**. SQLite funciona out-of-the-box, os testes de integração usam `:memory:`, e não preciso configurar Docker para um banco. Em produção, eu trocaria por PostgreSQL sem mudar nada no código (EF Core abstrai isso).

## Por que EnsureCreated() em vez de Migrations?

Para um teste técnico, `EnsureCreated()` é suficiente e mais simples. Migrations fazem sentido quando você tem dados em produção e precisa de controle sobre o esquema. Para este contexto, eu quero que o banco seja criado do zero a cada execução.
