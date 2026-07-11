import api from './axios';
import type { TotalsResponse } from '../types/transaction';

export const getTotals = async (): Promise<TotalsResponse> => {
  const response = await api.get<TotalsResponse>('/reports/totals');
  return response.data;
};
