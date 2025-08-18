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
      const adminList = reactive({
        header: [
          { title: 'UserName', class: '', fixed: false, },
          { title: 'DisplayName', class: '', fixed: false, },
          { title: 'SuperAdmin', class: '', fixed: false, },
        ],
        data: [
        { id: 0, UserName: 'eddy', DisplayName: 'eddy86', Avatar: null, SuderAdmin: 0, status: 0, isDelete: 0, checked: true },
        { id: 1, UserName: 'sally', DisplayName: 'sally88', Avatar: null, SuderAdmin: 1, status: 1, isDelete: 0, checked: false },
        { id: 2, UserName: 'eason', DisplayName: 'eason87', Avatar: null, SuderAdmin: 1, status: 0, isDelete: 0, checked: false },
      ]
      })
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
        adminList,
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

