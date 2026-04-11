import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { mealsApi } from '@/services/api'
import type { CreateMealRequest, Meal, UpdateMealRequest } from '@/types'

function getMondayOf(date: Date): Date {
  const d = new Date(date)
  const day = d.getDay()
  const diff = day === 0 ? -6 : 1 - day
  d.setDate(d.getDate() + diff)
  d.setHours(0, 0, 0, 0)
  return d
}

function formatDate(date: Date): string {
  return date.toISOString().slice(0, 10)
}

export const usePlanningStore = defineStore('planning', () => {
  const currentWeekMonday = ref<Date>(getMondayOf(new Date()))
  const meals = ref<Meal[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const weekDays = computed<Date[]>(() => {
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(currentWeekMonday.value)
      d.setDate(d.getDate() + i)
      return d
    })
  })

  const weekStart = computed(() => formatDate(currentWeekMonday.value))

  function mealsForDay(date: string): Meal[] {
    return meals.value.filter((m) => m.date === date).sort((a, b) => a.slot - b.slot)
  }

  async function fetchWeek() {
    loading.value = true
    error.value = null
    try {
      const { data } = await mealsApi.getWeek(weekStart.value)
      meals.value = data
    } catch {
      error.value = 'Impossible de charger les repas'
    } finally {
      loading.value = false
    }
  }

  function previousWeek() {
    const d = new Date(currentWeekMonday.value)
    d.setDate(d.getDate() - 7)
    currentWeekMonday.value = d
    fetchWeek()
  }

  function nextWeek() {
    const d = new Date(currentWeekMonday.value)
    d.setDate(d.getDate() + 7)
    currentWeekMonday.value = d
    fetchWeek()
  }

  async function addMeal(data: CreateMealRequest) {
    const { data: meal } = await mealsApi.create(data)
    meals.value.push(meal)
  }

  async function updateMeal(id: number, data: UpdateMealRequest) {
    const { data: updated } = await mealsApi.update(id, data)
    const idx = meals.value.findIndex((m) => m.id === id)
    if (idx !== -1) meals.value[idx] = updated
  }

  async function deleteMeal(id: number) {
    await mealsApi.delete(id)
    meals.value = meals.value.filter((m) => m.id !== id)
  }

  return {
    currentWeekMonday,
    meals,
    loading,
    error,
    weekDays,
    weekStart,
    mealsForDay,
    fetchWeek,
    previousWeek,
    nextWeek,
    addMeal,
    updateMeal,
    deleteMeal,
  }
})
