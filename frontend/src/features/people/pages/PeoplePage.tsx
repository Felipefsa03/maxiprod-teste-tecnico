import React, { useState } from 'react';
import { Card } from '../../../components/Card';
import { Input } from '../../../components/Input';
import { Button } from '../../../components/Button';
import { usePeople } from '../hooks/usePeople';

export const PeoplePage: React.FC = () => {
  const { people, loading, error, addPerson, removePerson } = usePeople();
  const [name, setName] = useState('');
  const [age, setAge] = useState('');
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (!name.trim()) {
      setFormError('Nome é obrigatório');
      return;
    }

    const ageNum = parseInt(age, 10);
    if (isNaN(ageNum) || ageNum < 1 || ageNum > 150) {
      setFormError('Idade deve ser entre 1 e 150');
      return;
    }

    setSubmitting(true);
    try {
      await addPerson({ name: name.trim(), age: ageNum });
      setName('');
      setAge('');
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setFormError(axiosErr?.response?.data?.error || 'Erro ao cadastrar pessoa');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Tem certeza que deseja excluir esta pessoa? Todas as transações também serão excluídas.')) return;
    try {
      await removePerson(id);
    } catch {
      alert('Erro ao excluir pessoa');
    }
  };

  return (
    <div>
      <Card title="Cadastrar Pessoa">
        <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '12px', alignItems: 'flex-end', flexWrap: 'wrap' }}>
          <Input
            label="Nome"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="Nome da pessoa"
          />
          <Input
            label="Idade"
            type="number"
            value={age}
            onChange={e => setAge(e.target.value)}
            placeholder="Idade"
            min={1}
            max={150}
          />
          <Button type="submit" disabled={submitting}>
            {submitting ? 'Cadastrando...' : 'Cadastrar'}
          </Button>
        </form>
        {formError && <p style={{ color: '#ef4444', marginTop: '8px' }}>{formError}</p>}
      </Card>

      <Card title="Pessoas Cadastradas">
        {loading && <p>Carregando...</p>}
        {error && <p style={{ color: '#ef4444' }}>{error}</p>}
        {!loading && people.length === 0 && <p>Nenhuma pessoa cadastrada.</p>}
        {!loading && people.length > 0 && (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Nome</th>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Idade</th>
                <th style={{ textAlign: 'left', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Criado em</th>
                <th style={{ textAlign: 'right', padding: '8px', borderBottom: '2px solid #e5e7eb' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {people.map((person) => (
                <tr key={person.id}>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>{person.name}</td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>{person.age}</td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb' }}>
                    {new Date(person.createdAt).toLocaleDateString('pt-BR')}
                  </td>
                  <td style={{ padding: '8px', borderBottom: '1px solid #e5e7eb', textAlign: 'right' }}>
                    <Button variant="danger" onClick={() => handleDelete(person.id)}>
                      Excluir
                    </Button>
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
