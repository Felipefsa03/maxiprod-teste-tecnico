export interface Person {
  id: string;
  name: string;
  age: number;
  createdAt: string;
}

export interface CreatePersonRequest {
  name: string;
  age: number;
}
