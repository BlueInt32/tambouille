import { ref } from 'vue'
import { defineStore } from 'pinia'
import { knownDishesApi } from '@/services/api'

export const useKnownDishesStore = defineStore('knownDishes', () => {
  const dishes = ref<string[]>([])
  const loaded = ref(false)

  async function load() {
    if (loaded.value) return
    dishes.value = (await knownDishesApi.getAll()).data
    loaded.value = true
  }

  function invalidate() {
    loaded.value = false
  }

  return { dishes, load, invalidate }
})
