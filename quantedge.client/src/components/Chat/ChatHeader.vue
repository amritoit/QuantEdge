<template>
  <div class="chat-header">
    <h2>QuantEdge</h2>
    <div class="header-controls">
      <span class="theme-indicator">{{ isDarkMode ? '🌙' : '☀️' }}</span>
      <span v-if="isConnected" class="status connected">Connected</span>
      <span v-else class="status disconnected">Disconnected</span>
    </div>
  </div>
</template>

<script setup>
  const props = defineProps({
    isConnected: {
      type: Boolean,
      required: true
    },
    isDarkMode: {
      type: Boolean,
      required: true
    }
})
</script>

<style scoped>
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
</style>
