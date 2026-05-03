<script setup lang="ts">
import MealForm from "@/components/MealForm.vue";
import { usePlanningStore } from "@/stores/planning";
import type { Meal } from "@/types";
import { computed, ref } from "vue";

const DAYS_SHORT = ["Di", "Lu", "Ma", "Me", "Je", "Ve", "Sa"];

const props = defineProps<{
  date: string;
}>();

const emit = defineEmits<{ back: [] }>();

const store = usePlanningStore();


const DAYS_FR = [
  "Dimanche",
  "Lundi",
  "Mardi",
  "Mercredi",
  "Jeudi",
  "Vendredi",
  "Samedi",
];
const MONTHS_FR = [
  "janvier",
  "février",
  "mars",
  "avril",
  "mai",
  "juin",
  "juillet",
  "août",
  "septembre",
  "octobre",
  "novembre",
  "décembre",
];
const SLOT_LABELS = ["Midi", "Soir"];

const dateObj = new Date(props.date + "T12:00:00");
const dayLabel = computed(() => {
  return `${DAYS_FR[dateObj.getDay()]} ${dateObj.getDate()} ${MONTHS_FR[dateObj.getMonth()]}`;
});

const meals = computed(() => store.mealsForDay(props.date));

// Refs des cards par slot (tableau plain, pas réactif)
const cardEls: (HTMLElement | null)[] = [null, null];

function scrollToCardAfterTransition(slot: number) {
  // On attend la fin de la transition d'expansion (150ms) avant de scroller
  setTimeout(() => {
    cardEls[slot]?.scrollIntoView({ behavior: "smooth", block: "nearest" });
  }, 160);
}

// État du formulaire
const editingMeal = ref<Meal | null>(null);
const addingSlot = ref<number | null>(null);

function toggleAddSlot(slot: number) {
  addingSlot.value = addingSlot.value === slot ? null : slot;
  if (addingSlot.value === slot) {
    scrollToCardAfterTransition(slot);
  }
}

const deletingId = ref<number | null>(null);

// État du déplacement
const movingMealId = ref<number | null>(null);
const isMoving = ref(false);

const movingMeal = computed(() =>
  movingMealId.value !== null
    ? (store.meals.find((m) => m.id === movingMealId.value) ?? null)
    : null
);

