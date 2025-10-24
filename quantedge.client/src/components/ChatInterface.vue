src/components/ChatInterface.vue
<script setup>
  import { ref, onMounted, onUnmounted, nextTick } from 'vue'
  import * as signalR from '@microsoft/signalr'

  const messages = ref([])
  const newMessage = ref('')
  const username = ref('')
  const isConnected = ref(false)
  const isTyping = ref(false)
  const typingUser = ref('')
  const connection = ref(null)
  const messagesContainer = ref(null)

  let typingTimeout = null

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
        .withUrl('/chathub')  // Changed from absolute to relative URL
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)  // Added for debugging
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

  onUnmounted(async () => {
    await disconnect()
  })
</script>

<template>
  <div class="chat-container">
    <div class="chat-header">
      <h2>💬 Chat Interface</h2>
      <span v-if="isConnected" class="status connected">Connected</span>
      <span v-else class="status disconnected">Disconnected</span>
    </div>

    <div v-if="!isConnected" class="login-section">
      <h3>Enter the Chat</h3>
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
          <div class="message-text">{{ msg.message }}</div>
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
  .chat-container {
    max-width: 800px;
    margin: 2rem auto;
    border: 1px solid #ddd;
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    background: white;
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
  }

    .login-section h3 {
      margin-bottom: 1.5rem;
      color: #333;
    }

  .username-input {
    width: 100%;
    max-width: 300px;
    padding: 0.75rem;
    margin-bottom: 1rem;
    border: 2px solid #ddd;
    border-radius: 8px;
    font-size: 1rem;
    transition: border-color 0.3s;
  }

    .username-input:focus {
      outline: none;
      border-color: #667eea;
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
  }

  .messages-container {
    flex: 1;
    padding: 1.5rem;
    overflow-y: auto;
    background: #f9fafb;
  }

  .message {
    margin-bottom: 1rem;
    padding: 0.75rem;
    border-radius: 8px;
    max-width: 70%;
    animation: slideIn 0.3s ease;
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
    background: white;
    border: 1px solid #e5e7eb;
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
  }

  .message-time {
    font-size: 0.7rem;
  }

  .message-text {
    font-size: 0.95rem;
    word-wrap: break-word;
  }

  .typing-indicator {
    padding: 0.5rem;
    font-style: italic;
    color: #6b7280;
    font-size: 0.875rem;
  }

  .input-section {
    display: flex;
    padding: 1rem;
    background: white;
    border-top: 1px solid #e5e7eb;
    gap: 0.5rem;
  }

  .message-input {
    flex: 1;
    padding: 0.75rem;
    border: 2px solid #ddd;
    border-radius: 8px;
    font-size: 1rem;
    transition: border-color 0.3s;
  }

    .message-input:focus {
      outline: none;
      border-color: #667eea;
    }

  .send-btn {
    padding: 0.75rem 1.5rem;
    background: #10b981;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    cursor: pointer;
    transition: background 0.3s;
  }

    .send-btn:hover {
      background: #059669;
    }

  .disconnect-btn {
    padding: 0.75rem 1rem;
    background: #ef4444;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    cursor: pointer;
    transition: background 0.3s;
  }

    .disconnect-btn:hover {
      background: #dc2626;
    }
</style>
