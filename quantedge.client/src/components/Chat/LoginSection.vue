<template>
  <div class="login-section">
    <h3>Your insight starts here. Claim your QuantEdge.</h3>
    <input 
      v-model="localUsername"
      type="text"
      placeholder="Enter your username"
      @keyup.enter="handleConnect"
      class="username-input" 
    />
    <button @click="handleConnect" class="connect-btn" :disabled="connecting">
      {{ connecting ? 'Connecting...' : 'Login' }}
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  username: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['connect'])

const localUsername = ref(props.username)
const connecting = ref(false)

const handleConnect = async () => {
  if (!localUsername.value.trim()) {
    alert('Please enter a username')
    return
  }

  connecting.value = true
  emit('connect', localUsername.value)
  connecting.value = false
}
</script>

<style scoped>
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

.connect-btn:hover:not(:disabled) {
  transform: translateY(-2px);
}

.connect-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
</style>
