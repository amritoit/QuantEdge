src/components/ChatInterface.vue
<script setup>
  import { ref, onMounted, onUnmounted, nextTick } from 'vue'
  import * as signalR from '@microsoft/signalr'

  import MarkdownIt from 'markdown-it'
  import DOMPurify from 'dompurify'
  import hljs from 'highlight.js'

  // Dynamic theme detection and highlight.js style switching
  const isDarkMode = ref(false)

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

  // create markdown renderer with code highlighting
  const md = new MarkdownIt({
    breaks: true,
    linkify: true,
    highlight: function (str, lang) {
      if (lang && hljs.getLanguage(lang)) {
        try {
          return `<pre class="hljs"><code>${hljs.highlight(str, { language: lang, ignoreIllegals: true }).value}</code></pre>`
        } catch (__) { }
      }
      return `<pre class="hljs"><code>${md.utils.escapeHtml(str)}</code></pre>`
    }
  })

  // helper to turn markdown -> safe HTML
  const renderMarkdown = (text) => {
    const dirty = md.render(text || '')
    return DOMPurify.sanitize(dirty)
  }

  const messages = ref([])
  const newMessage = ref('')
  const username = ref('')
  const isConnected = ref(false)
  const isTyping = ref(false)
  const typingUser = ref('')
  const connection = ref(null)
  const messagesContainer = ref(null)

  let typingTimeout = null
  let themeMediaQuery = null

  const scrollToBottom = () => {
    nextTick(() => {
      if (messagesContainer.value) {
        messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
      }
    })
  }

  const connectToChat = async () => {
    if (!username.value.trim()) {
      alert('Please enter a username')
      return
    }

    try {
      connection.value = new signalR.HubConnectionBuilder()
        .withUrl('/chathub')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build()

      connection.value.on('ReceiveMessage', (message) => {
        messages.value.push(message)
        scrollToBottom()
      })

      connection.value.on('UserTyping', (user) => {
        typingUser.value = user
        isTyping.value = true

        clearTimeout(typingTimeout)
        typingTimeout = setTimeout(() => {
          isTyping.value = false
        }, 2000)
      })

      connection.value.on('UserConnected', (connectionId) => {
        console.log('User connected:', connectionId)
      })

      connection.value.on('UserDisconnected', (connectionId) => {
        console.log('User disconnected:', connectionId)
      })

      connection.value.onreconnecting((error) => {
        console.log('Connection lost, reconnecting...', error)
        isConnected.value = false
      })

      connection.value.onreconnected((connectionId) => {
        console.log('Reconnected with ID:', connectionId)
        isConnected.value = true
      })

      connection.value.onclose((error) => {
        console.log('Connection closed', error)
        isConnected.value = false
      })

      await connection.value.start()
      isConnected.value = true
      console.log('Connected to chat hub successfully!')
    } catch (err) {
      console.error('Connection failed:', err)
      alert(`Failed to connect to chat: ${err.message || 'Unknown error'}`)
    }
  }

  const sendMessage = async () => {
    if (!newMessage.value.trim() || !isConnected.value) return

    try {
      await connection.value.invoke('SendMessage', username.value, newMessage.value)
      newMessage.value = ''
    } catch (err) {
      console.error('Error sending message:', err)
    }
  }

  const handleTyping = async () => {
    if (!isConnected.value) return

    try {
      await connection.value.invoke('UserTyping', username.value)
    } catch (err) {
      console.error('Error sending typing indicator:', err)
    }
  }

  const formatTime = (timestamp) => {
    const date = new Date(timestamp)
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const disconnect = async () => {
    if (connection.value) {
      await connection.value.stop()
      isConnected.value = false
      messages.value = []
      newMessage.value = ''
    }
  }

  onMounted(() => {
    // Initialize theme detection
    updateTheme()

    // Listen for theme changes
    themeMediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    themeMediaQuery.addEventListener('change', updateTheme)
  })

  onUnmounted(async () => {
    await disconnect()

    // Clean up theme listener
    if (themeMediaQuery) {
      themeMediaQuery.removeEventListener('change', updateTheme)
    }
  })
</script>

<template>
  <div class="chat-container" :class="{ 'dark-mode': isDarkMode }">
    <div class="chat-header">
      <h2>💬 QuantEdge</h2>
      <div class="header-controls">
        <span class="theme-indicator">{{ isDarkMode ? '🌙' : '☀️' }}</span>
        <span v-if="isConnected" class="status connected">Connected</span>
        <span v-else class="status disconnected">Disconnected</span>
      </div>
    </div>

    <div v-if="!isConnected" class="login-section">
      <h3>Start</h3>
      <input v-model="username"
             type="text"
             placeholder="Enter your username"
             @keyup.enter="connectToChat"
             class="username-input" />
      <button @click="connectToChat" class="connect-btn">
        Join Chat
      </button>
    </div>

    <div v-else class="chat-main">
      <div class="messages-container" ref="messagesContainer">
        <div v-for="(msg, index) in messages"
             :key="index"
             :class="['message', msg.user === username ? 'own-message' : 'other-message']">
          <div class="message-header">
            <span class="message-user">{{ msg.user }}</span>
            <span class="message-time">{{ formatTime(msg.timestamp) }}</span>
          </div>
          <div class="message-text" v-html="renderMarkdown(msg.message)"></div>
        </div>

        <div v-if="isTyping" class="typing-indicator">
          {{ typingUser }} is typing...
        </div>
      </div>

      <div class="input-section">
        <input v-model="newMessage"
               type="text"
               placeholder="Type a message..."
               @keyup.enter="sendMessage"
               @input="handleTyping"
               class="message-input" />
        <button @click="sendMessage" class="send-btn">
          Send
        </button>
        <button @click="disconnect" class="disconnect-btn">
          Disconnect
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
  /* CSS Custom Properties for Theme Support */
  .chat-container {
    --bg-primary: #ffffff;
    --bg-secondary: #f9fafb;
    --bg-tertiary: #ffffff;
    --text-primary: #000000;
    --text-secondary: #333333;
    --text-muted: #6b7280;
    --border-color: #ddd;
    --border-light: #e5e7eb;
    --shadow-color: rgba(0, 0, 0, 0.1);
    --message-bg: #ffffff;
    --message-border: #e5e7eb;
    --input-bg: #ffffff;
    --input-border: #ddd;
  }

    /* Dark Mode Variables */
    .chat-container.dark-mode {
      --bg-primary: #1f2937;
      --bg-secondary: #111827;
      --bg-tertiary: #374151;
      --text-primary: #f9fafb;
      --text-secondary: #e5e7eb;
      --text-muted: #9ca3af;
      --border-color: #4b5563;
      --border-light: #374151;
      --shadow-color: rgba(0, 0, 0, 0.3);
      --message-bg: #374151;
      --message-border: #4b5563;
      --input-bg: #374151;
      --input-border: #4b5563;
    }

  .chat-container {
    max-width: 800px;
    margin: 2rem auto;
    border: 1px solid var(--border-color);
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 4px 6px var(--shadow-color);
    background: var(--bg-primary);
    transition: all 0.3s ease;
  }

  .chat-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 1.5rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

    .chat-header h2 {
      margin: 0;
      font-size: 1.5rem;
    }

  .header-controls {
    display: flex;
    align-items: center;
    gap: 1rem;
  }

  .theme-indicator {
    font-size: 1.2rem;
    opacity: 0.8;
  }

  .status {
    padding: 0.5rem 1rem;
    border-radius: 20px;
    font-size: 0.875rem;
    font-weight: bold;
  }

    .status.connected {
      background: #10b981;
    }

    .status.disconnected {
      background: #ef4444;
    }

  .login-section {
    padding: 3rem;
    text-align: center;
    background: var(--bg-primary);
  }

    .login-section h3 {
      margin-bottom: 1.5rem;
      color: var(--text-secondary);
    }

  .username-input {
    width: 100%;
    max-width: 300px;
    padding: 0.75rem;
    margin-bottom: 1rem;
    border: 2px solid var(--input-border);
    border-radius: 8px;
    font-size: 1rem;
    background: var(--input-bg);
    color: var(--text-primary);
    transition: border-color 0.3s, background-color 0.3s;
  }

    .username-input:focus {
      outline: none;
      border-color: #667eea;
    }

    .username-input::placeholder {
      color: var(--text-muted);
    }

  .connect-btn {
    padding: 0.75rem 2rem;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 1rem;
    font-weight: bold;
    cursor: pointer;
    transition: transform 0.2s;
  }

    .connect-btn:hover {
      transform: translateY(-2px);
    }

  .chat-main {
    display: flex;
    flex-direction: column;
    height: 600px;
    background: var(--bg-primary);
  }

  .messages-container {
    flex: 1;
    padding: 1.5rem;
    overflow-y: auto;
    background: var(--bg-secondary);
    transition: background-color 0.3s;
  }

  .message {
    margin-bottom: 1rem;
    padding: 0.75rem;
    border-radius: 8px;
    max-width: 70%;
    animation: slideIn 0.3s ease;
    transition: all 0.3s ease;
  }

  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateY(10px);
    }

    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .own-message {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    margin-left: auto;
  }

  .other-message {
    background: var(--message-bg);
    border: 1px solid var(--message-border);
    color: var(--text-primary);
  }

  .message-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.5rem;
    font-size: 0.75rem;
    opacity: 0.8;
  }

  .message-user {
    font-weight: bold;
    color: inherit;
  }

  .message-time {
    font-size: 0.7rem;
    color: inherit;
  }

  .message-text {
    font-size: 0.95rem;
    word-wrap: break-word;
    color: inherit;
  }

  /* Dark mode specific styles for message text content */
  .dark-mode .message-text :deep(h1),
  .dark-mode .message-text :deep(h2),
  .dark-mode .message-text :deep(h3),
  .dark-mode .message-text :deep(h4),
  .dark-mode .message-text :deep(h5),
  .dark-mode .message-text :deep(h6) {
    color: var(--text-primary);
  }

  .dark-mode .message-text :deep(p) {
    color: var(--text-primary);
  }

  .dark-mode .message-text :deep(ul),
  .dark-mode .message-text :deep(ol),
  .dark-mode .message-text :deep(li) {
    color: var(--text-primary);
  }

  .dark-mode .message-text :deep(a) {
    color: #60a5fa;
  }

  .dark-mode .message-text :deep(a:hover) {
    color: #93c5fd;
  }

  .dark-mode .message-text :deep(code) {
    background: rgba(255, 255, 255, 0.1);
    color: #f9fafb;
  }

  .dark-mode .message-text :deep(blockquote) {
    border-left: 4px solid var(--border-color);
    background: rgba(255, 255, 255, 0.05);
    color: var(--text-secondary);
  }

  /* Code blocks in dark mode */
  .dark-mode .message-text pre.hljs {
    background: #0d1117 !important;
    border: 1px solid var(--border-color);
  }

  .message-text pre.hljs {
    padding: 0.75rem;
    border-radius: 8px;
    overflow-x: auto;
    white-space: pre-wrap;
    word-break: break-word;
    max-width: 100%;
    box-sizing: border-box;
    transition: all 0.3s ease;
  }

    .message-text pre.hljs code {
      display: block;
      white-space: pre-wrap;
      word-break: break-word;
    }

  .typing-indicator {
    padding: 0.5rem;
    font-style: italic;
    color: var(--text-muted);
    font-size: 0.875rem;
  }

  .input-section {
    display: flex;
    padding: 1rem;
    background: var(--bg-tertiary);
    border-top: 1px solid var(--border-light);
    gap: 0.5rem;
    transition: all 0.3s ease;
  }

  .message-input {
    flex: 1;
    padding: 0.75rem;
    border: 2px solid var(--input-border);
    border-radius: 8px;
    font-size: 1rem;
    background: var(--input-bg);
    color: var(--text-primary);
    transition: border-color 0.3s, background-color 0.3s;
  }

    .message-input:focus {
      outline: none;
      border-color: #667eea;
    }

    .message-input::placeholder {
      color: var(--text-muted);
    }

  .send-btn {
    padding: 0.75rem 1.5rem;
    background: #10b981;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    cursor: pointer;
    transition: background 0.3s, transform 0.2s;
  }

    .send-btn:hover {
      background: #059669;
      transform: translateY(-1px);
    }

  .disconnect-btn {
    padding: 0.75rem 1rem;
    background: #ef4444;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    cursor: pointer;
    transition: background 0.3s, transform 0.2s;
  }

    .disconnect-btn:hover {
      background: #dc2626;
      transform: translateY(-1px);
    }

  /* Responsive Design */
  @media (max-width: 768px) {
    .chat-container {
      margin: 1rem;
      border-radius: 8px;
    }

    .message {
      max-width: 85%;
    }

    .header-controls {
      gap: 0.5rem;
    }
  }

  /* High Contrast Mode Support */
  @media (prefers-contrast: high) {
    .chat-container {
      border-width: 2px;
    }

    .message {
      border-width: 2px;
    }

    .username-input,
    .message-input {
      border-width: 3px;
    }
  }

  /* Reduced Motion Support */
  @media (prefers-reduced-motion: reduce) {
    .chat-container,
    .message,
    .username-input,
    .message-input,
    .send-btn,
    .disconnect-btn,
    .connect-btn {
      transition: none;
    }

    .message {
      animation: none;
    }
  }
</style>
