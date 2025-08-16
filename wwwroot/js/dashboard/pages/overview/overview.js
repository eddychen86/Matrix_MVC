const { createApp, ref, reactive, onMounted } = Vue

window.mountOverviewPage = function() {
  const app = createApp({
    setup() {
      //#region 變數

      const isLoading = ref(true)
      const total = reactive({
        /* 總用戶數       */ user: 0,
        /* 文章總數       */ post: 0,
        /* 待處理檢舉     */ reports: 0,
        /* 今日活躍使用者 */ todayLoginUsers: 0,
      })
      const systemStatus = reactive({
        /* 系統運行時間   */ totelRunTime: 0,
        /* 資料庫連線狀態 */ DBConnectStatus: false,
        /* SMTP 服務狀態  */ SMTPStatus: false,
        /* 剩餘儲存空間   */ Storage: 0,
      })

      //#endregion

      //#region Total

      const getTotalUsers = async () => {
        let result = 0

        try {
          const response = await fetch(
            '',
            {
              
            }
          )

          total.user = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getTotalPosts = async () => {
        let result = 0

        try {

          total.post = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getTotalNoProcessReposts = async () => {
        let result = 0

        try {

          total.reports = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getTotaltodaysLoginUsers = async () => {
        let result = 0

        try {

          total.todayLoginUsers = result
        } catch (err) {
          console.log('', err)
        }
      }

      //#endregion

      //#region System

      const getSystemRunTime = async () => {
        let result = 0

        try {

          systemStatus.totelRunTime = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getDBConnectStatus = async () => {
        let result = 0

        try {

          systemStatus.DBConnectStatus = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getSMTPStatus = async () => {
        let result = 0

        try {

          systemStatus.SMTPStatus = result
        } catch (err) {
          console.log('', err)
        }
      }

      const getStorage = async () => {
        let result = 0

        try {

          systemStatus.Storage = result
        } catch (err) {
          console.log('', err)
        }
      }

      //#endregion

      //#region init

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫
          await getTotalUsers()
          await getTotalPosts()
          await getTotalNoProcessReposts()
          await getTotaltodaysLoginUsers()
          await getSystemRunTime()
          await getDBConnectStatus()
          await getSMTPStatus()
          await getStorage()
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      //#endregion

      return {
        isLoading,

        // Data
        total,
        systemStatus,
      }
    }
  })

  if (window.OverviewApp && typeof window.OverviewApp.unmount === 'function') {
    try { window.OverviewApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#adminOverview')
  if (el) window.OverviewApp = app.mount(el)
}

if (document.querySelector('#adminOverview')) {
  window.mountOverviewPage()
}

