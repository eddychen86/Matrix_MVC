const { createApp, ref, onMounted } = Vue

window.mountReportsPage = function() {
  const app = createApp({
    setup() {
      const isLoading = ref(true)

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ReportsApi 可在此呼叫
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      return { isLoading }
    }
  })

  if (window.ReportsApp && typeof window.ReportsApp.unmount === 'function') {
    try { window.ReportsApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#adminReports')
  if (el) window.ReportsApp = app.mount(el)
}

if (document.querySelector('#adminReports')) {
  window.mountReportsPage()
}