function dateStr(date: Date): string {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

function mealAtSlot(targetDate: string, targetSlot: number): Meal | undefined {
  return store.meals.find(
    (m) => m.date === targetDate && m.slot === targetSlot
  );
}

function isSourceSlot(targetDate: string, targetSlot: number): boolean {
  return (
    movingMeal.value?.date === targetDate &&
    movingMeal.value?.slot === targetSlot
  );
}

function slotClass(targetDate: string, targetSlot: number): string {
  if (isSourceSlot(targetDate, targetSlot)) return "bg-primary/20 text-primary";
  if (mealAtSlot(targetDate, targetSlot))
    return "bg-amber-200 dark:bg-amber-700/50 text-text active:scale-95";
  return "bg-amber-50 dark:bg-amber-900/20 text-text-muted/40 active:scale-95 active:bg-amber-100 dark:active:bg-amber-800/40";
}

function toggleMove(mealId: number) {
  if (movingMealId.value === mealId) {
    movingMealId.value = null;
  } else {
    editingMeal.value = null;
    addingSlot.value = null;
    movingMealId.value = mealId;
  }
}

async function onMoveClick(targetDate: string, targetSlot: number) {
  if (
    !movingMeal.value ||
    isMoving.value ||
    isSourceSlot(targetDate, targetSlot)
  )
    return;
  isMoving.value = true;
  try {
    await store.moveMeal(movingMeal.value.id, targetDate, targetSlot);
    movingMealId.value = null;
  } catch {
    // laisser le picker ouvert, l'utilisateur peut réessayer
  } finally {
    isMoving.value = false;
  }
}

function startEdit(meal: Meal) {
  addingSlot.value = null;
  movingMealId.value = null;
  editingMeal.value = meal;
  scrollToCardAfterTransition(meal.slot);
}

function closeForm() {
  editingMeal.value = null;
  addingSlot.value = null;
}

async function onSaved() {
  closeForm();
}

async function deleteMeal(id: number) {
  deletingId.value = id;
  try {
    await store.deleteMeal(id);
  } finally {
    deletingId.value = null;
  }
}
</script>

<template>
  <div class="pb-8">
    <!-- En-tête du jour -->
    <div
      class="px-4 py-4 border-b border-amber-100 dark:border-amber-900/30 flex items-start justify-between"
    >
      <div>
        <h2 class="font-display text-text text-2xl font-bold capitalize">
          {{ dayLabel }}
        </h2>
        <p class="font-sans text-text-muted text-sm mt-0.5">
          {{
            meals.length === 0
              ? "Aucun repas prévu"
              : `${meals.length} repas planifié${meals.length > 1 ? "s" : ""}`
          }}
        </p>
      </div>
      <button
        @click="emit('back')"
        class="w-8 h-8 rounded-xl flex items-center justify-center text-text-muted hover:bg-amber-100 dark:hover:bg-amber-900/30 active:scale-95 transition-all"
      >
        <i class="pi pi-times text-base"></i>
      </button>
    </div>

    <div class="px-4 mt-4 flex flex-col gap-3">
      <!-- Carte Midi -->
      <div v-for="slot in [0, 1]" :key="slot">
        <div
          :ref="
            (el) => {
              cardEls[slot] = el as HTMLElement | null;
            }
          "
          class="rounded-2xl overflow-hidden bg-surface-card border-t border-l border-b-4 border-r-4 border-t-amber-200 dark:border-t-amber-700/40 border-l-amber-200 dark:border-l-amber-700/40 border-b-amber-300 dark:border-b-amber-700 border-r-amber-300 dark:border-r-amber-700"
        >
          <!-- Slot avec repas -->
          <template v-if="meals.find((m) => m.slot === slot) as Meal">
            <div class="p-4">
              <div class="flex items-start justify-between gap-2">
                <button
                  class="flex-1 min-w-0 text-left"
                  @click="startEdit(meals.find((m) => m.slot === slot)!)"
                >
                  <span
                    class="inline-block text-xs font-sans font-semibold text-primary uppercase tracking-wider mb-1"
                  >
                    {{ SLOT_LABELS[slot] }}
                  </span>
                  <p
                    class="font-display text-text text-lg font-semibold leading-snug"
                  >
                    {{ meals.find((m) => m.slot === slot)!.name }}
                  </p>
                  <p
                    class="font-sans text-text-muted text-sm mt-1 flex items-center gap-1"
                  >
                    <i class="pi pi-user text-sm"></i>
                    {{ meals.find((m) => m.slot === slot)!.persons }} personne{{
                      meals.find((m) => m.slot === slot)!.persons > 1 ? "s" : ""
                    }}
                  </p>
                </button>
                <div class="flex items-center gap-0.5 flex-shrink-0">
                  <button
                    @click.stop="
                      toggleMove(meals.find((m) => m.slot === slot)!.id)
                    "
                    :disabled="isMoving"
                    class="w-8 h-8 rounded-xl flex items-center justify-center transition-all disabled:opacity-40"
                    :class="
                      movingMealId === meals.find((m) => m.slot === slot)!.id
                        ? 'text-primary bg-primary/10'
                        : 'text-text-muted hover:bg-amber-50 dark:hover:bg-amber-900/20 active:scale-95'
                    "
                  >
                    <i class="pi pi-arrows-v text-sm"></i>
                  </button>
                  <button
                    @click="deleteMeal(meals.find((m) => m.slot === slot)!.id)"
                    :disabled="
                      deletingId === meals.find((m) => m.slot === slot)!.id
                    "
                    class="w-8 h-8 rounded-xl flex items-center justify-center text-text-muted hover:bg-red-50 hover:text-red-500 active:scale-95 transition-all disabled:opacity-40"
                  >
                    <i
                      v-if="
                        deletingId !== meals.find((m) => m.slot === slot)!.id
                      "
                      class="pi pi-trash text-sm"
                    ></i>
                    <i v-else class="pi pi-spinner pi-spin text-sm"></i>
                  </button>
                </div>
              </div>

              <!-- Formulaire d'édition inline -->
              <Transition name="expand">
                <div
                  v-if="
                    editingMeal?.id === meals.find((m) => m.slot === slot)?.id
                  "
                  class="overflow-hidden"
                >
                  <div
                    class="min-h-0 px-1 mt-4 pt-4 border-t border-amber-100 dark:border-amber-900/30"
                  >
                    <MealForm
                      :date="date"
                      :meal="editingMeal"
                      :slot="slot"
                      @saved="onSaved"
                      @cancel="closeForm"
                    />
                  </div>
                </div>
              </Transition>

              <!-- Picker de déplacement inline -->
              <Transition name="expand">
                <div
                  v-if="
                    movingMeal?.id === meals.find((m) => m.slot === slot)?.id
                  "
                  class="overflow-hidden"
                >
                  <div
                    class="min-h-0 mt-4 pt-4 border-t border-amber-100 dark:border-amber-900/30"
                  >
                    <p
                      class="text-[10px] font-sans font-semibold text-text-muted uppercase tracking-wider mb-2.5"
                    >
                      Déplacer vers
                    </p>
                    <div
                      class="space-y-1.5"
                      :class="isMoving ? 'opacity-50 pointer-events-none' : ''"
                    >
                      <!-- En-têtes des jours -->
                      <div class="flex gap-1 pl-5">
                        <div
                          v-for="day in store.weekDays"
                          :key="dateStr(day)"
                          class="flex-1 text-center text-[10px] font-sans font-semibold uppercase leading-tight"
                          :class="
                            dateStr(day) === date
                              ? 'text-primary'
                              : 'text-text-muted'
                          "
                        >
                          {{ DAYS_SHORT[day.getDay()] }}
                        </div>
                      </div>
                      <!-- Ligne Midi -->
                      <div class="flex items-center gap-1">
                        <span
                          class="w-5 text-[10px] font-sans font-semibold text-text-muted flex-shrink-0 text-right leading-none"
                          >M</span
                        >
                        <button
                          v-for="day in store.weekDays"
                          :key="dateStr(day) + '-0'"
                          :disabled="isSourceSlot(dateStr(day), 0)"
                          @click="onMoveClick(dateStr(day), 0)"
                          class="flex-1 h-9 rounded-lg text-[10px] font-sans overflow-hidden transition-none"
                          :class="slotClass(dateStr(day), 0)"
                        >
                          <span
                            v-if="isSourceSlot(dateStr(day), 0)"
                            class="text-primary"
                            >●</span
                          >
                          <span
                            v-else-if="mealAtSlot(dateStr(day), 0)"
                            class="block truncate px-0.5 leading-tight"
                            >{{
                              mealAtSlot(dateStr(day), 0)!.name.slice(0, 5)
                            }}</span
                          >
                        </button>
                      </div>
                      <!-- Ligne Soir -->
                      <div class="flex items-center gap-1">
                        <span
                          class="w-5 text-[10px] font-sans font-semibold text-text-muted flex-shrink-0 text-right leading-none"
                          >S</span
                        >
                        <button
                          v-for="day in store.weekDays"
                          :key="dateStr(day) + '-1'"
                          :disabled="isSourceSlot(dateStr(day), 1)"
                          @click="onMoveClick(dateStr(day), 1)"
                          class="flex-1 h-9 rounded-lg text-[10px] font-sans overflow-hidden transition-none"
                          :class="slotClass(dateStr(day), 1)"
                        >
                          <span
                            v-if="isSourceSlot(dateStr(day), 1)"
                            class="text-primary"
                            >●</span
                          >
                          <span
                            v-else-if="mealAtSlot(dateStr(day), 1)"
                            class="block truncate px-0.5 leading-tight"
                            >{{
                              mealAtSlot(dateStr(day), 1)!.name.slice(0, 5)
                            }}</span
                          >
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </Transition>
            </div>
          </template>

          <!-- Slot vide -->
          <template v-else>
            <button
              @click="toggleAddSlot(slot)"
              class="w-full p-4 flex items-center gap-3 text-left cursor-pointer"
            >
              <span
                class="text-xs font-sans font-semibold text-text-muted uppercase tracking-wider w-8"
                >{{ SLOT_LABELS[slot] }}</span
              >
              <span class="font-sans text-text-muted text-sm italic"
                >Ajouter un repas…</span
              >
            </button>

            <Transition name="expand">
              <div v-if="addingSlot === slot" class="overflow-hidden">
                <div class="min-h-0 px-4 pb-4">
                  <MealForm
                    :date="date"
                    :meal="null"
                    :slot="slot"
                    @saved="onSaved"
                    @cancel="closeForm"
                  />
                </div>
              </div>
            </Transition>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.expand-enter-active {
  display: grid;
  transition:
    grid-template-rows 0.15s ease-out,
    opacity 0.12s ease-out;
}
.expand-leave-active {
  display: grid;
  transition:
    grid-template-rows 0.12s ease-in,
    opacity 0.1s ease-in;
}
.expand-enter-from,
.expand-leave-to {
  grid-template-rows: 0fr;
  opacity: 0;
}
.expand-enter-to,
.expand-leave-from {
  grid-template-rows: 1fr;
}
</style>
