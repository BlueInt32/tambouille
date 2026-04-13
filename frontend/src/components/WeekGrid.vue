<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { usePlanningStore, addDays } from '@/stores/planning'
import DayCard from '@/components/DayCard.vue'

const emit = defineEmits<{ 'select-day': [date: string] }>()
const store = usePlanningStore()

// --- Helpers ---
const MONTHS_FR = ['janvier', 'février', 'mars', 'avril', 'mai', 'juin',
                   'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre']

function isoWeek(monday: Date): number {
  const d = new Date(monday)
  d.setDate(d.getDate() + 3 - (d.getDay() + 6) % 7)
  const week1 = new Date(d.getFullYear(), 0, 4)
  return 1 + Math.round(((d.getTime() - week1.getTime()) / 86400000 - 3 + (week1.getDay() + 6) % 7) / 7)
}

function weekLabel(monday: Date): string {
  const days = Array.from({ length: 7 }, (_, i) => addDays(monday, i))
  const first = days[0], last = days[6]
  const d1 = first.getDate(), d2 = last.getDate()
  const m1 = MONTHS_FR[first.getMonth()], m2 = MONTHS_FR[last.getMonth()]
  const y = last.getFullYear()
  return first.getMonth() === last.getMonth()
    ? `${d1} – ${d2} ${m1} ${y}`
    : `${d1} ${m1} – ${d2} ${m2} ${y}`
}

function daysOf(monday: Date): Date[] {
  return Array.from({ length: 7 }, (_, i) => addDays(monday, i))
}

function dateStr(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

// --- Panneaux (état local, indépendant du store) ---
const leftMonday = ref<Date>(addDays(store.currentWeekMonday, -7))
const centerMonday = ref<Date>(new Date(store.currentWeekMonday))
const rightMonday = ref<Date>(addDays(store.currentWeekMonday, 7))

// --- Carrousel ---
const track = ref<HTMLElement | null>(null)
const dragDelta = ref(0)
const animated = ref(false)
const isAnimating = ref(false)
const touchStartX = ref(0)
const touchStartY = ref(0)
const dragging = ref(false)

const trackStyle = computed(() => ({
  transform: `translateX(calc(-100vw + ${dragDelta.value}px))`,
  transition: animated.value ? 'transform 180ms cubic-bezier(0.25, 0.46, 0.45, 0.94)' : 'none',
  willChange: 'transform',
}))

async function completeNav(direction: 1 | -1) {
  // direction: 1 = semaine suivante (glisse à gauche), -1 = semaine précédente (glisse à droite)
  isAnimating.value = true
  animated.value = true
  dragDelta.value = direction === 1 ? -window.innerWidth : window.innerWidth

  await sleep(180)

  if (direction === 1) {
    const oldCenter = new Date(centerMonday.value)
    const oldRight = new Date(rightMonday.value)
    // 1. Mettre le centre à jour → Vue re-rend le panneau central avec le contenu du droit
    centerMonday.value = oldRight
    await nextTick()
    // 2. Maintenant le centre a le bon contenu → snap sans animation
    animated.value = false
    dragDelta.value = 0
    // 3. Mettre à jour les panneaux hors-écran
    leftMonday.value = oldCenter
    rightMonday.value = addDays(oldRight, 7)
    store.goToWeek(oldRight)
  } else {
    const oldCenter = new Date(centerMonday.value)
    const oldLeft = new Date(leftMonday.value)
    centerMonday.value = oldLeft
    await nextTick()
    animated.value = false
    dragDelta.value = 0
    rightMonday.value = oldCenter
    leftMonday.value = addDays(oldLeft, -7)
    store.goToWeek(oldLeft)
  }

  isAnimating.value = false
}

// --- Touch ---
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
    if (Math.abs(dy) > Math.abs(dx)) return
    if (Math.abs(dx) < 8) return
    dragging.value = true
  }
  e.preventDefault()
  dragDelta.value = dx
}

async function onTouchEnd(e: TouchEvent) {
  if (!dragging.value) return
  dragging.value = false
  const dx = e.changedTouches[0].clientX - touchStartX.value
  if (Math.abs(dx) >= 60) {
    await completeNav(dx < 0 ? 1 : -1)
  } else {
    animated.value = true
    dragDelta.value = 0
    await sleep(180)
  }
}

