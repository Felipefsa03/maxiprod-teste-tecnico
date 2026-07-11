import React from 'react';
import { Card } from '../../../components/Card';
import { useReports } from '../hooks/useReports';

export const ReportsPage: React.FC = () => {
  const { totals, loading, error } = useReports();

  if (loading) return <p>Carregando...</p>;
  if (error) return <p style={{ color: '#ef4444' }}>{error}</p>;
  if (!totals) return null;

  return (
    <div>
      <Card title="Resumo Financeiro">
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px' }}>
          <div style={{
            padding: '20px',
            borderRadius: '8px',
            backgroundColor: '#f0fdf4',
            border: '1px solid #bbf7d0'
          }}>
            <p style={{ margin: 0, fontSize: '14px', color: '#16a34a', fontWeight: 500 }}>Total Receitas</p>
            <p style={{ margin: '4px 0 0', fontSize: '28px', fontWeight: 700, color: '#16a34a' }}>
              R$ {totals.totalReceitas.toFixed(2)}
            </p>
          </div>

          <div style={{
            padding: '20px',
            borderRadius: '8px',
            backgroundColor: '#fef2f2',
            border: '1px solid #fecaca'
          }}>
            <p style={{ margin: 0, fontSize: '14px', color: '#ef4444', fontWeight: 500 }}>Total Despesas</p>
            <p style={{ margin: '4px 0 0', fontSize: '28px', fontWeight: 700, color: '#ef4444' }}>
              R$ {totals.totalDespesas.toFixed(2)}
            </p>
          </div>

          <div style={{
            padding: '20px',
            borderRadius: '8px',
            backgroundColor: totals.saldo >= 0 ? '#eff6ff' : '#fef2f2',
            border: `1px solid ${totals.saldo >= 0 ? '#bfdbfe' : '#fecaca'}`
          }}>
            <p style={{
              margin: 0, fontSize: '14px',
              color: totals.saldo >= 0 ? '#2563eb' : '#ef4444',
              fontWeight: 500
            }}>
              Saldo
            </p>
            <p style={{
              margin: '4px 0 0', fontSize: '28px', fontWeight: 700,
              color: totals.saldo >= 0 ? '#2563eb' : '#ef4444'
            }}>
              R$ {totals.saldo.toFixed(2)}
            </p>
          </div>
        </div>
      </Card>
    </div>
  );
};
