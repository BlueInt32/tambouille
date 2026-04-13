<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { usePlanningStore } from '@/stores/planning'
import { useTheme } from '@/composables/useTheme'
import WeekGrid from '@/components/WeekGrid.vue'
import DayDetail from '@/components/DayDetail.vue'

const router = useRouter()
const store = usePlanningStore()
const { isDark, toggle: toggleTheme } = useTheme()
const selectedDate = ref<string | null>(null)

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
          class="flex items-center gap-1.5 text-primary font-semibold text-sm py-1 px-2 -ml-2 rounded-lg
                 hover:bg-primary/10 active:bg-primary/20 transition-colors"
        >
          <i class="pi pi-chevron-left text-sm"></i>
          Semaine
        </button>
        <span v-else key="title" class="font-display text-text text-lg font-semibold">Tambouille</span>
      </Transition>

      <div class="flex items-center gap-0.5">
        <button
          @click="toggleTheme"
          class="text-text-muted/60 hover:text-text-muted transition-colors p-1.5 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20"
          :title="isDark ? 'Mode clair' : 'Mode nuit'"
        >
          <i :class="isDark ? 'pi pi-sun' : 'pi pi-moon'" class="text-base"></i>
        </button>
        <button
          @click="logout"
          class="text-text-muted/60 hover:text-text-muted transition-colors p-1.5 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20"
          title="Déconnexion"
        >
          <i class="pi pi-sign-out text-base"></i>
        </button>
      </div>
    </header>

    <!-- Contenu principal avec transition slide -->
    <div class="relative flex-1 min-h-0 overflow-hidden">
      <Transition name="slide">
        <DayDetail
          v-if="selectedDate"
          :key="selectedDate"
          :date="selectedDate"
          @back="back"
        />
        <WeekGrid
          v-else
          @select-day="selectDay"
        />
      </Transition>
    </div>
  </div>
</template>

<style scoped>
.slide-enter-active,
.slide-leave-active {
  transition: transform 0.28s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.2s ease;
  position: absolute;
  width: 100%;
}
.slide-enter-from {
  transform: translateX(100%);
  opacity: 0;
}
.slide-leave-to {
  transform: translateX(-30%);
  opacity: 0;
}
.slide-leave-from,
.slide-enter-to {
  transform: translateX(0);
  opacity: 1;
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
