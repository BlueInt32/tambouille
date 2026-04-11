<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePlanningStore } from '@/stores/planning'
import WeekGrid from '@/components/WeekGrid.vue'
import DayDetail from '@/components/DayDetail.vue'

const router = useRouter()
const store = usePlanningStore()
const selectedDate = ref<string | null>(null)

onMounted(() => {
  store.fetchWeek()
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
  <div class="min-h-svh bg-surface font-sans overflow-x-hidden">
    <!-- Barre de titre -->
    <header class="bg-surface-card border-b border-amber-100 px-4 py-3 flex items-center justify-between sticky top-0 z-20 shadow-sm">
      <Transition name="fade-fast" mode="out-in">
        <button
          v-if="selectedDate"
          key="back"
          @click="back"
          class="flex items-center gap-1.5 text-primary font-semibold text-sm py-1 px-2 -ml-2 rounded-lg
                 hover:bg-primary/10 active:bg-primary/20 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-4 h-4">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
          </svg>
          Semaine
        </button>
        <span v-else key="title" class="font-display text-text text-lg font-semibold">Tambouille</span>
      </Transition>

      <button
        @click="logout"
        class="text-text-muted/60 hover:text-text-muted transition-colors p-1.5 rounded-lg hover:bg-amber-50"
        title="Déconnexion"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15M12 9l-3 3m0 0 3 3m-3-3h12.75" />
        </svg>
      </button>
    </header>

    <!-- Contenu principal avec transition slide -->
    <div class="relative">
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
