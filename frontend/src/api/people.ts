import api from './axios';
import type { Person, CreatePersonRequest } from '../types/person';

export const getPeople = async (): Promise<Person[]> => {
  const response = await api.get<Person[]>('/people');
  return response.data;
};

export const createPerson = async (data: CreatePersonRequest): Promise<Person> => {
  const response = await api.post<Person>('/people', data);
  return response.data;
};

export const deletePerson = async (id: string): Promise<void> => {
  await api.delete(`/people/${id}`);
};
