<template>
  <div :class="['message', messageTypeClass]">
    <div class="message-header">
      <span class="message-user">{{ message.user }}</span>
      <span class="message-time">{{ formatTime(message.timestamp) }}</span>
    </div>
    <div class="message-text" v-html="renderMarkdown(message.message)"></div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useMarkdown } from '../../composables/useMarkdown'

const props = defineProps({
  message: {
    type: Object,
    required: true
  },
  currentUsername: {
    type: String,
    required: true
  }
})

const { renderMarkdown } = useMarkdown()

const messageTypeClass = computed(() => {
  if (props.message.type === 'System' || props.message.user === 'System') {
    return 'system-message'
  } else if (props.message.type === 'Bot' || props.message.user === 'ChatBot') {
    return 'bot-message'
  } else if (props.message.user === props.currentUsername) {
    return 'own-message'
  } else {
    return 'other-message'
  }
})

const formatTime = (timestamp) => {
  const date = new Date(timestamp)
  return date.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
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

.system-message {
  background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
  color: #92400e;
  border: 2px solid #f59e0b;
  border-radius: 12px;
  text-align: center;
  font-size: 0.9rem;
  max-width: 90%;
  margin: 0 auto;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.2);
}

.bot-message {
  background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
  color: #1e3a8a;
  border: 2px solid #60a5fa;
  border-bottom-left-radius: 4px;
  box-shadow: 0 4px 12px rgba(96, 165, 250, 0.2);
  max-width: 85%;
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

/* Code block styling */
.message-text :deep(pre.hljs) {
  padding: 0.75rem;
  border-radius: 8px;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-word;
  max-width: 100%;
  box-sizing: border-box;
}
</style>