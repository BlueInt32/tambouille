import axios from 'axios'
import type { CreateMealRequest, Meal, UpdateMealRequest } from '@/types'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8888',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const authApi = {
  login: (password: string) =>
    api.post<{ token: string }>('/api/auth/login', { password }),
}

export const mealsApi = {
  getWeek: (weekStart: string) =>
    api.get<Meal[]>('/api/meals', { params: { weekStart } }),

  create: (data: CreateMealRequest) =>
    api.post<Meal>('/api/meals', data),

  update: (id: number, data: UpdateMealRequest) =>
    api.put<Meal>(`/api/meals/${id}`, data),

  delete: (id: number) =>
    api.delete(`/api/meals/${id}`),
}
