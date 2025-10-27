import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'

export function useSignalR() {
  const connection = ref(null)
  const isConnected = ref(false)
  const messages = ref([])

  const connectToChat = async () => {
    try {
      connection.value = new signalR.HubConnectionBuilder()
        .withUrl('/chathub')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Set up event handlers
      connection.value.on('ReceiveMessage', (message) => {
        messages.value.push(message)
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
      
      return { success: true }
    } catch (err) {
      console.error('Connection failed:', err)
      return { 
        success: false, 
        error: `Failed to connect to chat: ${err.message || 'Unknown error'}` 
      }
    }
  }

  const disconnect = async () => {
    if (connection.value) {
      await connection.value.stop()
      isConnected.value = false
      messages.value = []
    }
  }

  const sendMessage = async (username, message) => {
    if (!message.trim() || !isConnected.value) return false

    try {
      await connection.value.invoke('SendMessage', username, message)
      return true
    } catch (err) {
      console.error('Error sending message:', err)
      return false
    }
  }

  const sendTyping = async (username) => {
    if (!isConnected.value) return

    try {
      await connection.value.invoke('UserTyping', username)
    } catch (err) {
      console.error('Error sending typing indicator:', err)
    }
  }

  const setupTypingHandler = (callback) => {
    if (connection.value) {
      connection.value.on('UserTyping', callback)
    }
  }

  return {
    connection,
    isConnected,
    messages,
    connectToChat,
    disconnect,
    sendMessage,
    sendTyping,
    setupTypingHandler
  }
}