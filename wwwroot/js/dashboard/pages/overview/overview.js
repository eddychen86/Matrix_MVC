const { createApp, ref, reactive, onMounted } = Vue

window.mountOverviewPage = function() {
  const app = createApp({
    setup() {
      //#region 變數

      const isLoading = ref(true)
      const total = reactive([
        /* 總用戶數       */{ 
          id: 0, 
          title: 'user', 
          result: 0, 
          color: 'blue',
        },
        /* 文章總數       */{ 
          id: 1, 
          title: 'post', 
          result: 0, 
          color: 'green',
        },
        /* 待處理檢舉     */{ 
          id: 2, 
          title: 'reports', 
          result: 0, 
          color: 'yellow',
        },
        /* 今日活躍使用者 */{ 
          id: 3, 
          title: 'todayLoginUsers', 
          result: 0, 
          color: 'purple',
        },
      ])
      const systemStatus = reactive({
        /* 系統運行時間   */ totelRunTime: 0,
        /* 資料庫連線狀態 */ DBConnectStatus: false,
        /* SMTP 服務狀態  */ SMTPStatus: false,
        /* 剩餘儲存空間   */ Storage: 0,
      })

      //#endregion

      //#region Total

      const getTotalUsers = async () => {
        try {
          const response = await fetch('/api/Overview/GetUserState')

          if (response.ok) {
            const data = await response.json()
            
            // 計算總用戶數
            const totalUsersCount = data.totalUsers || 0
            
            // 計算今日登入用戶數
            const today = new Date()
            const todayDateString = today.toLocaleDateString() // 格式: "Fri Aug 16 2025"
            
            const todayLoginUsersCount = data.users.filter(user => {
              if (!user.lastLoginTime) return false
              
              const loginDate = new Date(user.lastLoginTime)
              // 修正時區問題：為登入日期加1天
              loginDate.setDate(loginDate.getDate() + 1)
              return loginDate.toLocaleDateString() === todayDateString
            }).length

            // 更新 total 陣列中對應的項目
            const userItem = total.find(item => item.title === 'user')
            const todayLoginItem = total.find(item => item.title === 'todayLoginUsers')
            
            if (userItem) userItem.result = totalUsersCount
            if (todayLoginItem) todayLoginItem.result = todayLoginUsersCount
          } else {
            console.error('API request failed:', response.status)
          }
        } catch (err) {
          console.error('Failed to fetch users:', err)
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
        try {
          const response = await fetch('/api/Overview/GetReportsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 更新 total 陣列中待處理報告的項目
            const reportsItem = total.find(item => item.title === 'reports')
            if (reportsItem) {
              reportsItem.result = data.pendingReports || 0
            }
          } else {
            console.error('Failed to fetch reports state:', response.status)
          }
        } catch (err) {
          console.error('Failed to fetch reports:', err)
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

const overviewElement = document.querySelector('#adminOverview')

if (overviewElement) {
  window.mountOverviewPage()
} else {
  console.log('❌ 找不到 #adminOverview 元素')
}

