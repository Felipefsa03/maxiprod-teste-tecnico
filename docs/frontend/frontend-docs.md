# Frontend — Estrutura e Decisões

## Stack

- **React 19** com TypeScript
- **Vite** como bundler
- **Axios** para chamadas HTTP
- **React Router** para navegação
- **Sem UI library** (CSS inline + componentes manuais)

## Por que React 19 + Vite?

React 19 porque é a versão atual e traz melhorias de performance (Server Components, otimizações de reconciliação). Vite porque é o padrão da indústria para projetos React — build rápido, HMR instantâneo, e configuração mínima.

## Por que não usei uma UI library (MUI, Chakra, shadcn)?

O desafio técnico foca em **arquitetura e lógica**, não em visual. Eu poderia ter usado shadcn/ui ou MUI, mas:
1. Componentes manuais mostram que eu sei trabalhar com React sem dependências pesadas
2. Não preciso me preocupar com a API de uma lib externa
3. O CSS inline é suficiente para um CRUD funcional e legível

Se o desafio fosse um produto real com design system, eu usaria shadcn/ui — ele é headless e给了我 total controle sobre estilização.

## Organização por Features

```
frontend/src/
├── api/              # Camada de HTTP (axios + endpoints)
├── components/       # Componentes reutilizáveis (Button, Card, Input)
├── features/
│   ├── people/       # hooks + pages de pessoas
│   ├── transactions/ # hooks + pages de transações
│   └── reports/      # hooks + pages de relatórios
├── layouts/          # Layout com sidebar
├── types/            # Interfaces TypeScript
├── App.tsx           # Router
└── main.tsx          # Entry point
```

**Por que organizar por feature e não por tipo (components/, pages/, hooks/)?**

Quando eu organizo por tipo, para mudar algo de "pessoas" eu tenho que ir em `pages/PeoplePage.tsx`, `hooks/usePeople.ts`, e `api/people.ts` — tudo em diretórios diferentes. Organizando por feature, tudo relacionado a pessoas está em `features/people/`. É mais fácil de encontrar e de deletar se a feature for removida.

## Camada API

```typescript
// api/axios.ts
const api = axios.create({
  baseURL: 'http://localhost:5000/api',
});
```

Centralizo o axios em um único arquivo. Cada feature tem seu arquivo de chamadas:

```typescript
// api/people.ts
export const peopleApi = {
  getAll: () => api.get<PersonResponse[]>('/people'),
  create: (data: CreatePersonRequest) => api.post<PersonResponse>('/people', data),
  delete: (id: string) => api.delete(`/people/${id}`),
};
```

**Por que não usar TanStack Query?**
Para 3 endpoints com operações CRUD simples, um hook custom com `useState` + `useEffect` é suficiente. TanStack Query agregaria valor se eu tivesse cache, polling, optimistic updates, ou mutations complexas. Para este escopo, seria uma dependência desnecessária.

## Hooks customizados

Cada feature tem um hook que encapsula estado + chamadas API:

```typescript
// features/people/hooks/usePeople.ts
export function usePeople() {
  const [people, setPeople] = useState<PersonResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchPeople = async () => { /* ... */ };
  const addPerson = async (data: CreatePersonRequest) => { /* ... */ };
  const removePerson = async (id: string) => { /* ... */ };

  useEffect(() => { fetchPeople(); }, []);

  return { people, loading, error, addPerson, removePerson, refresh: fetchPeople };
}
```

O hook expõe `refresh` para que a página possa recarregar depois de uma operação. O loading e error são gerenciados internamente — o componente não precisa se preocupar com isso.

## TransactionType como const object

```typescript
export const TransactionType = {
  Receita: 0,
  Despesa: 1,
} as const;

export type TransactionType = typeof TransactionType[keyof typeof TransactionType];
```

**Por que não usei `enum`?**
O tsconfig tem `"erasableSyntaxOnly": true` (uma configuração nova do TypeScript 5.8+) que proíbe `enum` numérico porque ele gera código JavaScript no output — algo que não deveria acontecer em um projeto com `"noEmit": true`. O `const object` + `type` derivation é a alternativa idiomática: funciona como enum em tempo de compilação, mas erasa completamente no output.

## `verbatimModuleSyntax`

Essa flag do tsconfig exige que imports de tipo usem `import type`:

```typescript
import type { PersonResponse } from '../../types/person';
```

Em vez de:

```typescript
import { PersonResponse } from '../../types/person';
```

Isso força consistência: se algo é apenas um tipo, o import deve refletir isso. Ajuda o bundler a tree-shake melhor e evita erros sutis quando um módulo tem side effects.

## Validação no Frontend

O frontend valida antes de enviar para o backend:

```typescript
if (!name.trim()) {
  setFormError('Nome é obrigatório');
  return;
}
```

Isso é uma **duplicação intencional** da validação do backend. Eu mantenho as mesmas regras nos dois lados porque:
1. O usuário recebe feedback imediato sem esperar a resposta do servidor
2. Se o backend cair, o frontend ainda rejeita input inválido
3. O backend é a fonte de verdade — se o frontend deixar passar algo, o backend rejeita

## Aviso visual para menores

Na página de transações, quando o usuário seleciona uma pessoa menor de 18 anos, eu exibo um aviso amarelo: "Pessoa menor de 18 anos - somente despesas permitidas". O select de tipo também continua disponível, mas se o usuário escolher "Receita", a validação bloqueia no submit. Isso é mais UX-friendly do que esconder a opção.

## CSS inline

Toda estilização é feita via `style` props. Para um projeto deste tamanho, isso é mais rápido e mais fácil de manter do que criar arquivos CSS separados ou usar uma lib como Tailwind. Se o projeto crescesse, eu migraria para Tailwind CSS ou CSS Modules.

## Navegação (Layout com Sidebar)

```tsx
<Layout>
  <Routes>
    <Route path="/" element={<PeoplePage />} />
    <Route path="/transactions" element={<TransactionsPage />} />
    <Route path="/reports" element={<ReportsPage />} />
  </Routes>
</Layout>
```

O `Layout` component tem uma sidebar com links de navegação. Cada feature é uma rota separada. Não usei lazy loading (`React.lazy`) porque o bundle é pequeno — não há benefício perceptível de carregamento.
