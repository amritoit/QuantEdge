<template>
  <div class="input-section">
    <input 
      v-model="localMessage"
      type="text"
      placeholder="Type a message..."
      @keyup.enter="handleSend"
      @input="handleTyping"
      class="message-input" 
    />
    <button @click="handleSend" class="send-btn" :disabled="!localMessage.trim()">
      Send
    </button>
    <button @click="handleDisconnect" class="disconnect-btn">
      Disconnect
    </button>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  message: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['send', 'typing', 'disconnect', 'update:message'])

const localMessage = ref(props.message)

watch(() => props.message, (newValue) => {
  localMessage.value = newValue
})

const handleSend = () => {
  if (localMessage.value.trim()) {
    emit('send', localMessage.value)
    localMessage.value = ''
    emit('update:message', '')
  }
}

const handleTyping = () => {
  emit('update:message', localMessage.value)
  emit('typing')
}

const handleDisconnect = () => {
  emit('disconnect')
}
</script>

<style scoped>
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

.send-btn:hover:not(:disabled) {
  background: #059669;
  transform: translateY(-1px);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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
</style>