<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePlanningStore } from '@/stores/planning'
import { useKnownDishesStore } from '@/stores/knownDishes'
import type { Meal } from '@/types'

const props = defineProps<{
  date: string
  meal: Meal | null
  slot: number
}>()

const emit = defineEmits<{
  saved: []
  cancel: []
}>()

const store = usePlanningStore()
const knownDishesStore = useKnownDishesStore()

const name = ref(props.meal?.name ?? '')
const persons = ref(props.meal?.persons ?? 2)
const nameInput = ref<HTMLInputElement | null>(null)
const showSuggestions = ref(false)

const suggestions = computed(() => {
  const q = name.value.trim().toLowerCase()
  if (!q) return []
  return knownDishesStore.dishes
    .filter(d => d.toLowerCase().includes(q) && d.toLowerCase() !== q)
    .slice(0, 5)
})

function selectSuggestion(s: string) {
  name.value = s
  showSuggestions.value = false
}

onMounted(() => {
  nameInput.value?.focus({ preventScroll: true })
  knownDishesStore.load()
})
const loading = ref(false)
const error = ref('')

function increment() {
  persons.value = Math.min(persons.value + 1, 20)
}
function decrement() {
  persons.value = Math.max(persons.value - 1, 1)
}

async function submit() {
  if (!name.value.trim() || loading.value) return
  loading.value = true
  error.value = ''
  try {
    if (props.meal) {
      await store.updateMeal(props.meal.id, { name: name.value.trim(), persons: persons.value })
    } else {
      await store.addMeal({ date: props.date, slot: props.slot, name: name.value.trim(), persons: persons.value })
    }
    knownDishesStore.invalidate()
    emit('saved')
  } catch {
    error.value = 'Une erreur est survenue'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <form @submit.prevent="submit" class="flex flex-col gap-3">
    <!-- Nom du plat -->
    <div>
      <label class="block text-xs font-sans font-semibold text-text-muted uppercase tracking-wider mb-1.5">
        Plat
      </label>
      <input
        ref="nameInput"
        v-model="name"
        type="text"
        placeholder="Ex : Poulet rôti, Pâtes…"
        autocomplete="off"
        @focus="showSuggestions = true"
        @blur="showSuggestions = false"
        class="w-full px-3.5 py-2.5 rounded-xl border border-amber-200 dark:border-amber-800/40 bg-surface font-sans text-text text-sm
               placeholder:text-text-muted/40 focus:outline focus:outline-2 focus:outline-primary focus:outline-offset-0
               focus:border-transparent transition-all"
        :class="{ 'border-red-300': error }"
      />
      <ul
        v-if="showSuggestions && suggestions.length"
        class="mt-1 rounded-xl border border-amber-200 dark:border-amber-800/40 bg-surface overflow-hidden"
      >
        <li
          v-for="s in suggestions"
          :key="s"
          @mousedown.prevent="selectSuggestion(s)"
          @touchstart.prevent="selectSuggestion(s)"
          class="px-3.5 py-3 font-sans text-sm text-text active:bg-amber-50 dark:active:bg-amber-900/20 cursor-pointer
                 border-b border-amber-100 dark:border-amber-800/20 last:border-b-0"
        >
          {{ s }}
        </li>
      </ul>
    </div>

    <!-- Nombre de personnes -->
    <div>
      <label class="block text-xs font-sans font-semibold text-text-muted uppercase tracking-wider mb-1.5">
        Personnes
      </label>
      <div class="flex items-center gap-3">
        <button
          type="button"
          @click="decrement"
          :disabled="persons <= 1"
          class="w-9 h-9 rounded-xl bg-amber-50 dark:bg-amber-900/20 text-text font-bold text-lg
                 flex items-center justify-center
                 hover:bg-amber-100 dark:hover:bg-amber-900/30 active:scale-95 transition-all
                 disabled:opacity-30 disabled:cursor-not-allowed"
        >−</button>

        <span class="font-display text-text font-bold text-xl w-8 text-center">{{ persons }}</span>

        <button
          type="button"
          @click="increment"
          :disabled="persons >= 20"
          class="w-9 h-9 rounded-xl bg-amber-50 dark:bg-amber-900/20 text-text font-bold text-lg
                 flex items-center justify-center
                 hover:bg-amber-100 dark:hover:bg-amber-900/30 active:scale-95 transition-all
                 disabled:opacity-30 disabled:cursor-not-allowed"
        >+</button>

        <span class="font-sans text-text-muted text-sm">
          personne{{ persons > 1 ? 's' : '' }}
        </span>
      </div>
    </div>

    <!-- Erreur -->
    <p v-if="error" class="text-red-500 text-sm font-sans">{{ error }}</p>

    <!-- Actions -->
    <div class="flex gap-2 pt-1">
      <button
        type="button"
        @click="emit('cancel')"
        class="flex-1 py-2.5 rounded-xl border border-amber-200 dark:border-amber-800/40 text-text-muted font-sans font-medium text-sm
               hover:bg-amber-50 dark:hover:bg-amber-900/20 active:scale-95 transition-all"
      >
        Annuler
      </button>
      <button
        type="submit"
        :disabled="!name.trim() || loading"
        class="flex-1 py-2.5 rounded-xl bg-primary text-white font-sans font-semibold text-sm
               hover:bg-primary-dark active:scale-95 transition-all
               disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-1.5"
      >
        <i v-if="loading" class="pi pi-spinner pi-spin text-sm"></i>
        {{ meal ? 'Enregistrer' : 'Ajouter' }}
      </button>
    </div>
  </form>
</template>
