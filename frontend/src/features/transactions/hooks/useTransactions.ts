import { useState, useEffect } from 'react';
import { getTransactions, createTransaction } from '../../../api/transactions';
import type { Transaction, CreateTransactionRequest } from '../../../types/transaction';
import { getPeople } from '../../../api/people';
import type { Person } from '../../../types/person';

export const useTransactions = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [people, setPeople] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [transData, peopleData] = await Promise.all([
        getTransactions(),
        getPeople()
      ]);
      setTransactions(transData);
      setPeople(peopleData);
      setError(null);
    } catch {
      setError('Erro ao carregar dados');
    } finally {
      setLoading(false);
    }
  };

  const addTransaction = async (request: CreateTransactionRequest) => {
    const transaction = await createTransaction(request);
    await fetchData();
    return transaction;
  };

  useEffect(() => {
    fetchData();
  }, []);

  return { transactions, people, loading, error, addTransaction, refetch: fetchData };
};
