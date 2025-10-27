<template>
  <div class="messages-container" ref="messagesContainer">
    <ChatMessage
      v-for="(message, index) in messages"
      :key="index"
      :message="message"
      :current-username="currentUsername"
    />
    
    <TypingIndicator v-if="isTyping" :typing-user="typingUser" />
  </div>
</template>

<script setup>
import { ref, nextTick, watch } from 'vue'
import ChatMessage from './ChatMessage.vue'
import TypingIndicator from './TypingIndicator.vue'

const props = defineProps({
  messages: {
    type: Array,
    required: true
  },
  currentUsername: {
    type: String,
    required: true
  },
  isTyping: {
    type: Boolean,
    default: false
  },
  typingUser: {
    type: String,
    default: ''
  }
})

const messagesContainer = ref(null)

const scrollToBottom = () => {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
    }
  })
}

// Watch for new messages and scroll to bottom
watch(() => props.messages.length, () => {
  scrollToBottom()
})

// Watch for typing changes and scroll to bottom
watch(() => props.isTyping, () => {
  if (props.isTyping) {
    scrollToBottom()
  }
})
</script>

<style scoped>
.messages-container {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
  background: var(--bg-secondary);
  transition: background-color 0.3s;
}
</style>