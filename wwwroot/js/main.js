const globalApp = content => {
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
                window.globalApp = app.mount('#app')
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
            window.globalApp = app.mount('#app')
        }
    }
}

globalApp({
    setup() {
        const { reactive, ref, computed, onMounted } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)

        //#region User Authentication State
        
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

        // 獲取當前用戶信息
        const getCurrentUser = async () => {
            try {
                if (window.authService) {
                    const authStatus = await window.authService.getAuthStatus()
                    
                    if (authStatus.success && authStatus.data.authenticated) {
                        const user = authStatus.data.user
                        currentUser.isAuthenticated = true
                        currentUser.userId = user.id
                        currentUser.username = user.username
                        currentUser.email = user.email
                        currentUser.role = user.role || 0
                        currentUser.status = user.status || 0
                        currentUser.isAdmin = user.isAdmin || false
                        currentUser.isMember = user.isMember || true
                    } else {
                        // 未認證狀態
                        currentUser.isAuthenticated = false
                        currentUser.userId = null
                    }
                } else {
                    console.warn('AuthService not available, using direct API call')
                    // Fallback to direct API call (should rarely happen)
                    const response = await fetch('/api/auth/status')
                    const data = await response.json()
                    
                    if (data.success && data.data.authenticated) {
                        const user = data.data.user
                        currentUser.isAuthenticated = true
                        currentUser.userId = user.id
                        currentUser.username = user.username
                        currentUser.email = user.email
                        currentUser.role = user.role || 0
                        currentUser.status = user.status || 0
                        currentUser.isAdmin = user.isAdmin || false
                        currentUser.isMember = user.isMember || true
                    } else {
                        // 未認證狀態
                        currentUser.isAuthenticated = false
                        currentUser.userId = null
                    }
                }
            } catch (err) {
                console.error('獲取用戶信息失敗:', err)
                currentUser.isAuthenticated = false
                currentUser.userId = null
            }
        }

        // 將 currentUser 設為全局可訪問
        window.currentUser = currentUser

        //#endregion

        //#region Page Detection and Profile Integration
        
        // 檢測是否為 Profile 頁面
        const isProfilePage = window.location.pathname.toLowerCase().includes('/profile')
        let profileFunctions = {}
        
        // 如果是 Profile 頁面，載入 Profile 功能
        if (isProfilePage) {
            try {
                // 優先使用新的 useProfile (Vue 3 風格)
                if (typeof useProfile === 'function') {
                    profileFunctions = useProfile()
                    console.log('Profile 模組載入成功 (Vue 3 風格)')
                }
                // 回退到舊的 useProfileVue2 (Vue 2 風格)
                else if (typeof useProfileVue2 === 'function') {
                    const profileModule = useProfileVue2()
                    
                    // 簡化的轉換：將 Vue2 data() 轉換為響應式數據
                    const profileData = profileModule.data()
                    const reactiveData = {}
                    
                    // 將所有數據轉換為響應式
                    Object.keys(profileData).forEach(key => {
                        const value = profileData[key]
                        if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
                            reactiveData[key] = reactive(value)
                        } else {
                            reactiveData[key] = ref(value)
                        }
                    })
                    
                    // 創建一個包含所有功能的對象
                    profileFunctions = {
                        // 響應式數據
                        ...reactiveData
                    }
                    
                    // 添加 methods，確保 this 綁定正確
                    if (profileModule.methods) {
                        Object.keys(profileModule.methods).forEach(key => {
                            profileFunctions[key] = profileModule.methods[key].bind(profileFunctions)
                        })
                    }
                    
                    // 添加 computed 屬性
                    if (profileModule.computed) {
                        Object.keys(profileModule.computed).forEach(key => {
                            profileFunctions[key] = computed(() => {
                                return profileModule.computed[key].call(profileFunctions)
                            })
                        })
                    }
                    
                    // 執行 Vue2 的 mounted 生命週期
                    if (profileModule.mounted) {
                        onMounted(() => {
                            // 使用 Vue.nextTick 確保 DOM 已更新
                            Vue.nextTick(() => {
                                profileModule.mounted.call(profileFunctions)
                            })
                        })
                    }
                    
                    console.log('Profile 模組載入成功 (Vue 2 風格)')
                }
                else {
                    console.warn('找不到 Profile 模組函數')
                }
            } catch (error) {
                console.error('Profile 模組載入失敗:', error)
                profileFunctions = {}
            }
        }

        //#endregion

        //#region Pop-Up Events

        // Popup State
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        // Popup Data Storage
        const popupData = reactive({
            Search: [],
            Notify: [],
            Follows: [],
            Collects: []
        })

        // popup helper
        const getPopupTitle = type => {
            const titles = {
                'Search': '搜尋',
                'Notify': '通知',
                'Follows': '追蹤',
                'Collects': '收藏'
            }

            return titles[type] || '視窗'
        }

        // Update popup data
        const updatePopupData = (type, data) => {
            if (popupData[type] !== undefined) popupData[type] = data
        }

        // Popup click
        const openPopup = async type => {
            popupState.type = type
            popupState.title = getPopupTitle(type)
            popupState.isVisible = true
            isLoading.value = true   // 👈 加上這行：開始 loading

            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()

                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            } finally {
                isLoading.value = false
            }  // 👈 加上這行：結束 loading
        }

        const closePopup = () => {
            popupState.isVisible = false
            popupState.type = ''
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

        //#endregion

        //#region Menu Integration

        // Import menu functionality
        const menuFunctions = useMenu()

        //#endregion

        //#region Lifecycle
        
        // 組件掛載時獲取用戶信息
        onMounted(async () => {
            await getCurrentUser()
        })

        //#endregion

        return {
            // user state
            currentUser,
            getCurrentUser,
            
            // pop-up
            popupState,
            popupData,
            isLoading, //加入 loading 狀態
            getPopupTitle,
            openPopup,
            closePopup,
            // 為新版 popup 提供向後兼容
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            // hooks
            formatDate,
            timeAgo,

            // menu functions (spread from useMenu)
            ...menuFunctions,

            // profile functions (only available on profile page)
            ...profileFunctions,
        }
    }
})