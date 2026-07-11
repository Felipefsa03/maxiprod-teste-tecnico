# API Reference

Base URL: `http://localhost:5000/api`

## Pessoas

### Listar pessoas
```
GET /api/people
```

**Response 200:**
```json
[
  {
    "id": "c2890047-48c8-430d-b887-89d8bc1db19c",
    "name": "Alice",
    "age": 30,
    "createdAt": "2026-07-10T22:55:44.4070051Z"
  }
]
```

### Criar pessoa
```
POST /api/people
Content-Type: application/json

{
  "name": "Alice",
  "age": 30
}
```

**Regras de validação:**
- `name`: obrigatório, máx. 100 caracteres
- `age`: obrigatório, entre 1 e 150

**Response 201:**
```json
{
  "id": "c2890047-48c8-430d-b887-89d8bc1db19c",
  "name": "Alice",
  "age": 30,
  "createdAt": "2026-07-10T22:55:44.4070051Z"
}
```

**Response 400 (validação):**
```json
{
  "error": "Nome é obrigatório."
}
```

### Deletar pessoa
```
DELETE /api/people/{id}
```

**Response 204:** Pessoa e todas suas transações deletadas (cascade delete).

**Response 404:**
```json
{
  "error": "Pessoa com id {id} não encontrada."
}
```

## Transações

### Listar transações
```
GET /api/transactions
```

**Response 200:**
```json
[
  {
    "id": "4683bc50-2e1b-4746-8efd-639dc7788832",
    "description": "Salário",
    "amount": 5000,
    "type": 0,
    "personId": "c2890047-48c8-430d-b887-89d8bc1db19c",
    "personName": "Alice",
    "createdAt": "2026-07-10T22:55:50.2204943Z"
  }
]
```

> `type`: 0 = Receita, 1 = Despesa

### Criar transação
```
POST /api/transactions
Content-Type: application/json

{
  "description": "Salário",
  "amount": 5000,
  "type": 0,
  "personId": "c2890047-48c8-430d-b887-89d8bc1db19c"
}
```

**Regras de validação:**
- `description`: obrigatório, máx. 200 caracteres
- `amount`: obrigatório, maior que 0
- `personId`: obrigatório

**Regra de negócio:**
- Pessoas menores de 18 anos **não podem** criar transações do tipo Receita (type 0)

**Response 201:**
```json
{
  "id": "4683bc50-2e1b-4746-8efd-639dc7788832",
  "description": "Salário",
  "amount": 5000,
  "type": 0,
  "personId": "c2890047-48c8-430d-b887-89d8bc1db19c",
  "personName": "Alice",
  "createdAt": "2026-07-10T22:55:50.2204943Z"
}
```

**Response 400 (menor + receita):**
```json
{
  "error": "Pessoas menores de 18 anos não podem cadastrar receitas."
}
```

**Response 400 (validação):**
```json
{
  "error": "Valor deve ser maior que zero."
}
```

**Response 404 (pessoa não encontrada):**
```json
{
  "error": "Pessoa com id {id} não encontrada."
}
```

## Relatórios

### Totais financeiros
```
GET /api/reports/totals
```

**Response 200:**
```json
{
  "totalReceitas": 5000,
  "totalDespesas": 1525,
  "saldo": 3475
}
```

> `saldo` = `totalReceitas` - `totalDespesas`

## OpenAPI

```
GET /openapi/v1.json
```

Documentação OpenAPI 3.0 do schema completo da API. Disponível apenas em modo Development.
