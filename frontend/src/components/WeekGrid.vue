<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { usePlanningStore } from '@/stores/planning'
import DayCard from '@/components/DayCard.vue'

const emit = defineEmits<{
  'select-day': [date: string]
}>()

const store = usePlanningStore()

const MONTHS_FR = ['janvier', 'février', 'mars', 'avril', 'mai', 'juin',
                   'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre']

const weekLabel = computed(() => {
  const days = store.weekDays
  const first = days[0]
  const last = days[6]
  const d1 = first.getDate()
  const d2 = last.getDate()
  const m1 = MONTHS_FR[first.getMonth()]
  const m2 = MONTHS_FR[last.getMonth()]
  const y = last.getFullYear()
  if (first.getMonth() === last.getMonth()) {
    return `${d1} – ${d2} ${m1} ${y}`
  }
  return `${d1} ${m1} – ${d2} ${m2} ${y}`
})

function dateString(date: Date): string {
  return date.toISOString().slice(0, 10)
}

// --- Swipe ---
const wrapper = ref<HTMLElement | null>(null)
const offsetX = ref(0)
const animated = ref(false)
const isAnimating = ref(false)
const touchStartX = ref(0)
const touchStartY = ref(0)
const dragging = ref(false)

const contentStyle = computed(() => ({
  transform: `translateX(${offsetX.value}px)`,
  transition: animated.value ? 'transform 300ms cubic-bezier(0.25, 0.46, 0.45, 0.94)' : 'none',
  willChange: 'transform',
}))

function onTouchStart(e: TouchEvent) {
  if (isAnimating.value) return
  touchStartX.value = e.touches[0].clientX
  touchStartY.value = e.touches[0].clientY
  dragging.value = false
  animated.value = false
}

function onTouchMove(e: TouchEvent) {
  if (isAnimating.value) return
  const dx = e.touches[0].clientX - touchStartX.value
  const dy = e.touches[0].clientY - touchStartY.value

  if (!dragging.value) {
    if (Math.abs(dy) > Math.abs(dx)) return        // scroll vertical
    if (Math.abs(dx) < 8) return                   // pas encore décidé
    dragging.value = true
  }

  e.preventDefault()
  offsetX.value = dx
}

async function onTouchEnd(e: TouchEvent) {
  if (!dragging.value) return
  dragging.value = false

  const dx = e.changedTouches[0].clientX - touchStartX.value
  const vw = window.innerWidth
  const THRESHOLD = 60

  if (Math.abs(dx) >= THRESHOLD && !store.loading) {
    isAnimating.value = true
    animated.value = true
    offsetX.value = dx > 0 ? vw : -vw          // glisse vers l'extérieur

    await sleep(300)

    animated.value = false
    if (dx > 0) store.previousWeek()
    else store.nextWeek()
    offsetX.value = dx > 0 ? -vw : vw          // arrive de l'autre côté

    await sleep(30)                              // laisser Vue updater le DOM

    animated.value = true
    offsetX.value = 0                           // glisse vers le centre

    await sleep(300)
    isAnimating.value = false
  } else {
    animated.value = true
    offsetX.value = 0
    await sleep(300)
  }
}

function sleep(ms: number) {
  return new Promise(r => setTimeout(r, ms))
}

onMounted(() => {
  wrapper.value?.addEventListener('touchmove', onTouchMove, { passive: false })
})

onUnmounted(() => {
  wrapper.value?.removeEventListener('touchmove', onTouchMove)
})
</script>

<template>
  <div
    ref="wrapper"
    class="px-4 pb-8"
    :style="contentStyle"
    @touchstart="onTouchStart"
    @touchend="onTouchEnd"
  >
    <!-- Navigation semaine -->
    <div class="flex items-center justify-between py-5">
      <button
        @click="store.previousWeek"
        class="w-10 h-10 flex items-center justify-center rounded-full text-primary
               hover:bg-primary/10 active:bg-primary/20 transition-colors"
        :disabled="store.loading"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-5 h-5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
        </svg>
      </button>

      <div class="text-center">
        <p class="font-display text-text font-semibold text-base leading-tight">{{ weekLabel }}</p>
        <p v-if="store.loading" class="text-text-muted text-xs mt-0.5 font-sans">Chargement…</p>
      </div>

      <button
        @click="store.nextWeek"
        class="w-10 h-10 flex items-center justify-center rounded-full text-primary
               hover:bg-primary/10 active:bg-primary/20 transition-colors"
        :disabled="store.loading"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-5 h-5">
          <path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
        </svg>
      </button>
    </div>

    <!-- Erreur -->
    <div v-if="store.error" class="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm font-sans">
      {{ store.error }}
    </div>

    <!-- Grille des jours -->
    <div class="flex flex-col gap-3">
      <DayCard
        v-for="day in store.weekDays"
        :key="dateString(day)"
        :date="day"
        :meals="store.mealsForDay(dateString(day))"
        @click="emit('select-day', dateString(day))"
      />
    </div>
  </div>
</template>
