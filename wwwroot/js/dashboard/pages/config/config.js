const { createApp, ref, onMounted } = Vue

window.mountConfigPage = function() {
  const app = createApp({
    setup() {
      const isLoading = ref(true)

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      return { isLoading }
    }
  })

  if (window.ConfigApp && typeof window.ConfigApp.unmount === 'function') {
    try { window.ConfigApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#adminConfig')
  if (el) window.ConfigApp = app.mount(el)
}

if (document.querySelector('#adminConfig')) {
  window.mountConfigPage()
}

