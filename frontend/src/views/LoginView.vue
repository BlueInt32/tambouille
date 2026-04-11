<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/services/api'

const router = useRouter()
const password = ref('')
const error = ref('')
const loading = ref(false)

async function login() {
  if (!password.value || loading.value) return
  loading.value = true
  error.value = ''
  try {
    const { data } = await authApi.login(password.value)
    localStorage.setItem('token', data.token)
    router.push('/')
  } catch {
    error.value = 'Mot de passe incorrect'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-svh bg-surface flex flex-col">
    <!-- Header décoratif -->
    <div class="bg-primary px-6 pt-16 pb-12 text-center relative overflow-hidden">
      <!-- Cercles décoratifs -->
      <div class="absolute -top-8 -right-8 w-40 h-40 rounded-full bg-primary-light opacity-30"></div>
      <div class="absolute -bottom-6 -left-6 w-28 h-28 rounded-full bg-primary-dark opacity-20"></div>

      <p class="font-display text-white/70 text-sm italic tracking-wide mb-1">Bienvenue dans votre cuisine</p>
      <h1 class="font-display text-white text-5xl font-bold tracking-tight leading-none">
        Tambouille
      </h1>
      <p class="mt-3 text-white/60 font-sans text-sm">Qu'est-ce qu'on mange cette semaine ?</p>
    </div>

    <!-- Formulaire -->
    <div class="flex-1 flex flex-col items-center px-6 pt-10">
      <div class="w-full max-w-sm">
        <div class="bg-surface-card rounded-2xl shadow-md p-6">
          <h2 class="font-display text-text text-xl font-semibold mb-1">Accès privé</h2>
          <p class="text-text-muted text-sm font-sans mb-6">Entrez votre mot de passe pour continuer.</p>

          <form @submit.prevent="login" class="flex flex-col gap-4">
            <div>
              <input
                v-model="password"
                type="password"
                placeholder="Mot de passe"
                autocomplete="current-password"
                class="w-full px-4 py-3 rounded-xl border border-amber-200 bg-surface font-sans text-text
                       placeholder:text-text-muted/50 focus:outline-none focus:ring-2 focus:ring-primary
                       focus:border-transparent transition-all text-base"
                :class="{ 'border-red-300 focus:ring-red-400': error }"
              />
              <p v-if="error" class="mt-2 text-red-500 text-sm font-sans">{{ error }}</p>
            </div>

            <button
              type="submit"
              :disabled="loading || !password"
              class="w-full py-3.5 rounded-xl bg-primary text-white font-sans font-semibold text-base
                     hover:bg-primary-dark active:scale-95 transition-all duration-150 shadow-sm
                     disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="loading" class="inline-flex items-center gap-2">
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                </svg>
                Connexion…
              </span>
              <span v-else>Entrer</span>
            </button>
          </form>
        </div>

        <!-- Déco bas de page -->
        <p class="text-center text-text-muted/40 text-xs font-sans mt-8">
          🍳 Bon appétit
        </p>
      </div>
    </div>
  </div>
</template>
