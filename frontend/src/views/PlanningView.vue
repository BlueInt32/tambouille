<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { usePlanningStore } from '@/stores/planning'
import { useTheme } from '@/composables/useTheme'
import WeekGrid from '@/components/WeekGrid.vue'
import DayDetail from '@/components/DayDetail.vue'

const router = useRouter()
const store = usePlanningStore()
const { isDark, toggle: toggleTheme } = useTheme()
const selectedDate = ref<string | null>(null)
const weekGridRef = ref<InstanceType<typeof WeekGrid> | null>(null)

function getTodayMondayStr(): string {
  const d = new Date()
  const day = d.getDay()
  const diff = day === 0 ? -6 : 1 - day
  d.setDate(d.getDate() + diff)
  d.setHours(0, 0, 0, 0)
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${dd}`
}

function mondayStr(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

const isOnTodayWeek = computed(() =>
  mondayStr(store.currentWeekMonday) === getTodayMondayStr()
)

function goToToday() {
  weekGridRef.value?.goToToday()
}

function getISOWeek(date: Date): number {
  const d = new Date(date)
  d.setHours(0, 0, 0, 0)
  d.setDate(d.getDate() + 3 - (d.getDay() + 6) % 7)
  const week1 = new Date(d.getFullYear(), 0, 4)
  return 1 + Math.round(((d.getTime() - week1.getTime()) / 86400000 - 3 + (week1.getDay() + 6) % 7) / 7)
}

function setWeekTitle() {
  document.title = `Tambouille - Semaine ${getISOWeek(store.currentWeekMonday)}`
}

onMounted(() => {
  store.fetchWeek()
  setWeekTitle()
})

watch(selectedDate, (date) => {
  if (date) {
    const fullDate = new Date(date).toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
    document.title = `Tambouille - ${fullDate.charAt(0).toUpperCase() + fullDate.slice(1)}`
  } else {
    setWeekTitle()
  }
})

watch(() => store.currentWeekMonday, () => {
  if (!selectedDate.value) setWeekTitle()
})

function selectDay(date: string) {
  selectedDate.value = date
}

function back() {
  selectedDate.value = null
}

function logout() {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

<template>
  <div class="h-dvh flex flex-col bg-surface font-sans overflow-hidden">
    <!-- Barre de titre -->
    <header class="bg-surface-card border-b border-amber-100 dark:border-amber-900/30 px-4 py-2 flex items-center justify-between sticky top-0 z-20 shadow-sm">
      <Transition name="fade-fast" mode="out-in">
        <button
          v-if="selectedDate"
          key="back"
          @click="back"
          class="flex items-center gap-1.5 text-text font-semibold text-sm py-1 px-2 -ml-2 rounded-lg
                 hover:bg-amber-100 dark:hover:bg-amber-900/30 active:bg-amber-100/80 transition-colors"
        >
          <i class="pi pi-chevron-left text-sm"></i>
          Semaine
        </button>
        <span v-else key="title" class="font-display text-text text-lg font-semibold">Tambouille</span>
      </Transition>

      <div class="flex items-center gap-0.5">
        <Transition name="fade-fast">
          <button
            v-if="!isOnTodayWeek && !selectedDate"
            @click="goToToday"
            class="text-text-muted hover:text-text transition-colors p-1.5 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20"
            title="Revenir à cette semaine"
          >
            <i class="pi pi-calendar text-base"></i>
          </button>
        </Transition>
        <button
          @click="toggleTheme"
          class="text-text-muted hover:text-text transition-colors p-1.5 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20"
          :title="isDark ? 'Mode clair' : 'Mode nuit'"
        >
          <i :class="isDark ? 'pi pi-sun' : 'pi pi-moon'" class="text-base"></i>
        </button>
        <button
          @click="logout"
          class="text-text-muted hover:text-text transition-colors p-1.5 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20"
          title="Déconnexion"
        >
          <i class="pi pi-sign-out text-base"></i>
        </button>
      </div>
    </header>

    <!-- Contenu principal avec transition slide -->
    <div class="relative flex-1 min-h-0 overflow-hidden">
      <WeekGrid
        ref="weekGridRef"
        @select-day="selectDay"
      />
      <Transition name="sheet">
        <DayDetail
          v-if="selectedDate"
          :key="selectedDate"
          :date="selectedDate"
          @back="back"
          class="absolute inset-0 overflow-y-auto bg-surface"
        />
      </Transition>
    </div>
  </div>
</template>

<style scoped>
.sheet-enter-active,
.sheet-leave-active {
  transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}
.sheet-enter-from,
.sheet-leave-to {
  transform: translateY(100%);
}

.fade-fast-enter-active,
.fade-fast-leave-active {
  transition: opacity 0.15s ease;
}
.fade-fast-enter-from,
.fade-fast-leave-to {
  opacity: 0;
}
</style>
