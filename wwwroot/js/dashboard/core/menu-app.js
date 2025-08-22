/**
 * Menu App - Dashboard 選單 Vue 應用程式
 * 從 d_main.js 中抽離出來的選單應用功能
 */

(function() {
    'use strict';

    // Content loader controls (inside #dashboard-content)
    const getLoaderEl = () => document.getElementById('dashboard-loader')
    const getSlotEl = () => document.getElementById('dashboard-slot')
    const showContentLoader = () => { 
        const l = getLoaderEl(); 
        const s = getSlotEl(); 
        if (l) l.classList.remove('hidden'); 
        if (s) s.classList.add('hidden') 
    }
    const hideContentLoader = () => { 
        const l = getLoaderEl(); 
        const s = getSlotEl(); 
        if (l) l.classList.add('hidden'); 
        if (s) s.classList.remove('hidden') 
    }

    // Dashboard Menu App
    const DashboardMenuApp = function() {
        if (typeof Vue === 'undefined') {
            console.log('Vue not ready, retrying...')
            return
        }

        const createMenuApp = function() {
            const app = Vue.createApp({
                compilerOptions: {
                    isCustomElement: (tag) => tag.includes('calendar-')
                },
                setup() {
                    // 引入全域狀態
                    const globalStateModule = window.useGlobalState()
                    
                    const {
                        globalState,
                        currentLanguage,
                        availableLanguages,
                        currentUser,
                        theme,
                        menuData,
                        loadMenuData
                    } = globalStateModule

                    // 側邊欄狀態
                    const isCollapsed = Vue.ref(false)
                    const searchQuery = Vue.ref('')

                    // 側邊欄切換
                    const toggleSidebar = function() {
                        isCollapsed.value = !isCollapsed.value
                        if (typeof lucide !== 'undefined') {
                            lucide.createIcons()
                        }
                    }

                    // 語言切換
                    const toggleLang = function() {
                        if (window.DashboardLanguageManager) {
                            return window.DashboardLanguageManager.toggleLanguage()
                        }
                        console.error('DashboardLanguageManager not found')
                    }

                    // 主題切換
                    const toggleTheme = function() {
                        if (window.DashboardThemeManager) {
                            return window.DashboardThemeManager.toggleTheme()
                        }
                        console.error('DashboardThemeManager not found')
                    }

                    // 登出功能
                    const logout = async function() {
                        try {
                            const response = await fetch('/api/auth/logout', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' }
                            })

                            const result = await response.json()

                            if (result.success) {
                                // console.log('Logout successful')
                                window.location.href = '/'
                            } else {
                                console.error('Logout failed')
                            }
                        } catch (error) {
                            console.error('Error during logout:', error)
                        }
                    }

                    // 登入跳轉功能
                    const login = function() {
                        window.location.href = '/login'
                    }

                    // 動態載入對應頁面的腳本（僅載入一次）
                    const ensurePageScriptLoaded = (page) => {
                        const name = page.toLowerCase()
                        const id = `dashboard-page-script-${name}`
                        if (document.getElementById(id)) return Promise.resolve()
                        
                        const version = (typeof window !== 'undefined' && (window.__APP_VERSION__ || window.__assetVersion)) || Date.now()
                        const path = `/js/dashboard/pages/${name}/${name}.js?v=${version}`
                        
                        return new Promise((resolve, reject) => {
                            const s = document.createElement('script')
                            s.src = path
                            s.async = true
                            s.id = id
                            s.onload = () => resolve()
                            s.onerror = () => reject(new Error(`Failed to load ${path}`))
                            document.body.appendChild(s)
                        })
                    }

                    // 檢查是否在 Dashboard 路徑下
                    const isDashboardPath = function() {
                        return window.location.pathname.toLowerCase().includes('/dashboard/')
                    }

                    // 檢查路徑是否相同
                    const isSamePath = function(targetPath) {
                        const currentPath = window.location.pathname.toLowerCase()
                        const normalizedTarget = targetPath.toLowerCase()
                        
                        return currentPath === normalizedTarget || 
                               currentPath === normalizedTarget + '/' || 
                               currentPath + '/' === normalizedTarget
                    }

                    // 載入 Dashboard 頁面內容（SSR 導航）
                    const loadDashboardPage = function(page) {
                        const capitalizedPage = page.charAt(0).toUpperCase() + page.slice(1)
                        const targetUrl = `/Dashboard/${capitalizedPage}`
                        
                        // 檢查是否為當前頁面，如果是則不跳轉
                        if (isSamePath(targetUrl)) {
                            // console.log(`已在目標頁面 ${targetUrl}，跳過導航`)
                            return
                        }
                        
                        // 中止目前頁面相關的 DB GET 請求
                        if (typeof window.abortAllDbGets === 'function') {
                            window.abortAllDbGets()
                        }
                        
                        // 直接 SSR 導航到完整頁面
                        window.location.href = targetUrl
                    }

                    // 處理 Dashboard 選單點擊（新增）
                    const handleDashboardMenuClick = function(page) {
                        // 如果當前不在 Dashboard 路徑下，直接導航
                        if (!isDashboardPath()) {
                            loadDashboardPage(page)
                            return
                        }

                        // 如果在 Dashboard 路徑下，檢查是否為相同頁面
                        const capitalizedPage = page.charAt(0).toUpperCase() + page.slice(1)
                        const targetUrl = `/Dashboard/${capitalizedPage}`
                        
                        if (isSamePath(targetUrl)) {
                            // console.log(`已在目標頁面 ${targetUrl}，跳過導航`)
                            return
                        }

                        // 不同頁面則導航
                        loadDashboardPage(page)
                    }

                    // 處理主要 Dashboard 按鈕點擊（新增）
                    const handleDashboardMainClick = function() {
                        const targetUrl = '/Dashboard/Overview'
                        
                        // 如果當前不在 Dashboard 路徑下，直接導航
                        if (!isDashboardPath()) {
                            window.location.href = targetUrl
                            return
                        }

                        // 如果在 Dashboard 路徑下，檢查是否為相同頁面
                        if (isSamePath(targetUrl)) {
                            // console.log(`已在目標頁面 ${targetUrl}，跳過導航`)
                            return
                        }

                        // 不同頁面則導航
                        window.location.href = targetUrl
                    }

                    // 統一處理選單點擊事件
                    const handleMenuClick = function(action) {
                        switch (action) {
                            case 'toggleLang':
                                toggleLang()
                                break
                            case 'toggleSidebar':
                                toggleSidebar()
                                break
                            case 'logout':
                                logout()
                                break
                            case 'login':
                                login()
                                break
                            default:
                                console.warn(`Unknown action: ${action}`)
                        }
                    }

                    // 獲取當前語系資訊
                    const getCurrentLanguageInfo = Vue.computed(() => {
                        return availableLanguages.value.find(lang => lang.code === currentLanguage.value)
                    })

                    // 生命週期
                    Vue.onMounted(() => {
                        // 初始化 Lucide 圖標
                        if (typeof lucide !== 'undefined') {
                            lucide.createIcons()
                        }
                    })

                    return {
                        // 全域狀態
                        globalState,
                        currentLanguage,
                        availableLanguages,
                        currentUser,
                        theme,
                        menuData,

                        // 選單狀態
                        isCollapsed,
                        searchQuery,

                        // 功能方法
                        toggleSidebar,
                        toggleLang,
                        toggleTheme,
                        logout,
                        login,
                        loadDashboardPage,
                        handleMenuClick,
                        handleDashboardMenuClick,
                        handleDashboardMainClick,
                        getCurrentLanguageInfo,
                        ensurePageScriptLoaded,
                        
                        // 載入控制
                        showContentLoader,
                        hideContentLoader,
                        
                        // 路徑檢查工具
                        isDashboardPath,
                        isSamePath
                    }
                }
            })

            // 配置警告處理器
            app.config.warnHandler = function(msg, instance, trace) {
                if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
                    return // 忽略 script/style 標籤警告
                }
                console.warn(msg)
            }

            return app
        }

        // 根據 DOM 載入狀態決定執行時機
        const mountApp = () => {
            const menuApp = createMenuApp()
            if (document.querySelector('#dashboard-menu')) {
                window.DashboardMenuApp = menuApp.mount('#dashboard-menu')
                return window.DashboardMenuApp
            }
            return null
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', mountApp)
        } else {
            return mountApp()
        }
    }

    // 暴露到全域
    window.createDashboardMenuApp = DashboardMenuApp
    
    // 暴露載入控制函數
    window.DashboardContentLoader = {
        show: showContentLoader,
        hide: hideContentLoader,
        getLoaderEl,
        getSlotEl
    }

})();