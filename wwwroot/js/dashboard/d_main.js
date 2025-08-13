/**
 * Dashboard Main App - 整合全域狀態管理和選單應用
 * 不使用 ES modules，直接在全域環境中運行
 * 支援 zh-TW 和 en-US 語系切換
 */

(function() {
    'use strict';

    // Content loader controls (inside #dashboard-content)
    const getLoaderEl = () => document.getElementById('dashboard-loader')
    const getSlotEl = () => document.getElementById('dashboard-slot')
    const showContentLoader = () => { const l = getLoaderEl(); const s = getSlotEl(); if (l) l.classList.remove('hidden'); if (s) s.classList.add('hidden') }
    const hideContentLoader = () => { const l = getLoaderEl(); const s = getSlotEl(); if (l) l.classList.add('hidden'); if (s) s.classList.remove('hidden') }

    // Navigation-scoped AbortController for page GET APIs
    let navAbortController = (typeof window !== 'undefined' && window.__navAbortController) || new AbortController()
    if (typeof window !== 'undefined') window.__navAbortController = navAbortController

    // 提供外部呼叫以中止當前頁面的 DB GET 請求
    if (typeof window !== 'undefined') {
        window.abortAllDbGets = function() {
            try { navAbortController.abort() } finally {
                navAbortController = new AbortController()
                window.__navAbortController = navAbortController
            }
        }
    }

    // Patch fetch to track /api/Db_* requests and attach cancellable signals (GET only)
    if (typeof window !== 'undefined' && typeof window.fetch === 'function' && !window.__dbFetchPatched) {
        window.__dbFetchPatched = true
        const origFetch = window.fetch
        let dbApiInFlight = 0
        const isDbApi = (input) => {
            try {
                let url
                if (typeof input === 'string') url = input
                else if (input && typeof input.url === 'string') url = input.url
                else return false
                const u = new URL(url, window.location.origin)
                return /^\/api\/Db_/i.test(u.pathname)
            } catch { return false }
        }
        window.__getDbInFlight = () => dbApiInFlight
        window.fetch = function(...args) {
            const tracking = isDbApi(args[0])
            const method = (args[1] && args[1].method) ? String(args[1].method).toUpperCase() : 'GET'

            // 建立可被導航中止的 signal（僅 GET）
            let init = args[1]
            let combinedController = null
            let detachHandlers = () => {}
            if (tracking && method === 'GET') {
                combinedController = new AbortController()
                const pageAbort = navAbortController

                const onPageAbort = () => { try { combinedController.abort() } catch(_){} }
                pageAbort.signal.addEventListener('abort', onPageAbort)

                let onOrigAbort = null
                if (init && init.signal) {
                    if (init.signal.aborted) {
                        combinedController.abort()
                    } else {
                        onOrigAbort = () => { try { combinedController.abort() } catch(_){} }
                        init.signal.addEventListener('abort', onOrigAbort)
                    }
                }

                init = { ...(init || {}), signal: combinedController.signal }
                detachHandlers = () => {
                    try { pageAbort.signal.removeEventListener('abort', onPageAbort) } catch(_){}
                    try { if (init && init.signal && onOrigAbort) init.signal.removeEventListener('abort', onOrigAbort) } catch(_){}
                }
            }

            if (tracking) { dbApiInFlight++ }
            const finalize = () => { if (tracking) { dbApiInFlight = Math.max(0, dbApiInFlight - 1) } detachHandlers() }

            try {
                const p = origFetch.call(this, args[0], init)
                if (p && typeof p.finally === 'function') return p.finally(finalize)
                // Fallback
                return p.then((x) => { finalize(); return x }, (e) => { finalize(); throw e })
            } catch (e) {
                finalize(); throw e
            }
        }
    }

    // 全域狀態物件
    const globalState = Vue.reactive({
        // 語系相關
        currentLanguage: document.documentElement.lang || 'zh-TW',
        availableLanguages: [
            { code: 'zh-TW', name: '繁體中文', flag: '🇹🇼' },
            { code: 'en-US', name: 'English', flag: '🇺🇸' }
        ],

        // 使用者狀態
        currentUser: {
            isAuthenticated: false,
            userId: null,
            username: '',
            email: '',
            role: 0,
            status: 0,
            isAdmin: false,
            isMember: false
        },

        // 主題相關
        theme: {
            isDark: false,
            primaryColor: '#3b82f6'
        },

        // 載入狀態
        loading: {
            global: false,
            menu: false
        },

        // 選單資料
        menuData: null
    })

    // 翻譯快取系統（參考 menu.js）
    const translationCache = new Map()
    
    // 初始資料（由伺服端注入）
    const initialMenu = (typeof window !== 'undefined' && window.__INITIAL_MENU__) ? window.__INITIAL_MENU__ : null
    // API 請求快取，防止重複請求（改為基於初始資料）
    let menuDataPromise = null

    // 全域方法
    const globalActions = {
        // 語系切換 - 參考 menu.js 的 toggleLang 邏輯
        async toggleLang() {
            const curLang = document.documentElement.lang

            // 標準化當前語言代碼
            let normalizedCurLang = curLang
            if (curLang === 'en-TW' || curLang === 'zh-tw' || curLang === 'zh') {
                normalizedCurLang = 'zh-TW'
            } else if (curLang === 'en' || curLang === 'en-tw') {
                normalizedCurLang = 'en-US'
            }

            // 切換到另一種語言
            const changeLang = (normalizedCurLang === 'zh-TW' || normalizedCurLang.includes('zh')) ? 'en-US' : 'zh-TW'

            try {
                let translations

                // 檢查快取
                if (translationCache.has(changeLang)) {
                    translations = translationCache.get(changeLang)
                } else {
                    // 從 API 取得翻譯
                    const response = await fetch(`/api/translation/${changeLang}`)
                    if (!response.ok) throw new Error(`HTTP ${response.status}`)
                    translations = await response.json()
                    translationCache.set(changeLang, translations)
                }

                // 更新頁面文字
                this.updatePageText(translations)

                // 設定 cookie
                const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`

                // 更新 html lang 屬性和全域狀態
                document.documentElement.lang = changeLang
                globalState.currentLanguage = changeLang

                // 觸發自定義事件
                window.dispatchEvent(new CustomEvent('language-changed', {
                    detail: { 
                        language: changeLang, 
                        langData: globalState.availableLanguages.find(l => l.code === changeLang)
                    }
                }))

                console.log(`語系已切換至: ${changeLang}`)

            } catch (error) {
                console.error('Error switching language:', error)
                // 錯誤處理邏輯
                const cachedTranslations = translationCache.get(changeLang)
                if (cachedTranslations) {
                    this.updatePageText(cachedTranslations)
                    const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                    document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`
                    document.documentElement.lang = changeLang
                    globalState.currentLanguage = changeLang
                } else {
                    alert('Language switching failed. Please try again.')
                }
            }
        },

        // 更新頁面文字 - 參考 menu.js
        updatePageText(translations) {
            if (!translations || typeof translations !== 'object') {
                console.error('Invalid translations object:', translations)
                return
            }

            // 找到所有帶有 data-i18n 屬性的元素
            document.querySelectorAll('[data-i18n]').forEach(element => {
                const key = element.getAttribute('data-i18n')

                if (translations[key]) {
                    if (element.tagName === 'INPUT' && (element.type === 'submit' || element.type === 'button')) {
                        element.value = translations[key]
                    } else if (element.placeholder !== undefined) {
                        element.placeholder = translations[key]
                    } else {
                        element.textContent = translations[key]
                    }
                }
            })

            // 處理 data-i18n-placeholder 屬性的元素
            document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
                const key = element.getAttribute('data-i18n-placeholder')
                if (translations[key]) element.placeholder = translations[key]
            })

            // 更新頁面標題
            if (translations['Title']) document.title = translations['Title']

            // 呼叫翻譯回調函數
            if (window.profileTranslationCallbacks) {
                window.profileTranslationCallbacks.forEach(callback => {
                    if (typeof callback === 'function') {
                        try {
                            callback(translations)
                        } catch (error) {
                            console.error('Profile translation callback error:', error)
                        }
                    }
                })
            }
        },

        // 初始化語系
        initLanguage() {
            const currentLang = document.documentElement.lang
            if (currentLang && globalState.availableLanguages.find(l => l.code === currentLang)) {
                globalState.currentLanguage = currentLang
            }
        },


        // 載入選單和使用者資料 - 使用伺服端注入的初始資料
        async loadMenuData(forceRefresh = false) {
            // 如果已有請求在進行中且不強制刷新，返回現有的 Promise
            if (menuDataPromise && !forceRefresh) return menuDataPromise

            // 以同步方式將伺服端資料放入全域狀態（包裝 Promise 介面）
            menuDataPromise = (async () => {
                try {
                    globalState.loading.menu = true
                    globalState.loading.global = true

                    const menuData = initialMenu || null
                    if (menuData) {
                        globalState.menuData = menuData
                        // 從初始資料更新 currentUser（屬性名稱為 PascalCase）
                        Object.assign(globalState.currentUser, {
                            isAuthenticated: !!menuData.IsAuthenticated,
                            userId: menuData.UserId || null,
                            username: menuData.UserName || '',
                            email: menuData.UserEmail || '',
                            role: menuData.UserRole || 0,
                            status: menuData.UserStatus || 0,
                            isAdmin: (menuData.UserRole || 0) >= 1,
                            isMember: !menuData.IsGuest
                        })
                        return menuData
                    }
                    return null
                } finally {
                    globalState.loading.menu = false
                    globalState.loading.global = false
                    menuDataPromise = null
                }
            })()

            return menuDataPromise
        },

        // 設定主題
        setTheme(isDark) {
            globalState.theme.isDark = isDark
            localStorage.setItem('dashboard-theme', isDark ? 'dark' : 'light')
            
            window.dispatchEvent(new CustomEvent('theme-changed', {
                detail: { isDark }
            }))
        },

        // 初始化主題
        initTheme() {
            const savedTheme = localStorage.getItem('dashboard-theme')
            if (savedTheme) {
                globalState.theme.isDark = savedTheme === 'dark'
            }
        }
    }

    // Composable 函數供 Vue 實例使用
    window.useGlobalState = function() {
        return {
            // 狀態
            globalState: Vue.readonly(globalState),
            
            // 語系相關
            currentLanguage: Vue.toRef(globalState, 'currentLanguage'),
            availableLanguages: Vue.toRef(globalState, 'availableLanguages'),
            
            // 使用者狀態
            currentUser: Vue.toRef(globalState, 'currentUser'),
            
            // 主題
            theme: Vue.toRef(globalState, 'theme'),
            
            // 載入狀態
            loading: Vue.toRef(globalState, 'loading'),
            
            // 選單資料
            menuData: Vue.toRef(globalState, 'menuData'),
            
            // 方法
            toggleLang: globalActions.toggleLang.bind(globalActions),
            updatePageText: globalActions.updatePageText.bind(globalActions),
            loadMenuData: globalActions.loadMenuData.bind(globalActions),
            setTheme: globalActions.setTheme.bind(globalActions),
            
            // 強制刷新方法
            refreshData: () => globalActions.loadMenuData(true)
        }
    }

    // 全域初始化函數
    window.initGlobalState = async function() {
        globalActions.initLanguage()
        globalActions.initTheme()
        await globalActions.loadMenuData()  // 一次載入選單和使用者資料
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
                        toggleLang,
                        loadMenuData,
                        setTheme
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

                    // 主題切換
                    const toggleTheme = function() {
                        setTheme(!theme.value.isDark)
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
                                console.log('Logout successful')
                                window.location.href = '/'
                            } else {
                                console.error('Logout failed')
                            }
                        } catch (error) {
                            console.error('Error during logout:', error)
                        }
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

                    // 登入跳轉功能
                    const login = function() {
                        window.location.href = '/login'
                    }

                    // 載入 Dashboard 頁面內容（SSR 導航）
                    const loadDashboardPage = function(page) {
                        const capitalizedPage = page.charAt(0).toUpperCase() + page.slice(1)
                        const targetUrl = `/Dashboard/${capitalizedPage}`
                        
                        // 檢查是否為當前頁面，如果是則不跳轉
                        const currentPath = window.location.pathname
                        if (currentPath === targetUrl || 
                            currentPath === targetUrl + '/' || 
                            currentPath + '/' === targetUrl) {
                            console.log(`已在目標頁面 ${targetUrl}，跳過導航`)
                            return
                        }
                        
                        // 中止目前頁面相關的 DB GET 請求
                        if (typeof window.abortAllDbGets === 'function') window.abortAllDbGets()
                        // 直接 SSR 導航到完整頁面
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

                        console.log('Dashboard Menu App 已載入')
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
                        getCurrentLanguageInfo
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
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                const menuApp = createMenuApp()
                if (document.querySelector('#dashboard-menu')) {
                    window.DashboardMenuApp = menuApp.mount('#dashboard-menu')
                }
            })
        } else {
            const menuApp = createMenuApp()
            if (document.querySelector('#dashboard-menu')) {
                window.DashboardMenuApp = menuApp.mount('#dashboard-menu')
            }
        }
    }

    // 將狀態和方法暴露到 window
    window.dashboardGlobalState = {
        state: globalState,
        actions: globalActions,
        useGlobalState: window.useGlobalState
    }

    // 將翻譯相關函數暴露到全域（相容性）
    window.updatePageText = globalActions.updatePageText.bind(globalActions)
    window.translationCache = translationCache

    // 初始化全域狀態並啟動應用
    window.initGlobalState().then(() => {
        console.log('全域狀態初始化完成')
        DashboardMenuApp()
    })

})();
