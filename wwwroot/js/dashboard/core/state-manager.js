// State Manager - 處理 Dashboard 全域狀態管理（從 d_main.js 抽出）

(function() {
    'use strict';

    // 初始資料（由伺服端注入）
    const initialMenu = (typeof window !== 'undefined' && window.__INITIAL_MENU__) ? window.__INITIAL_MENU__ : null
    
    // API 請求快取，防止重複請求
    let menuDataPromise = null

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

    // 狀態管理器
    const StateManager = {
        // 獲取全域狀態
        getState() {
            return Vue.readonly(globalState)
        },

        // 更新使用者狀態
        updateUser(userData) {
            Object.assign(globalState.currentUser, userData)
        },

        // 更新語言狀態
        updateLanguage(langCode) {
            globalState.currentLanguage = langCode
        },

        // 更新主題狀態
        updateTheme(themeData) {
            Object.assign(globalState.theme, themeData)
        },

        // 設置載入狀態
        setLoading(type, isLoading) {
            if (globalState.loading.hasOwnProperty(type)) {
                globalState.loading[type] = isLoading
            }
        },

        // 載入選單和使用者資料
        async loadMenuData(forceRefresh = false) {
            // 如果已有請求在進行中且不強制刷新，返回現有的 Promise
            if (menuDataPromise && !forceRefresh) return menuDataPromise

            // 以同步方式將伺服端資料放入全域狀態
            menuDataPromise = (async () => {
                try {
                    this.setLoading('menu', true)
                    this.setLoading('global', true)

                    const menuData = initialMenu || null
                    if (menuData) {
                        globalState.menuData = menuData
                        
                        // 從初始資料更新 currentUser（屬性名稱為 PascalCase）
                        this.updateUser({
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
                    this.setLoading('menu', false)
                    this.setLoading('global', false)
                    menuDataPromise = null
                }
            })()

            return menuDataPromise
        },

        // 強制刷新資料
        async refreshData() {
            return this.loadMenuData(true)
        },

        // 重置使用者狀態
        resetUser() {
            this.updateUser({
                isAuthenticated: false,
                userId: null,
                username: '',
                email: '',
                role: 0,
                status: 0,
                isAdmin: false,
                isMember: false
            })
        },

        // 初始化語系
        initLanguage() {
            const currentLang = document.documentElement.lang
            if (currentLang && globalState.availableLanguages.find(l => l.code === currentLang)) {
                globalState.currentLanguage = currentLang
            }
        },

        // 獲取當前使用者資訊
        getCurrentUser() {
            return Vue.readonly(globalState.currentUser)
        },

        // 獲取選單資料
        getMenuData() {
            return Vue.readonly(globalState.menuData)
        },

        // 檢查使用者權限
        hasPermission(requiredRole = 0) {
            return globalState.currentUser.isAuthenticated && globalState.currentUser.role >= requiredRole
        },

        // 檢查是否為管理員
        isAdmin() {
            return globalState.currentUser.isAuthenticated && globalState.currentUser.isAdmin
        }
    }

    // Composable 函數供 Vue 實例使用
    const useGlobalState = function() {
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
            loadMenuData: StateManager.loadMenuData.bind(StateManager),
            refreshData: StateManager.refreshData.bind(StateManager),
            updateUser: StateManager.updateUser.bind(StateManager),
            updateLanguage: StateManager.updateLanguage.bind(StateManager),
            updateTheme: StateManager.updateTheme.bind(StateManager),
            setLoading: StateManager.setLoading.bind(StateManager),
            hasPermission: StateManager.hasPermission.bind(StateManager),
            isAdmin: StateManager.isAdmin.bind(StateManager)
        }
    }

    // 全域初始化函數
    const initGlobalState = async function() {
        StateManager.initLanguage()
        await StateManager.loadMenuData()
    }

    // 暴露到全域
    window.DashboardStateManager = StateManager
    window.useGlobalState = useGlobalState
    window.initGlobalState = initGlobalState

    // 向後相容性
    window.dashboardGlobalState = {
        state: globalState,
        actions: StateManager,
        useGlobalState: useGlobalState
    }

})();
