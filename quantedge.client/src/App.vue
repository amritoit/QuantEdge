<script setup>
  import ChatInterface from './components/ChatInterface.vue'
</script>

<template>
  <header>
    <img alt="Vue logo" class="logo" src="./assets/logo.svg" width="125" height="125" />
    <div class="wrapper">
      <h1>QuantEdge Chat</h1>
    </div>
  </header>

  <main>
    <ChatInterface />
  </main>
</template>

<style scoped>
  header {
    line-height: 1.5;
    text-align: center;
    padding: 2rem 0;
  }

  .logo {
    display: block;
    margin: 0 auto 1rem;
  }

  h1 {
    font-size: 2rem;
    color: #333;
  }

  main {
    padding: 2rem;
  }
</style>
