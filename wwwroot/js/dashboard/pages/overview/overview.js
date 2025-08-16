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
        /* 系統運行時間   */ totelRunTime: '載入中...',
        /* 資料庫連線狀態 */ DBConnectStatus: false,
        /* SMTP 服務狀態  */ SMTPStatus: false,
        /* 剩餘儲存空間   */ Storage: '載入中...',
      })

      const hashtagsData = reactive({
        totalHashtags: 0,
        hashtags: []
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
        try {
          const response = await fetch('/api/Overview/GetPostsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 更新 total 陣列中文章總數的項目
            const postsItem = total.find(item => item.title === 'post')
            if (postsItem) {
              postsItem.result = data.totalPosts || 0
            }
          } else {
            console.error('Failed to fetch posts state:', response.status)
          }
        } catch (err) {
          console.error('Failed to fetch posts:', err)
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

      const getHashtagsState = async () => {
        try {
          const response = await fetch('/api/Overview/GetHashtagsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 更新標籤資料
            hashtagsData.totalHashtags = data.totalHashtags || 0
            hashtagsData.hashtags = data.hashtags || []
            
            console.log('Hashtags loaded:', hashtagsData.totalHashtags, 'tags')
          } else {
            console.error('Failed to fetch hashtags state:', response.status)
          }
        } catch (err) {
          console.error('Failed to fetch hashtags:', err)
        }
      }

      //#endregion

      //#region System

      const UPTIME_INTERVAL = 30 * 60 * 1000 // 30分鐘（毫秒）
      const DAILY_INTERVAL = 24 * 60 * 60 * 1000 // 24小時（毫秒）
      
      // 各別 API 的 localStorage key
      const UPTIME_KEY = 'matrix_uptime_last_update'
      const DB_STATUS_KEY = 'matrix_db_status_last_update'
      const SMTP_STATUS_KEY = 'matrix_smtp_status_last_update'
      const STORAGE_KEY = 'matrix_storage_last_update'

      const shouldUpdate = (storageKey, interval = DAILY_INTERVAL) => {
        const lastUpdate = localStorage.getItem(storageKey)
        if (!lastUpdate) return true
        
        const lastUpdateTime = parseInt(lastUpdate)
        const currentTime = Date.now()
        const timeDiff = currentTime - lastUpdateTime
        
        return timeDiff >= interval
      }

      const getSystemUptime = async (forceUpdate = false) => {
        try {
          if (!forceUpdate && !shouldUpdate(UPTIME_KEY, UPTIME_INTERVAL)) {
            console.log('System uptime: 尚未到更新時間，跳過API呼叫')
            return
          }

          const response = await fetch('/api/Overview/GetSystemUptime')
          
          if (response.ok) {
            const data = await response.json()
            systemStatus.totelRunTime = data.uptimeFormatted || '無法取得'
            localStorage.setItem(UPTIME_KEY, Date.now().toString())
            console.log('System uptime loaded:', data)
          } else {
            console.error('Failed to fetch system uptime:', response.status)
            systemStatus.totelRunTime = '載入失敗'
          }
        } catch (err) {
          console.error('Failed to fetch system uptime:', err)
          systemStatus.totelRunTime = '載入失敗'
        }
      }

      const getDatabaseStatus = async (forceUpdate = false) => {
        try {
          if (!forceUpdate && !shouldUpdate(DB_STATUS_KEY)) {
            console.log('Database status: 尚未到更新時間，跳過API呼叫')
            return
          }

          const response = await fetch('/api/Overview/GetDatabaseStatus')
          
          if (response.ok) {
            const data = await response.json()
            systemStatus.DBConnectStatus = data.databaseConnected || false
            localStorage.setItem(DB_STATUS_KEY, Date.now().toString())
            console.log('Database status loaded:', data)
          } else {
            console.error('Failed to fetch database status:', response.status)
            systemStatus.DBConnectStatus = false
          }
        } catch (err) {
          console.error('Failed to fetch database status:', err)
          systemStatus.DBConnectStatus = false
        }
      }

      const getSmtpStatus = async (forceUpdate = false) => {
        try {
          if (!forceUpdate && !shouldUpdate(SMTP_STATUS_KEY)) {
            console.log('SMTP status: 尚未到更新時間，跳過API呼叫')
            return
          }

          const response = await fetch('/api/Overview/GetSmtpStatus')
          
          if (response.ok) {
            const data = await response.json()
            systemStatus.SMTPStatus = data.smtpServiceActive || false
            localStorage.setItem(SMTP_STATUS_KEY, Date.now().toString())
            console.log('SMTP status loaded:', data)
          } else {
            console.error('Failed to fetch SMTP status:', response.status)
            systemStatus.SMTPStatus = false
          }
        } catch (err) {
          console.error('Failed to fetch SMTP status:', err)
          systemStatus.SMTPStatus = false
        }
      }

      const getStorageStatus = async (forceUpdate = false) => {
        try {
          if (!forceUpdate && !shouldUpdate(STORAGE_KEY)) {
            console.log('Storage status: 尚未到更新時間，跳過API呼叫')
            return
          }

          const response = await fetch('/api/Overview/GetStorageStatus')
          
          if (response.ok) {
            const data = await response.json()
            systemStatus.Storage = data.storageStatusText || '無法取得'
            localStorage.setItem(STORAGE_KEY, Date.now().toString())
            console.log('Storage status loaded:', data)
          } else {
            console.error('Failed to fetch storage status:', response.status)
            systemStatus.Storage = '載入失敗'
          }
        } catch (err) {
          console.error('Failed to fetch storage status:', err)
          systemStatus.Storage = '載入失敗'
        }
      }

      const setupSystemStatusTimers = () => {
        // 設定各別的定時器
        setInterval(() => getSystemUptime(), UPTIME_INTERVAL) // 30分鐘檢查一次
        setInterval(() => getDatabaseStatus(), DAILY_INTERVAL) // 一天檢查一次
        setInterval(() => getSmtpStatus(), DAILY_INTERVAL) // 一天檢查一次
        setInterval(() => getStorageStatus(), DAILY_INTERVAL) // 一天檢查一次
      }

      //#endregion

      //#region init

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫
          await getTotalUsers()
          await getTotalPosts()
          await getTotalNoProcessReposts()
          await getHashtagsState()
          
          // 各別系統狀態根據 localStorage 判斷是否需要更新
          await getSystemUptime()
          await getDatabaseStatus()
          await getSmtpStatus()
          await getStorageStatus()
          
          // 啟動系統狀態定時器
          setupSystemStatusTimers()
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      //#endregion

      //#region Helper Functions
      
      const getStorageStatusColor = (storageText) => {
        if (typeof storageText !== 'string') return 'text-gray-600'
        
        // 提取百分比數字
        const match = storageText.match(/(\d+(?:\.\d+)?)%/)
        if (!match) return 'text-gray-600'
        
        const percentage = parseFloat(match[1])
        
        if (percentage < 70) return 'text-green-600'
        if (percentage < 85) return 'text-yellow-600'
        if (percentage < 95) return 'text-orange-600'
        return 'text-red-600'
      }

      //#endregion

      return {
        isLoading,

        // Data
        total,
        systemStatus,
        hashtagsData,
        
        // Helper Functions
        getStorageStatusColor,
        
        // System Status Functions
        forceUpdateUptime: () => getSystemUptime(true),
        forceUpdateDatabaseStatus: () => getDatabaseStatus(true),
        forceUpdateSmtpStatus: () => getSmtpStatus(true),
        forceUpdateStorageStatus: () => getStorageStatus(true),
        forceUpdateAllSystemStatus: () => {
          getSystemUptime(true)
          getDatabaseStatus(true)
          getSmtpStatus(true)
          getStorageStatus(true)
        },
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

