import api from './axios';
import type { Transaction, CreateTransactionRequest } from '../types/transaction';

export const getTransactions = async (): Promise<Transaction[]> => {
  const response = await api.get<Transaction[]>('/transactions');
  return response.data;
};

export const createTransaction = async (data: CreateTransactionRequest): Promise<Transaction> => {
  const response = await api.post<Transaction>('/transactions', data);
  return response.data;
};
