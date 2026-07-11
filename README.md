<div align="center">

# 💰 Controle de Gastos

Sistema de gerenciamento de pessoas e transações financeiras desenvolvido como teste técnico.

## 🎥 Demonstração

[![YouTube](https://img.shields.io/badge/▶%20Assistir%20Demonstração-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/hhh165PzVO0)

## 🚀 Tecnologias

<p>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-68217A?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white" />
  <img src="https://img.shields.io/badge/FluentValidation-2E8B57?style=for-the-badge" />
  <img src="https://img.shields.io/badge/xUnit-5C2D91?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Moq-4B8BBE?style=for-the-badge" />
  <img src="https://img.shields.io/badge/FluentAssertions-009688?style=for-the-badge" />
</p>

<p>
  <img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=black" />
  <img src="https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white" />
  <img src="https://img.shields.io/badge/Vite-646CFF?style=for-the-badge&logo=vite&logoColor=white" />
  <img src="https://img.shields.io/badge/Axios-5A29E4?style=for-the-badge&logo=axios&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/OpenAPI-6BA539?style=for-the-badge&logo=swagger&logoColor=white" />
</p>

</div>

# Controle de Gastos

Sistema de gerenciamento de pessoas e transações financeiras (receitas/despesas).

## Tecnologias

### Backend
- .NET 10 Web API
- Entity Framework Core + SQLite
- FluentValidation
- Arquitetura em Camadas (Domain, Application, Infrastructure, Api)
- xUnit + Moq + FluentAssertions (testes)
- OpenAPI (documentação)

### Frontend
- React 19 + TypeScript
- Vite
- React Router DOM
- Axios
- Arquitetura por Features

## Estrutura do Projeto

```
controle-gastos/
├── backend/
│   ├── src/
│   │   ├── Api/              # Controllers, Middleware, Program.cs
│   │   ├── Application/      # Services, DTOs, Validators, Interfaces
│   │   ├── Domain/           # Entities, Enums, Interfaces, Exceptions
│   │   └── Infrastructure/   # DbContext, Repositories, DI
│   ├── tests/
│   │   ├── UnitTests/        # Testes unitários (services, validators, controllers)
│   │   └── IntegrationTests/ # Testes de integração (API completa)
│   └── controle-gastos.slnx
│
├── frontend/
│   ├── src/
│   │   ├── api/              # Camada de comunicação com a API
│   │   ├── components/       # Componentes reutilizáveis (Button, Card, Input)
│   │   ├── features/         # Organização por feature
│   │   │   ├── people/       # Pessoas (hooks, pages)
│   │   │   ├── transactions/ # Transações (hooks, pages)
│   │   │   └── reports/      # Relatórios (hooks, pages)
│   │   ├── layouts/          # Layout principal com sidebar
│   │   └── types/            # Tipos TypeScript
│   └── package.json
│
├── docker-compose.yml
├── .gitignore
└── README.md
```

## Como Executar

### Pré-requisitos
- .NET 10 SDK
- Node.js 18+

### Backend

```bash
cd backend

# Restaurar pacotes
dotnet restore

# Criar o banco de dados (SQLite)
dotnet run --project src/Api

# A API estará disponível em: http://localhost:5000
# OpenAPI: http://localhost:5000/openapi/v1.json
```

### Frontend

```bash
cd frontend

# Instalar dependências
npm install

# Iniciar em modo de desenvolvimento
npm run dev

# O frontend estará disponível em: http://localhost:5173
```

### Testes

```bash
cd backend

# Rodar todos os testes
dotnet test

# Rodar apenas unit tests
dotnet test tests/UnitTests

# Rodar apenas integration tests
dotnet test tests/IntegrationTests
```

### Docker

```bash
docker-compose up --build
```

## Endpoints da API

| Método | Rota                    | Descrição                        |
|--------|-------------------------|----------------------------------|
| GET    | /api/people             | Listar todas as pessoas          |
| POST   | /api/people             | Cadastrar pessoa                 |
| DELETE | /api/people/{id}        | Excluir pessoa e suas transações |
| GET    | /api/transactions       | Listar todas as transações       |
| POST   | /api/transactions       | Cadastrar transação              |
| GET    | /api/reports/totals     | Obter totais (receitas, despesas, saldo) |

## Regras de Negócio

1. **Menores de 18 anos** somente podem cadastrar **despesas** (não receitas)
2. Ao **excluir uma pessoa**, todas as suas transações são excluídas automaticamente (cascade delete)
3. Validações via FluentValidation:
   - Pessoa: nome obrigatório (max 100), idade entre 1 e 150
   - Transação: descrição obrigatória (max 200), valor maior que zero, PersonId obrigatório

## Decisões de Arquitetura

### Backend - Arquitetura em Camadas
- **Domain**: Entidades, enums, interfaces de repositório e exceções de domínio. Sem dependências externas.
- **Application**: Services (lógica de negócio), DTOs, validações (FluentValidation) e interfaces de serviço.
- **Infrastructure**: Implementação dos repositórios com EF Core + SQLite.
- **Api**: Controllers finos, middleware de tratamento global de exceções, configuração de DI.

### Frontend - Arquitetura por Features
- Cada feature (people, transactions, reports) contém seus hooks, páginas e tipos.
- Componentes reutilizáveis (Button, Card, Input) separados da lógica de negócio.
- Camada `api/` centraliza toda comunicação HTTP via Axios.

## Funcionalidades

### Pessoas
- Listar pessoas cadastradas
- Cadastrar nova pessoa (nome + idade)
- Excluir pessoa (com confirmação e cascade delete)

### Transações
- Listar transações com detalhes (pessoa, tipo, valor, data)
- Cadastrar transação (descrição, valor, tipo receita/despesa, pessoa)
- Validação visual para menores de 18 anos

### Relatórios
- Dashboard com totais: receitas, despesas e saldo
- Cards coloridos com formatação monetária
