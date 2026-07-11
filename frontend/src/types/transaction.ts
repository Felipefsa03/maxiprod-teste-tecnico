export const TransactionType = {
  Receita: 0,
  Despesa: 1,
} as const;

export type TransactionType = typeof TransactionType[keyof typeof TransactionType];

export interface Transaction {
  id: string;
  description: string;
  amount: number;
  type: TransactionType;
  personId: string;
  personName: string;
  createdAt: string;
}

export interface CreateTransactionRequest {
  description: string;
  amount: number;
  type: TransactionType;
  personId: string;
}

export interface TotalsResponse {
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}
