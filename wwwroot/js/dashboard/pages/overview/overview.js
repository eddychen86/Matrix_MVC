const { createApp, ref, reactive, onMounted } = Vue

window.mountOverviewPage = function() {
  const app = createApp({
    setup() {
      //#region 變數

      const isLoading = ref(true)
      const total = reactive([
        /* 總用戶數       */ { id: 0, title: 'user', result: 0, color: 'blue',  },
        /* 文章總數       */ { id: 1, title: 'post', result: 0, color: 'green', },
        /* 待處理檢舉     */ { id: 2, title: 'reports', result: 0, color: 'yellow', },
        /* 今日活躍使用者 */ { id: 3, title: 'todayLoginUsers', result: 0, color: 'purple', },
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

      //#region 格式化函數
      
      const formatUptime = (days, hours, minutes) => {
        if (days >= 1) {
          return `${days} 天 ${hours} 小時 ${minutes} 分鐘`
        } else if (hours >= 1) {
          return `${hours} 小時 ${minutes} 分鐘`
        } else {
          const totalMinutes = Math.max(1, minutes)
          return `${totalMinutes} 分鐘`
        }
      }
      
      const formatStorageStatus = (usagePercentage) => {
        if (usagePercentage < 70) {
          return `${usagePercentage.toFixed(1)}% 使用中 (基於 32GB)`
        } else if (usagePercentage < 85) {
          return `${usagePercentage.toFixed(1)}% 使用中 (基於 32GB)`
        } else if (usagePercentage < 95) {
          return `${usagePercentage.toFixed(1)}% 使用中 (接近滿載，基於 32GB)`
        } else {
          return `${usagePercentage.toFixed(1)}% 使用中 (空間不足，基於 32GB)`
        }
      }
      
      //#endregion

      //#region Tools
      
      const JWT_SECRET = 'matrix-dashboard-secret-key-2024'
      
      // 簡化版 JWT 實作 (Base64 編碼 + HMAC 簽名)
      const createJWT = (payload) => {
        try {
          const header = { alg: 'HS256', typ: 'JWT' }
          const encodedHeader = btoa(JSON.stringify(header))
          const encodedPayload = btoa(JSON.stringify(payload))
          
          // 簡化簽名 (實際應用中應使用真正的 HMAC)
          const signature = btoa(encodedHeader + '.' + encodedPayload + '.' + JWT_SECRET).slice(0, 32)
          
          return `${encodedHeader}.${encodedPayload}.${signature}`
        } catch (error) {
          console.error('JWT 加密失敗:', error)
          return null
        }
      }
      
      const verifyJWT = (token) => {
        try {
          const parts = token.split('.')
          if (parts.length !== 3) return null
          
          const [encodedHeader, encodedPayload, signature] = parts
          
          // 驗證簽名
          const expectedSignature = btoa(encodedHeader + '.' + encodedPayload + '.' + JWT_SECRET).slice(0, 32)
          if (signature !== expectedSignature) {
            console.warn('JWT 簽名驗證失敗')
            return null
          }
          
          // 解碼 payload
          const payload = JSON.parse(atob(encodedPayload))
          
          // 檢查過期時間
          if (payload.exp && Date.now() > payload.exp) {
            console.warn('JWT 已過期')
            return null
          }
          
          return payload
        } catch (error) {
          console.error('JWT 解密失敗:', error)
          return null
        }
      }
      
      // Cookie 操作函數
      const setCookie = (name, value, hours = 24) => {
        const expires = new Date()
        expires.setTime(expires.getTime() + (hours * 60 * 60 * 1000))
        document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Strict`
      }
      
      const getCookie = (name) => {
        const value = `; ${document.cookie}`
        const parts = value.split(`; ${name}=`)
        if (parts.length === 2) return parts.pop().split(';').shift()
        return null
      }
      
      const deleteCookie = (name) => {
        document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;`
      }
      
      // 儲存加密數據到 cookie
      const saveEncryptedData = (key, data, hours = 24) => {
        const payload = {
          data: data,
          timestamp: Date.now(),
          exp: Date.now() + (hours * 60 * 60 * 1000)
        }
        const token = createJWT(payload)
        if (token) {
          setCookie(key, token, hours)
          return true
        }
        return false
      }
      
      // 從 cookie 讀取解密數據
      const loadEncryptedData = (key) => {
        const token = getCookie(key)
        if (!token) return null
        
        const payload = verifyJWT(token)
        return payload ? payload.data : null
      }
      
      //#endregion

      //#region Total

      const getTotalUsers = async () => {
        try {
          // 先嘗試從 cookie 讀取加密數據
          const cachedData = loadEncryptedData(USERS_DATA_KEY)
          if (cachedData) {
            console.log('Users data: 從 cookie 讀取數據')
            
            // 更新 total 陣列中對應的項目
            const userItem = total.find(item => item.title === 'user')
            const todayLoginItem = total.find(item => item.title === 'todayLoginUsers')
            
            if (userItem) userItem.result = cachedData.totalUsersCount || 0
            if (todayLoginItem) todayLoginItem.result = cachedData.todayLoginUsersCount || 0
            return
          }

          console.log('Users data: 呼叫 API 獲取新數據')
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

            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              totalUsersCount,
              todayLoginUsersCount
            }
            saveEncryptedData(USERS_DATA_KEY, dataToSave, 24)

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
          // 先嘗試從 cookie 讀取加密數據
          const cachedData = loadEncryptedData(POSTS_DATA_KEY)
          if (cachedData) {
            console.log('Posts data: 從 cookie 讀取數據')
            
            // 更新 total 陣列中文章總數的項目
            const postsItem = total.find(item => item.title === 'post')
            if (postsItem) {
              postsItem.result = cachedData.totalPosts || 0
            }
            return
          }

          console.log('Posts data: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetPostsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              totalPosts: data.totalPosts || 0
            }
            saveEncryptedData(POSTS_DATA_KEY, dataToSave, 24)
            
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
          // 先嘗試從 cookie 讀取加密數據
          const cachedData = loadEncryptedData(REPORTS_DATA_KEY)
          if (cachedData) {
            console.log('Reports data: 從 cookie 讀取數據')
            
            // 更新 total 陣列中待處理報告的項目
            const reportsItem = total.find(item => item.title === 'reports')
            if (reportsItem) {
              reportsItem.result = cachedData.pendingReports || 0
            }
            return
          }

          console.log('Reports data: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetReportsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              pendingReports: data.pendingReports || 0
            }
            saveEncryptedData(REPORTS_DATA_KEY, dataToSave, 24)
            
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
          // 先嘗試從 cookie 讀取加密數據
          const cachedData = loadEncryptedData(HASHTAGS_DATA_KEY)
          if (cachedData) {
            console.log('Hashtags data: 從 cookie 讀取數據')
            
            // 更新標籤資料
            hashtagsData.totalHashtags = cachedData.totalHashtags || 0
            hashtagsData.hashtags = cachedData.hashtags || []
            
            console.log('Hashtags loaded from cache:', hashtagsData.totalHashtags, 'tags')
            return
          }

          console.log('Hashtags data: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetHashtagsState')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              totalHashtags: data.totalHashtags || 0,
              hashtags: data.hashtags || []
            }
            saveEncryptedData(HASHTAGS_DATA_KEY, dataToSave, 24)
            
            // 更新標籤資料
            hashtagsData.totalHashtags = data.totalHashtags || 0
            hashtagsData.hashtags = data.hashtags || []
            
            console.log('Hashtags loaded from API:', hashtagsData.totalHashtags, 'tags')
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
      
      // 各別 API 的 localStorage key（時間戳記）
      const UPTIME_KEY = 'matrix_uptime_last_update'
      const DB_STATUS_KEY = 'matrix_db_status_last_update'
      const SMTP_STATUS_KEY = 'matrix_smtp_status_last_update'
      const STORAGE_KEY = 'matrix_storage_last_update'
      
      // 各別 API 的 cookie key（加密數據）
      const USERS_DATA_KEY = 'matrix_users_data'
      const POSTS_DATA_KEY = 'matrix_posts_data' 
      const REPORTS_DATA_KEY = 'matrix_reports_data'
      const HASHTAGS_DATA_KEY = 'matrix_hashtags_data'
      const UPTIME_DATA_KEY = 'matrix_uptime_data'
      const DB_STATUS_DATA_KEY = 'matrix_db_status_data'
      const SMTP_STATUS_DATA_KEY = 'matrix_smtp_status_data'
      const STORAGE_DATA_KEY = 'matrix_storage_data'

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
          // 先嘗試從 cookie 讀取加密數據
          if (!forceUpdate) {
            const cachedData = loadEncryptedData(UPTIME_DATA_KEY)
            if (cachedData) {
              console.log('System uptime: 從 cookie 讀取數據')
              const uptimeFormatted = formatUptime(cachedData.days || 0, cachedData.hours || 0, cachedData.minutes || 0)
              systemStatus.totelRunTime = uptimeFormatted
              return
            }
          }

          if (!forceUpdate && !shouldUpdate(UPTIME_KEY, UPTIME_INTERVAL)) {
            console.log('System uptime: 尚未到更新時間，跳過API呼叫')
            return
          }

          console.log('System uptime: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetSystemUptime')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（30分鐘過期，與更新間隔一致）
            const dataToSave = {
              days: data.days || 0,
              hours: data.hours || 0,
              minutes: data.minutes || 0
            }
            saveEncryptedData(UPTIME_DATA_KEY, dataToSave, 0.5) // 30分鐘
            
            const uptimeFormatted = formatUptime(data.days || 0, data.hours || 0, data.minutes || 0)
            systemStatus.totelRunTime = uptimeFormatted
            localStorage.setItem(UPTIME_KEY, Date.now().toString())
            console.log('System uptime loaded from API:', data)
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
          // 先嘗試從 cookie 讀取加密數據
          if (!forceUpdate) {
            const cachedData = loadEncryptedData(DB_STATUS_DATA_KEY)
            if (cachedData) {
              console.log('Database status: 從 cookie 讀取數據')
              systemStatus.DBConnectStatus = cachedData.databaseConnected || false
              return
            }
          }

          if (!forceUpdate && !shouldUpdate(DB_STATUS_KEY)) {
            console.log('Database status: 尚未到更新時間，跳過API呼叫')
            return
          }

          console.log('Database status: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetDatabaseStatus')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              databaseConnected: data.databaseConnected || false
            }
            saveEncryptedData(DB_STATUS_DATA_KEY, dataToSave, 24)
            
            systemStatus.DBConnectStatus = data.databaseConnected || false
            localStorage.setItem(DB_STATUS_KEY, Date.now().toString())
            console.log('Database status loaded from API:', data)
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
          // 先嘗試從 cookie 讀取加密數據
          if (!forceUpdate) {
            const cachedData = loadEncryptedData(SMTP_STATUS_DATA_KEY)
            if (cachedData) {
              console.log('SMTP status: 從 cookie 讀取數據')
              systemStatus.SMTPStatus = cachedData.smtpServiceActive || false
              return
            }
          }

          if (!forceUpdate && !shouldUpdate(SMTP_STATUS_KEY)) {
            console.log('SMTP status: 尚未到更新時間，跳過API呼叫')
            return
          }

          console.log('SMTP status: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetSmtpStatus')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              smtpServiceActive: data.smtpServiceActive || false
            }
            saveEncryptedData(SMTP_STATUS_DATA_KEY, dataToSave, 24)
            
            systemStatus.SMTPStatus = data.smtpServiceActive || false
            localStorage.setItem(SMTP_STATUS_KEY, Date.now().toString())
            console.log('SMTP status loaded from API:', data)
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
          // 先嘗試從 cookie 讀取加密數據
          if (!forceUpdate) {
            const cachedData = loadEncryptedData(STORAGE_DATA_KEY)
            if (cachedData) {
              console.log('Storage status: 從 cookie 讀取數據')
              const storageFormatted = formatStorageStatus(cachedData.storageUsagePercentage || 0)
              systemStatus.Storage = storageFormatted
              return
            }
          }

          if (!forceUpdate && !shouldUpdate(STORAGE_KEY)) {
            console.log('Storage status: 尚未到更新時間，跳過API呼叫')
            return
          }

          console.log('Storage status: 呼叫 API 獲取新數據')
          const response = await fetch('/api/Overview/GetStorageStatus')
          
          if (response.ok) {
            const data = await response.json()
            
            // 儲存數據到加密 cookie（24小時過期）
            const dataToSave = {
              storageUsagePercentage: data.storageUsagePercentage || 0
            }
            saveEncryptedData(STORAGE_DATA_KEY, dataToSave, 24)
            
            const storageFormatted = formatStorageStatus(data.storageUsagePercentage || 0)
            systemStatus.Storage = storageFormatted
            localStorage.setItem(STORAGE_KEY, Date.now().toString())
            console.log('Storage status loaded from API:', data)
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

