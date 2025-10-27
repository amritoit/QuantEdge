/TypingIndicator.vue
<template>
  <div class="typing-indicator">
    <div class="typing-bubble">
      <span class="typing-dots">
        <span></span>
        <span></span>
        <span></span>
      </span>
      <span class="typing-text">{{ typingUser }} is typing...</span>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  typingUser: {
    type: String,
    required: true
  }
})
</script>

<style scoped>
.typing-indicator {
  display: flex;
  justify-content: flex-start;
  margin-bottom: 1rem;
}

.typing-bubble {
  background: linear-gradient(135deg, #f3f4f6 0%, #e5e7eb 100%);
  border: 1px solid #d1d5db;
  border-radius: 18px;
  border-bottom-left-radius: 4px;
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.typing-dots {
  display: flex;
  gap: 0.2rem;
}

.typing-dots span {
  width: 6px;
  height: 6px;
  background: #6b7280;
  border-radius: 50%;
  animation: typing 1.4s infinite ease-in-out;
}

.typing-dots span:nth-child(2) {
  animation-delay: 0.2s;
}

.typing-dots span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes typing {
  0%, 60%, 100% {
    transform: scale(0.8);
    opacity: 0.5;
  }
  30% {
    transform: scale(1);
    opacity: 1;
  }
}

.typing-text {
  font-style: italic;
  color: #6b7280;
  font-size: 0.875rem;
}
</style>