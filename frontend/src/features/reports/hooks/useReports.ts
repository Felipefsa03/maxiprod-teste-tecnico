import { useState, useEffect } from 'react';
import { getTotals } from '../../../api/reports';
import type { TotalsResponse } from '../../../types/transaction';

export const useReports = () => {
  const [totals, setTotals] = useState<TotalsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTotals = async () => {
    try {
      setLoading(true);
      const data = await getTotals();
      setTotals(data);
      setError(null);
    } catch {
      setError('Erro ao carregar relatório');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTotals();
  }, []);

  return { totals, loading, error, refetch: fetchTotals };
};
