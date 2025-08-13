const { createApp, ref, onMounted } = Vue

window.mountPostsPage = function() {
  const app = createApp({
    setup() {
      const isLoading = ref(true)

      // 預留：若未來有 /api/Db_PostsApi 可在此呼叫
      const init = async () => {
        try {
          // await fetch('/api/Db_PostsApi')
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      return { isLoading }
    }
  })

  if (window.PostsApp && typeof window.PostsApp.unmount === 'function') {
    try { window.PostsApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#adminPosts')
  if (el) window.PostsApp = app.mount(el)
}

if (document.querySelector('#adminPosts')) {
  window.mountPostsPage()
}
