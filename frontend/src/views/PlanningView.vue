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
  <div class="h-dvh flex flex-col bg-surface font-sans overflow-hidden">
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
          <i class="pi pi-chevron-left text-sm"></i>
          Semaine
        </button>
        <span v-else key="title" class="font-display text-text text-lg font-semibold">Tambouille</span>
      </Transition>

      <button
        @click="logout"
        class="text-text-muted/60 hover:text-text-muted transition-colors p-1.5 rounded-lg hover:bg-amber-50"
        title="Déconnexion"
      >
        <i class="pi pi-sign-out text-base"></i>
      </button>
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
