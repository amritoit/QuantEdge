<template>
  <div class="chat-container" :class="{ 'dark-mode': isDarkMode }">
    <ChatHeader :is-connected="isConnected" :is-dark-mode="isDarkMode" />
    
    <LoginSection 
      v-if="!isConnected" 
      :username="username"
      @connect="handleConnect" 
    />
    
    <div v-else class="chat-main">
      <MessagesContainer
        :messages="messages"
        :current-username="username"
        :is-typing="isTyping"
        :typing-user="typingUser"
      />
      
      <MessageInput
        v-model:message="newMessage"
        @send="handleSendMessage"
        @typing="handleTyping"
        @disconnect="handleDisconnect"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import ChatHeader from './chat/ChatHeader.vue'
import LoginSection from './chat/LoginSection.vue'
import MessagesContainer from './chat/MessagesContainer.vue'
import MessageInput from './chat/MessageInput.vue'
import { useSignalR } from '../composables/useSignalR'
import { useTheme } from '../composables/useTheme'

// Composables
const { 
  isConnected, 
  messages, 
  connectToChat, 
  disconnect, 
  sendMessage, 
  sendTyping,
  setupTypingHandler 
} = useSignalR()

const { isDarkMode, initializeTheme, cleanupTheme } = useTheme()

// Local state
const username = ref('')
const newMessage = ref('')
const isTyping = ref(false)
const typingUser = ref('')

let typingTimeout = null

// Methods
const handleConnect = async (usernameValue) => {
  username.value = usernameValue
  const result = await connectToChat()
  
  if (!result.success) {
    alert(result.error)
    return
  }

  // Setup typing handler after successful connection
  setupTypingHandler((user) => {
    typingUser.value = user
    isTyping.value = true

    clearTimeout(typingTimeout)
    typingTimeout = setTimeout(() => {
      isTyping.value = false
    }, 2000)
  })
}

const handleSendMessage = async (message) => {
  const success = await sendMessage(username.value, message)
  if (success) {
    newMessage.value = ''
  }
}

const handleTyping = async () => {
  await sendTyping(username.value)
}

const handleDisconnect = async () => {
  await disconnect()
  username.value = ''
  newMessage.value = ''
}

// Lifecycle
onMounted(() => {
  initializeTheme()
})

onUnmounted(async () => {
  await handleDisconnect()
  cleanupTheme()
  if (typingTimeout) {
    clearTimeout(typingTimeout)
  }
})
</script>

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

.chat-main {
  display: flex;
  flex-direction: column;
  height: 600px;
  background: var(--bg-primary);
}

/* Responsive Design */
@media (max-width: 768px) {
  .chat-container {
    margin: 1rem;
    border-radius: 8px;
  }
}

/* High Contrast Mode Support */
@media (prefers-contrast: high) {
  .chat-container {
    border-width: 2px;
  }
}

/* Reduced Motion Support */
@media (prefers-reduced-motion: reduce) {
  .chat-container {
    transition: none;
  }
}
</style>