function sleep(ms: number) {
  return new Promise((r) => setTimeout(r, ms))
}

// --- Indicateur de chargement avec debounce ---
const showLoading = ref(false)
let loadingTimer: ReturnType<typeof setTimeout> | null = null

watch(() => store.loading, (loading) => {
  if (loading) {
    loadingTimer = setTimeout(() => { showLoading.value = true }, 300)
  } else {
    if (loadingTimer) { clearTimeout(loadingTimer); loadingTimer = null }
    showLoading.value = false
  }
})

onMounted(() => {
  track.value?.addEventListener('touchmove', onTouchMove, { passive: false })
})
onUnmounted(() => {
  track.value?.removeEventListener('touchmove', onTouchMove)
})
</script>

<template>
  <div class="overflow-hidden h-full">
    <div ref="track" class="flex h-full" :style="trackStyle" @touchstart="onTouchStart" @touchend="onTouchEnd">

      <!-- Panneau gauche : semaine précédente -->
      <div class="w-screen shrink-0 px-4 flex flex-col h-full">
        <div class="flex items-center justify-between py-2 shrink-0">
          <button @click="completeNav(-1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-left text-base"></i>
          </button>
          <p class="font-display text-text font-semibold text-base leading-tight">{{ weekLabel(leftMonday) }}<span class="font-sans font-normal text-text-muted text-sm"> - S{{ isoWeek(leftMonday) }}</span></p>
          <button @click="completeNav(1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-right text-base"></i>
          </button>
        </div>
        <div class="flex flex-col gap-1.5 flex-1 min-h-0 pb-3">
          <DayCard
            v-for="day in daysOf(leftMonday)"
            :key="dateStr(day)"
            :date="day"
            :meals="store.mealsForDayOf(leftMonday, dateStr(day))"
            @click="emit('select-day', dateStr(day))"
            class="flex-1"
          />
        </div>
      </div>

      <!-- Panneau central : semaine courante -->
      <div class="w-screen shrink-0 px-4 flex flex-col h-full">
        <div class="flex items-center justify-between py-2 shrink-0">
          <button @click="completeNav(-1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-left text-base"></i>
          </button>
          <div class="text-center">
            <p class="font-display text-text font-semibold text-base leading-tight">{{ weekLabel(centerMonday) }}<span class="font-sans font-normal text-text-muted text-sm"> - S{{ isoWeek(centerMonday) }}</span></p>
            <p v-if="showLoading" class="text-text-muted text-xs mt-0.5 font-sans">Chargement…</p>
          </div>
          <button @click="completeNav(1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-right text-base"></i>
          </button>
        </div>
        <div v-if="store.error" class="mb-2 p-3 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm font-sans shrink-0">
          {{ store.error }}
        </div>
        <div class="flex flex-col gap-1.5 flex-1 min-h-0 pb-3">
          <DayCard
            v-for="day in daysOf(centerMonday)"
            :key="dateStr(day)"
            :date="day"
            :meals="store.mealsForDayOf(centerMonday, dateStr(day))"
            @click="emit('select-day', dateStr(day))"
            class="flex-1"
          />
        </div>
      </div>

      <!-- Panneau droit : semaine suivante -->
      <div class="w-screen shrink-0 px-4 flex flex-col h-full">
        <div class="flex items-center justify-between py-2 shrink-0">
          <button @click="completeNav(-1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-left text-base"></i>
          </button>
          <p class="font-display text-text font-semibold text-base leading-tight">{{ weekLabel(rightMonday) }}<span class="font-sans font-normal text-text-muted text-sm"> - S{{ isoWeek(rightMonday) }}</span></p>
          <button @click="completeNav(1)" :disabled="isAnimating"
            class="w-10 h-10 flex items-center justify-center rounded-full text-primary hover:bg-primary/10 active:bg-primary/20 transition-colors disabled:opacity-40">
            <i class="pi pi-chevron-right text-base"></i>
          </button>
        </div>
        <div class="flex flex-col gap-1.5 flex-1 min-h-0 pb-3">
          <DayCard
            v-for="day in daysOf(rightMonday)"
            :key="dateStr(day)"
            :date="day"
            :meals="store.mealsForDayOf(rightMonday, dateStr(day))"
            @click="emit('select-day', dateStr(day))"
            class="flex-1"
          />
        </div>
      </div>

    </div>
  </div>
</template>
