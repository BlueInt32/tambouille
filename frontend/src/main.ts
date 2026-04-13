import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import 'primeicons/primeicons.css'
import App from './App.vue'
import router from './router'

// Apply theme: use localStorage if set, otherwise follow system preference
const storedTheme = localStorage.getItem('theme')
const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
if (storedTheme === 'dark' || (!storedTheme && prefersDark)) {
  document.documentElement.classList.add('dark')
} else {
  document.documentElement.classList.remove('dark')
}

createApp(App).use(createPinia()).use(router).mount('#app')
