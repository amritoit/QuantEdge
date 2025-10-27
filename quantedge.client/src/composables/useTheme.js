import { ref, onMounted, onUnmounted } from 'vue'

export function useTheme() {
  const isDarkMode = ref(false)
  let themeMediaQuery = null

  const updateTheme = () => {
    isDarkMode.value = window.matchMedia('(prefers-color-scheme: dark)').matches

    // Dynamically load appropriate highlight.js theme
    const existingLink = document.querySelector('link[data-hljs-theme]')
    if (existingLink) {
      existingLink.remove()
    }

    const link = document.createElement('link')
    link.rel = 'stylesheet'
    link.setAttribute('data-hljs-theme', '')
    link.href = isDarkMode.value
      ? 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css'
      : 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css'
    document.head.appendChild(link)
  }

  const initializeTheme = () => {
    updateTheme()
    themeMediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    themeMediaQuery.addEventListener('change', updateTheme)
  }

  const cleanupTheme = () => {
    if (themeMediaQuery) {
      themeMediaQuery.removeEventListener('change', updateTheme)
    }
  }

  return {
    isDarkMode,
    initializeTheme,
    cleanupTheme
  }
}