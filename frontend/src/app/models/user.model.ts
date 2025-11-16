export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
  fullName: string;
}

export interface User {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
}