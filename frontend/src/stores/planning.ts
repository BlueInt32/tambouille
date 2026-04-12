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
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

export function addDays(date: Date, days: number): Date {
  const d = new Date(date)
  d.setDate(d.getDate() + days)
  return d
}

export const usePlanningStore = defineStore('planning', () => {
  const currentWeekMonday = ref<Date>(getMondayOf(new Date()))
  const mealsCache = ref<Record<string, Meal[]>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  const weekStart = computed(() => formatDate(currentWeekMonday.value))

  const weekDays = computed<Date[]>(() =>
    Array.from({ length: 7 }, (_, i) => addDays(currentWeekMonday.value, i))
  )

  // Backward compat
  const meals = computed(() => mealsCache.value[weekStart.value] ?? [])

  function mealsForDay(date: string): Meal[] {
    return (mealsCache.value[weekStart.value] ?? [])
      .filter((m) => m.date === date)
      .sort((a, b) => a.slot - b.slot)
  }

  function mealsForDayOf(monday: Date, date: string): Meal[] {
    return (mealsCache.value[formatDate(monday)] ?? [])
      .filter((m) => m.date === date)
      .sort((a, b) => a.slot - b.slot)
  }

  async function fetchWeekByMonday(monday: Date, force = false): Promise<void> {
    const key = formatDate(monday)
    if (!force && mealsCache.value[key] !== undefined) return
    const { data } = await mealsApi.getWeek(key)
    mealsCache.value[key] = data
  }

  async function fetchWeek() {
    loading.value = true
    error.value = null
    try {
      await fetchWeekByMonday(currentWeekMonday.value, true)
    } catch {
      error.value = 'Impossible de charger les repas'
    } finally {
      loading.value = false
    }
    // Préchargement silencieux des semaines adjacentes
    fetchWeekByMonday(addDays(currentWeekMonday.value, -7)).catch(() => {})
    fetchWeekByMonday(addDays(currentWeekMonday.value, 7)).catch(() => {})
  }

  function goToWeek(monday: Date) {
    currentWeekMonday.value = new Date(monday)
    fetchWeek()
  }

  function previousWeek() {
    goToWeek(addDays(currentWeekMonday.value, -7))
  }

  function nextWeek() {
    goToWeek(addDays(currentWeekMonday.value, 7))
  }

  async function addMeal(data: CreateMealRequest) {
    const { data: meal } = await mealsApi.create(data)
    const key = weekStart.value
    mealsCache.value[key] = [...(mealsCache.value[key] ?? []), meal]
  }

  async function updateMeal(id: number, data: UpdateMealRequest) {
    const { data: updated } = await mealsApi.update(id, data)
    for (const key of Object.keys(mealsCache.value)) {
      const idx = mealsCache.value[key].findIndex((m) => m.id === id)
      if (idx !== -1) {
        mealsCache.value[key] = mealsCache.value[key].map((m, i) => (i === idx ? updated : m))
        break
      }
    }
  }

  async function deleteMeal(id: number) {
    await mealsApi.delete(id)
    for (const key of Object.keys(mealsCache.value)) {
      if (mealsCache.value[key].some((m) => m.id === id)) {
        mealsCache.value[key] = mealsCache.value[key].filter((m) => m.id !== id)
        break
      }
    }
  }

  return {
    currentWeekMonday,
    meals,
    loading,
    error,
    weekDays,
    weekStart,
    mealsForDay,
    mealsForDayOf,
    fetchWeek,
    fetchWeekByMonday,
    goToWeek,
    previousWeek,
    nextWeek,
    addMeal,
    updateMeal,
    deleteMeal,
  }
})
