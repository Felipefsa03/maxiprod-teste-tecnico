import { useState, useEffect } from 'react';
import { getPeople, createPerson, deletePerson } from '../../../api/people';
import type { Person, CreatePersonRequest } from '../../../types/person';

export const usePeople = () => {
  const [people, setPeople] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPeople = async () => {
    try {
      setLoading(true);
      const data = await getPeople();
      setPeople(data);
      setError(null);
    } catch {
      setError('Erro ao carregar pessoas');
    } finally {
      setLoading(false);
    }
  };

  const addPerson = async (request: CreatePersonRequest) => {
    const person = await createPerson(request);
    setPeople(prev => [...prev, person]);
    return person;
  };

  const removePerson = async (id: string) => {
    await deletePerson(id);
    setPeople(prev => prev.filter(p => p.id !== id));
  };

  useEffect(() => {
    fetchPeople();
  }, []);

  return { people, loading, error, addPerson, removePerson, refetch: fetchPeople };
};
