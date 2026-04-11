<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePlanningStore } from '@/stores/planning'
import MealForm from '@/components/MealForm.vue'
import type { Meal } from '@/types'

const props = defineProps<{
  date: string
}>()

defineEmits<{ back: [] }>()

const store = usePlanningStore()

const DAYS_FR = ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi']
const MONTHS_FR = ['janvier', 'février', 'mars', 'avril', 'mai', 'juin',
                   'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre']
const SLOT_LABELS = ['Midi', 'Soir']

const dateObj = new Date(props.date + 'T12:00:00')
const dayLabel = computed(() => {
  return `${DAYS_FR[dateObj.getDay()]} ${dateObj.getDate()} ${MONTHS_FR[dateObj.getMonth()]}`
})

const meals = computed(() => store.mealsForDay(props.date))

const availableSlots = computed(() => {
  const taken = meals.value.map((m) => m.slot)
  return [0, 1].filter((s) => !taken.includes(s))
})

// État du formulaire
const editingMeal = ref<Meal | null>(null)
const addingSlot = ref<number | null>(null)
const formVisible = computed(() => editingMeal.value !== null || addingSlot.value !== null)

const deletingId = ref<number | null>(null)

function startAdd() {
  editingMeal.value = null
  addingSlot.value = availableSlots.value[0]
}

function startEdit(meal: Meal) {
  addingSlot.value = null
  editingMeal.value = meal
}

function closeForm() {
  editingMeal.value = null
  addingSlot.value = null
}

async function onSaved() {
  closeForm()
}

async function deleteMeal(id: number) {
  deletingId.value = id
  try {
    await store.deleteMeal(id)
  } finally {
    deletingId.value = null
  }
}
</script>

<template>
  <div class="pb-8">
    <!-- En-tête du jour -->
    <div class="px-4 py-5 border-b border-amber-100">
      <h2 class="font-display text-text text-2xl font-bold capitalize">{{ dayLabel }}</h2>
      <p class="font-sans text-text-muted text-sm mt-0.5">
        {{ meals.length === 0 ? 'Aucun repas prévu' : `${meals.length} repas planifié${meals.length > 1 ? 's' : ''}` }}
      </p>
    </div>

    <div class="px-4 mt-4 flex flex-col gap-3">
      <!-- Carte Midi -->
      <div v-for="slot in [0, 1]" :key="slot">
        <div
          class="rounded-2xl overflow-hidden"
          :class="meals.find(m => m.slot === slot) ? 'bg-surface-card shadow-sm' : 'bg-amber-50/50 border border-dashed border-amber-200'"
        >
          <!-- Slot avec repas -->
          <template v-if="meals.find(m => m.slot === slot) as Meal">
            <div class="p-4">
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <span class="inline-block text-xs font-sans font-semibold text-primary uppercase tracking-wider mb-1">
                    {{ SLOT_LABELS[slot] }}
                  </span>
                  <p class="font-display text-text text-lg font-semibold leading-snug">
                    {{ meals.find(m => m.slot === slot)!.name }}
                  </p>
                  <p class="font-sans text-text-muted text-sm mt-1 flex items-center gap-1">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-4 h-4">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
                    </svg>
                    {{ meals.find(m => m.slot === slot)!.persons }} personne{{ meals.find(m => m.slot === slot)!.persons > 1 ? 's' : '' }}
                  </p>
                </div>
                <div class="flex gap-1 flex-shrink-0">
                  <button
                    @click="startEdit(meals.find(m => m.slot === slot)!)"
                    class="w-9 h-9 rounded-xl flex items-center justify-center text-text-muted
                           hover:bg-amber-100 hover:text-primary active:scale-95 transition-all"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-4.5 h-4.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125" />
                    </svg>
                  </button>
                  <button
                    @click="deleteMeal(meals.find(m => m.slot === slot)!.id)"
                    :disabled="deletingId === meals.find(m => m.slot === slot)!.id"
                    class="w-9 h-9 rounded-xl flex items-center justify-center text-text-muted
                           hover:bg-red-50 hover:text-red-500 active:scale-95 transition-all
                           disabled:opacity-40"
                  >
                    <svg v-if="deletingId !== meals.find(m => m.slot === slot)!.id" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-4.5 h-4.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                    </svg>
                    <svg v-else class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                    </svg>
                  </button>
                </div>
              </div>

              <!-- Formulaire d'édition inline -->
              <Transition name="expand">
                <MealForm
                  v-if="editingMeal?.id === meals.find(m => m.slot === slot)?.id"
                  :date="date"
                  :meal="editingMeal"
                  :slot="slot"
                  class="mt-4 pt-4 border-t border-amber-100"
                  @saved="onSaved"
                  @cancel="closeForm"
                />
              </Transition>
            </div>
          </template>

          <!-- Slot vide -->
          <template v-else>
            <button
              v-if="!formVisible || addingSlot === slot"
              @click="addingSlot === slot ? null : (addingSlot = slot)"
              class="w-full p-4 flex items-center gap-3 text-left"
              :class="addingSlot === slot ? 'cursor-default' : 'hover:bg-amber-100/50 active:bg-amber-100 transition-colors cursor-pointer'"
            >
              <span class="text-xs font-sans font-semibold text-text-muted uppercase tracking-wider w-8">{{ SLOT_LABELS[slot] }}</span>
              <span v-if="addingSlot !== slot" class="font-sans text-text-muted/60 text-sm italic">Ajouter un repas…</span>
            </button>

            <Transition name="expand">
              <div v-if="addingSlot === slot" class="px-4 pb-4">
                <MealForm
                  :date="date"
                  :meal="null"
                  :slot="slot"
                  @saved="onSaved"
                  @cancel="closeForm"
                />
              </div>
            </Transition>
          </template>
        </div>
      </div>

      <!-- Bouton ajout si des slots sont libres et formulaire pas visible -->
      <button
        v-if="availableSlots.length > 0 && !formVisible"
        @click="startAdd"
        class="w-full py-3.5 rounded-2xl bg-primary text-white font-sans font-semibold text-sm
               flex items-center justify-center gap-2
               hover:bg-primary-dark active:scale-[0.98] transition-all shadow-sm mt-1"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-4 h-4">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
        </svg>
        Ajouter un repas
      </button>
    </div>
  </div>
</template>

<style scoped>
.expand-enter-active,
.expand-leave-active {
  transition: max-height 0.25s ease, opacity 0.2s ease;
  overflow: hidden;
  max-height: 400px;
}
.expand-enter-from,
.expand-leave-to {
  max-height: 0;
  opacity: 0;
}
</style>
