<script setup lang="ts">
import type { Meal } from '@/types'

const props = defineProps<{
  date: Date
  meals: Meal[]
}>()

const DAYS_FR = ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi']
const MONTHS_FR = ['jan', 'fév', 'mar', 'avr', 'mai', 'juin',
                   'juil', 'août', 'sep', 'oct', 'nov', 'déc']
const SLOT_LABELS = ['Midi', 'Soir']

const dayName = DAYS_FR[props.date.getDay()]
const dayNum = props.date.getDate()
const monthName = MONTHS_FR[props.date.getMonth()]

const isToday = (() => {
  const today = new Date()
  return props.date.getDate() === today.getDate()
    && props.date.getMonth() === today.getMonth()
    && props.date.getFullYear() === today.getFullYear()
})()
</script>

<template>
  <div
    class="bg-surface-card rounded-xl border-l-4 shadow-sm cursor-pointer
           active:scale-[0.99] transition-all duration-150"
    :class="isToday ? 'border-l-primary' : 'border-l-amber-200 dark:border-l-amber-800/50'"
  >
    <div class="flex items-center gap-2.5 px-3 py-2.5">
      <!-- Date -->
      <div
        class="flex-shrink-0 w-10 h-10 rounded-xl flex flex-col items-center justify-center"
        :class="isToday ? 'bg-primary text-white' : 'bg-amber-50 dark:bg-amber-900/20 text-text'"
      >
        <span class="font-display font-bold text-base leading-none">{{ dayNum }}</span>
        <span class="font-sans text-[10px] uppercase tracking-wide mt-0.5 opacity-70">{{ monthName }}</span>
      </div>

      <!-- Contenu -->
      <div class="flex-1 min-w-0">
        <div class="flex items-center justify-between">
          <span class="font-display font-semibold text-text text-sm">{{ dayName }}</span>
          <span
            v-if="meals.length > 0"
            class="text-xs font-sans text-text-muted bg-amber-50 dark:bg-amber-900/20 px-1.5 py-0.5 rounded-full"
          >
            {{ meals.length }} repas
          </span>
        </div>

        <!-- Repas -->
        <div v-if="meals.length > 0" class="mt-1 flex flex-col gap-0.5">
          <div
            v-for="meal in meals"
            :key="meal.id"
            class="flex items-center gap-2 text-xs"
          >
            <span class="font-sans text-text-muted w-7 flex-shrink-0">{{ SLOT_LABELS[meal.slot] }}</span>
            <span class="font-sans text-text truncate">{{ meal.name }}</span>
            <span class="ml-auto text-text-muted flex-shrink-0 flex items-center gap-0.5">
              <i class="pi pi-user text-[10px]"></i>
              {{ meal.persons }}
            </span>
          </div>
        </div>
        <p v-else class="mt-0.5 text-xs font-sans text-text-muted/60 italic">Aucun repas prévu</p>
      </div>

      <!-- Flèche -->
      <div class="flex-shrink-0 self-center text-text-muted/30">
        <i class="pi pi-chevron-right text-xs"></i>
      </div>
    </div>
  </div>
</template>
