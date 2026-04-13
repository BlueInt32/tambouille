import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import 'primeicons/primeicons.css'
import App from './App.vue'
import router from './router'

// Apply theme from localStorage; default to light
if (localStorage.getItem('theme') === 'dark') {
  document.documentElement.classList.add('dark')
} else {
  document.documentElement.classList.remove('dark')
}

createApp(App).use(createPinia()).use(router).mount('#app')
