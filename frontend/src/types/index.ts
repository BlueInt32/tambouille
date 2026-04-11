export interface Meal {
  id: number
  date: string
  slot: number
  name: string
  persons: number
}

export interface CreateMealRequest {
  date: string
  slot: number
  name: string
  persons: number
}

export interface UpdateMealRequest {
  name: string
  persons: number
}
