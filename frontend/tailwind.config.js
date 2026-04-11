/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      fontFamily: {
        display: ['Fraunces', 'Georgia', 'serif'],
        sans: ['DM Sans', 'system-ui', 'sans-serif'],
      },
      colors: {
        primary: { DEFAULT: '#ea7c1e', light: '#f5a84e', dark: '#c45e0a' },
        accent:  { DEFAULT: '#4a7c59', light: '#6aac7a' },
        surface: { DEFAULT: '#fdf6ee', card: '#fff8f0' },
        text:    { DEFAULT: '#2d1f0e', muted: '#8a6a4a' },
      },
    },
  },
  plugins: [],
}
