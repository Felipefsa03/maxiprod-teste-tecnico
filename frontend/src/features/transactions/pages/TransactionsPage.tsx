import React, { useState } from 'react';
import { Card } from '../../../components/Card';
import { Input } from '../../../components/Input';
import { Button } from '../../../components/Button';
import { useTransactions } from '../hooks/useTransactions';
import { TransactionType } from '../../../types/transaction';

export const TransactionsPage: React.FC = () => {
  const { transactions, people, loading, error, addTransaction } = useTransactions();
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [type, setType] = useState<TransactionType>(TransactionType.Despesa);
  const [personId, setPersonId] = useState('');
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const selectedPerson = people.find((p) => p.id === personId);
  const isMinor = selectedPerson ? selectedPerson.age < 18 : false;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (!description.trim()) {
      setFormError('Descrição é obrigatória');
      return;
    }
    const amountNum = parseFloat(amount);
    if (isNaN(amountNum) || amountNum <= 0) {
      setFormError('Valor deve ser maior que zero');
      return;
    }
    if (!personId) {
      setFormError('Selecione uma pessoa');
      return;
    }
    if (isMinor && type === TransactionType.Receita) {
      setFormError('Pessoas menores de 18 anos não podem cadastrar receitas');
      return;
    }

    setSubmitting(true);
    try {
      await addTransaction({ description: description.trim(), amount: amountNum, type, personId });
      setDescription('');
      setAmount('');
      setPersonId('');
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setFormError(axiosErr?.response?.data?.error || 'Erro ao cadastrar transação');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <Card title="Cadastrar Transação">
        <form onSubmit={handleSubmit}>
          <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'flex-end' }}>
            <Input
              label="Descrição"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Ex: Salário, Aluguel..."
            />
            <Input
              label="Valor"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00"
              min={0.01}
              step={0.01}
            />
            <div style={{ display: 'flex', flexDirection: 'column', marginBottom: '12px' }}>
              <label style={{ fontSize: '14px', fontWeight: 500, marginBottom: '4px', color: '#374151' }}>Tipo</label>
              <select
                value={type}
                onChange={(e) => setType(Number(e.target.value) as TransactionType)}
                style={{ padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontSize: '14px' }}
              >
                <option value={TransactionType.Receita}>Receita</option>
                <option value={TransactionType.Despesa}>Despesa</option>
              </select>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', marginBottom: '12px' }}>
              <label style={{ fontSize: '14px', fontWeight: 500, marginBottom: '4px', color: '#374151' }}>Pessoa</label>
              <select
                value={personId}
                onChange={(e) => setPersonId(e.target.value)}
                style={{ padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontSize: '14px' }}
              >
                <option value="">Selecione...</option>
                {people.map((p) => (
                  <option key={p.id} value={p.id}>{p.name} ({p.age} anos)</option>
                ))}
              </select>
            </div>
            <Button type="submit" disabled={submitting}>
              {submitting ? 'Cadastrando...' : 'Cadastrar'}
            </Button>
          </div>
          {isMinor && (
            <p style={{ color: '#f59e0b', marginTop: '8px', fontSize: '13px' }}>
              Pessoa menor de 18 anos - somente despesas permitidas
            </p>
          )}
          {formError && <p style={{ color: '#ef4444', marginTop: '8px' }}>{formError}</p>}
        </form>
      </Card>

      <Card title="Transações">
        {loading && <p>Carregando...</p>}
        {error && <p style={{ color: '#ef4444' }}>{error}</p>}
        {!loading && transactions.length === 0 && <p>Nenhuma transação cadastrada.</p>}
        {!loading && transactions.length > 0 && (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Descrição</th>
                <th style={{ textAlign: 'right', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Valor</th>
                <th style={{ textAlign: 'center', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Tipo</th>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Pessoa</th>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Data</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((t) => (
                <tr key={t.id}>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>{t.description}</td>
                  <td style={{
                    padding: '8px', borderBottom: '1px solid #e5e7eb', textAlign: 'right',
                    color: t.type === TransactionType.Receita ? '#16a34a' : '#ef4444'
                  }}>
                    R$ {t.amount.toFixed(2)}
                  </td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb', textAlign: 'center' }}>
                    <span style={{
                      padding: '2px 8px',
                      borderRadius: '4px',
                      fontSize: '12px',
                      fontWeight: 500,
                      backgroundColor: t.type === TransactionType.Receita ? '#dcfce7' : '#fee2e2',
                      color: t.type === TransactionType.Receita ? '#16a34a' : '#ef4444'
                    }}>
                      {t.type === TransactionType.Receita ? 'Receita' : 'Despesa'}
                    </span>
                  </td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>{t.personName}</td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>
                    {new Date(t.createdAt).toLocaleDateString('pt-BR')}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Card>
    </div>
  );
};
