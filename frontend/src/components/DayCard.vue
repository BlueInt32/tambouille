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
    class="bg-surface-card rounded-2xl border-l-4 shadow-sm cursor-pointer
           active:scale-[0.99] transition-all duration-150 overflow-hidden"
    :class="isToday ? 'border-l-primary' : 'border-l-amber-200'"
  >
    <div class="flex items-start gap-3 p-4">
      <!-- Date -->
      <div
        class="flex-shrink-0 w-11 h-11 rounded-xl flex flex-col items-center justify-center"
        :class="isToday ? 'bg-primary text-white' : 'bg-amber-50 text-text'"
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
            class="text-xs font-sans text-text-muted bg-amber-50 px-2 py-0.5 rounded-full"
          >
            {{ meals.length }} repas
          </span>
        </div>

        <!-- Repas -->
        <div v-if="meals.length > 0" class="mt-1.5 flex flex-col gap-1">
          <div
            v-for="meal in meals"
            :key="meal.id"
            class="flex items-center gap-2 text-sm"
          >
            <span class="text-xs font-sans text-text-muted w-8 flex-shrink-0">{{ SLOT_LABELS[meal.slot] }}</span>
            <span class="font-sans text-text truncate">{{ meal.name }}</span>
            <span class="ml-auto text-xs text-text-muted flex-shrink-0 flex items-center gap-0.5">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-3.5 h-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
              </svg>
              {{ meal.persons }}
            </span>
          </div>
        </div>
        <p v-else class="mt-1 text-sm font-sans text-text-muted/60 italic">Aucun repas prévu</p>
      </div>

      <!-- Flèche -->
      <div class="flex-shrink-0 self-center text-text-muted/30">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-4 h-4">
          <path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
        </svg>
      </div>
    </div>
  </div>
</template>
