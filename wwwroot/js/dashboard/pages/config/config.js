const { createApp, ref, reactive, onMounted } = Vue

window.mountConfigPage = function() {
  const app = createApp({
    setup() {
      //#region 變數
      
      const isLoading = ref(true)
      const toolStateList = reactive([
        { id: 0, name: 'FriendShip', state: false },
        { id: 0, name: 'NFTs_Collect', state: false },
        { id: 0, name: 'Web_Log', state: false },
      ])

      //#endregion

      //#region data



      //#endregion

      //#region tools



      //#endregion

      //#region LifeCycle

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      //#endregion

      return {
        isLoading,
        

        // Data
        toolStateList,
      }
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

