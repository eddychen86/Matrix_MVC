import { useFormatting } from '/js/hooks/useFormatting.js'
import { useMenu } from '/js/components/menu.js'
import { useOverview } from '/js/dashboard/pages/Overview/overview.js'
import { useUsers } from '/js/dashboard/pages/users/users.js'
import { usePosts } from '/js/dashboard/pages/posts/posts.js'
import { useReports } from '/js/dashboard/pages/reports/reports.js'
import { useConfig } from '/js/dashboard/pages/config/config.js'
import authManager from '/js/auth/auth-manager.js'
import loginPopupManager from '/js/auth/login-popup.js'

const DashboardApp = content => {
  if (typeof Vue === 'undefined') {
    console.log('Vue not ready, retrying...')
    return
  } else {
    lucide.createIcons()
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => {
        const app = Vue.createApp(content)
        // 配置警告處理器來忽略 script/style 標籤警告
        app.config.warnHandler = (msg, instance, trace) => {
          if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
            return // 忽略這類警告
          }
          console.warn(msg)
        }
        window.DashboardApp = app.mount('#app')
      })
    } else {
      // DOM 已經載入完成
      const app = Vue.createApp(content)
      // 配置警告處理器來忽略 script/style 標籤警告
      app.config.warnHandler = (msg, instance, trace) => {
        if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
          return // 忽略這類警告
        }
        console.warn(msg)
      }
      window.DashboardApp = app.mount('#dashboard')
    }
  }
}

// 將需要被內嵌 HTML 調用的單例暴露到全域
window.loginPopupManager = loginPopupManager

DashboardApp({
  setup() {
    //#region 宣告變數
    const { reactive, onMounted } = Vue
    const { formatDate, timeAgo } = useFormatting()
    // 全局用戶狀態
    const currentUser = reactive({
      isAuthenticated: false,
      userId: null,
      username: '',
      email: '',
      role: 0,
      status: 0,
      isAdmin: false,
      isMember: false
    })
    //#endregion

    //#region 獲取用戶信息

    const getCurrentUser = async () => {
      try {
        const { authService } = await import('/js/services/AuthService.js')
        if (authService) {
          const authStatus = await authService.getAuthStatus()

          if (authStatus.success && authStatus.data.authenticated) {
            const user = authStatus.data.user

            Object.assign(currentUser, {
              isAuthenticated: true,
              userId: user.id,
              username: user.username,
              email: user.email,
              role: user.role || 0,
              status: user.status || 0,
              isAdmin: user.isAdmin || false,
              isMember: user.isMember || true
            })
          } else {
            // 未認證狀態
            Object.assign(currentUser, {
              isAuthenticated: false,
              userId: null
            })
          }
        } else {
          console.warn('AuthService not available, using direct API call')
          // Fallback 直接 API 呼叫（理論上不會進入）
          const response = await fetch('/api/auth/status')
          const data = await response.json()

          if (data.success && data.data.authenticated) {
            const user = data.data.user
            Object.assign(currentUser, {
              isAuthenticated: true,
              userId: user.id,
              username: user.username,
              email: user.email,
              role: user.role || 0,
              status: user.status || 0,
              isAdmin: user.isAdmin || false,
              isMember: user.isMember || true
            })
          } else {
            // 未認證狀態
            Object.assign(currentUser, {
              isAuthenticated: false,
              userId: null
            })
          }
        }
      } catch (err) {
        console.error('獲取用戶信息失敗:', err)
        Object.assign(currentUser, {
          isAuthenticated: false,
          userId: null
        })
      }
    }

    // 將 currentUser 設為全局可訪問
    window.currentUser = currentUser

    //#endregion

    //#region 匯入各頁面的 Vue 模組（ESM）

    const LoadingPage = (pattern, useFunc) => {
      const path = window.location.pathname.toLowerCase()
      const matched = pattern instanceof RegExp
        ? pattern.test(path)
        : path.includes(String(pattern).toLowerCase())

      if (!matched) return {}
      try {
        return typeof useFunc === 'function' ? useFunc() : {}
      } catch (error) {
        console.error('頁面模組載入失敗:', error)
        return {}
      }
    }

    // 路徑偵測（供後續邏輯使用）
    const currentPath = window.location.pathname.toLowerCase()
    const isOverviewePage = /^\/overview(?:\/|$)/.test(currentPath)
    const isUsersPage = /^\/(?:users(?:\/|$))?$|^\/$/.test(currentPath)
    const isPostsPage = /^\/(?:posts(?:\/|$))?$|^\/$/.test(currentPath)
    const isReportsPage = /^\/(?:reports(?:\/|$))?$|^\/$/.test(currentPath)
    const isConfigPage = /^\/(?:config(?:\/|$))?$|^\/$/.test(currentPath)

    // 組件/頁面模組
    const Menu = (typeof useMenu === 'function') ? useMenu() : {}
    const Overview = LoadingPage(/^\/(?:overview(?:\/|$))?$|^\/$/i, useOverview)
    const Users = LoadingPage(/^\/users(?:\/|$)/i, useUsers)
    const Posts = LoadingPage(/^\/posts(?:\/|$)/i, usePosts)
    const Reports = LoadingPage(/^\/reports(?:\/|$)/i, useReports)
    const Config = LoadingPage(/^\/config(?:\/|$)/i, useConfig)


    //#endregion

    //#region Lifecycle

    // 組件掛載時獲取用戶信息並初始化頁面數據
    onMounted(async () => {
      await getCurrentUser()

      if (isOverviewePage) {

      } else if (isUsersPage) {

      } else if (isPostsPage) {

      } else if (isReportsPage) {

      } else if (isConfigPage) {

      }
    })

    //#endregion

    return {
      // user state
      currentUser,
      getCurrentUser,

      // hooks
      formatDate,
      timeAgo,

      // menu functions (spread from useMenu)
      ...Menu,
      ...Overview,
      ...Users,
      ...Posts,
      ...Reports,
      ...Config,
    }
  }
})
