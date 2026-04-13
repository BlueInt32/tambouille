<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { usePlanningStore } from '@/stores/planning'
import MealForm from '@/components/MealForm.vue'
import type { Meal } from '@/types'

const props = defineProps<{
  date: string
}>()

const emit = defineEmits<{ back: [] }>()

const store = usePlanningStore()

// Swipe to close
const rootEl = ref<HTMLElement | null>(null)
const dragY = ref(0)
const isDragging = ref(false)
let startY = 0
let startX = 0

function onTouchStart(e: TouchEvent) {
  startY = e.touches[0].clientY
  startX = e.touches[0].clientX
  isDragging.value = true
  dragY.value = 0
}

function onTouchMove(e: TouchEvent) {
  if (!isDragging.value) return
  const dy = e.touches[0].clientY - startY
  const dx = Math.abs(e.touches[0].clientX - startX)
  if (dy > 0 && dx < 40) {
    dragY.value = dy
    e.preventDefault()
  }
}

function onTouchEnd() {
  isDragging.value = false
  if (dragY.value > 90) {
    emit('back')
  } else {
    dragY.value = 0
  }
}

onMounted(() => {
  rootEl.value?.addEventListener('touchmove', onTouchMove, { passive: false })
})

onUnmounted(() => {
  rootEl.value?.removeEventListener('touchmove', onTouchMove)
})

const DAYS_FR = ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi']
const MONTHS_FR = ['janvier', 'février', 'mars', 'avril', 'mai', 'juin',
                   'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre']
const SLOT_LABELS = ['Midi', 'Soir']

const dateObj = new Date(props.date + 'T12:00:00')
const dayLabel = computed(() => {
  return `${DAYS_FR[dateObj.getDay()]} ${dateObj.getDate()} ${MONTHS_FR[dateObj.getMonth()]}`
})

const meals = computed(() => store.mealsForDay(props.date))

// État du formulaire
const editingMeal = ref<Meal | null>(null)
const addingSlot = ref<number | null>(null)

const deletingId = ref<number | null>(null)

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
  <div
    ref="rootEl"
    class="pb-8"
    :style="dragY > 0 ? { transform: `translateY(${dragY}px)`, transition: 'none', opacity: String(Math.max(0.6, 1 - dragY / 300)) } : { transition: 'transform 0.3s cubic-bezier(0.4,0,0.2,1), opacity 0.3s ease' }"
    @touchstart="onTouchStart"
    @touchend="onTouchEnd"
  >
    <!-- Drag handle -->
    <div class="flex justify-center pt-3 pb-1">
      <div class="w-10 h-1 rounded-full bg-text-muted/20"></div>
    </div>

    <!-- En-tête du jour -->
    <div class="px-4 py-4 border-b border-amber-100 dark:border-amber-900/30">
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
          :class="meals.find(m => m.slot === slot) ? 'bg-surface-card shadow-sm' : 'bg-amber-50/50 dark:bg-amber-900/10 border border-dashed border-amber-600 dark:border-amber-700'"
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
                    <i class="pi pi-user text-sm"></i>
                    {{ meals.find(m => m.slot === slot)!.persons }} personne{{ meals.find(m => m.slot === slot)!.persons > 1 ? 's' : '' }}
                  </p>
                </div>
                <div class="flex items-center gap-0.5 flex-shrink-0">
                  <button
                    @click="startEdit(meals.find(m => m.slot === slot)!)"
                    class="w-8 h-8 rounded-xl flex items-center justify-center text-text-muted
                           hover:bg-amber-100 dark:hover:bg-amber-900/30 hover:text-primary active:scale-95 transition-all"
                  >
                    <i class="pi pi-pencil text-sm"></i>
                  </button>
                  <button
                    @click="deleteMeal(meals.find(m => m.slot === slot)!.id)"
                    :disabled="deletingId === meals.find(m => m.slot === slot)!.id"
                    class="w-8 h-8 rounded-xl flex items-center justify-center text-text-muted
                           hover:bg-red-50 hover:text-red-500 active:scale-95 transition-all
                           disabled:opacity-40"
                  >
                    <i v-if="deletingId !== meals.find(m => m.slot === slot)!.id" class="pi pi-trash text-sm"></i>
                    <i v-else class="pi pi-spinner pi-spin text-sm"></i>
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
                  class="mt-4 pt-4 border-t border-amber-100 dark:border-amber-900/30"
                  @saved="onSaved"
                  @cancel="closeForm"
                />
              </Transition>
            </div>
          </template>

          <!-- Slot vide -->
          <template v-else>
            <button
              @click="addingSlot = addingSlot === slot ? null : slot"
              class="w-full p-4 flex items-center gap-3 text-left"
              :class="addingSlot === slot ? 'cursor-default' : 'hover:bg-amber-100/50 dark:hover:bg-amber-900/20 active:bg-amber-100 dark:active:bg-amber-900/30 transition-colors cursor-pointer'"
            >
              <span class="text-xs font-sans font-semibold text-text-muted uppercase tracking-wider w-8">{{ SLOT_LABELS[slot] }}</span>
              <span v-if="addingSlot !== slot" class="font-sans text-text-muted text-sm italic">Ajouter un repas…</span>
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
