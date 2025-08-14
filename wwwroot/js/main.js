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
        const { ref, reactive, computed, watch, onMounted } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)
        const isAppReady = ref(false)


        onMounted(() => {
            isAppReady.value = true

            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''
        })

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
                if (typeof useProfile === 'function') {
                    profileFunctions = useProfile()
                    // console.log('Profile 模組載入成功')
                } else {
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

        const searchQuery = ref('')


        // Popup Data Storage
        const popupData = reactive({
            Search: {
                Users: [],
                Hashtags: []
            },
            Notify: [],
            Follows: [],
            Collects: []
        })


        watch(searchQuery, (newVal) => {
            console.log('👀 searchQuery 改變：', newVal)
        })

        // 當 openPopup 的類型是 Search 的時候，清空 searchQuery
        //watch(() => popupState.type, (newType) => {
        //    if (newType === 'Search') {
        //        searchQuery.value = ''
        //        popupData.Search = []
        //    }
        //})

        const manualSearch = async () => {
            console.log('🔍 手動搜尋按鈕觸發！', searchQuery)

            const keyword = searchQuery.value

            if (!keyword || keyword.trim().length < 1) {
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            isLoading.value = true
            try {
                const [userRes, tagRes] = await Promise.all([
                    fetch(`/api/search/users?keyword=${encodeURIComponent(keyword)}`),
                    fetch(`/api/search/hashtags?keyword=${encodeURIComponent(keyword)}`)
                ])

                const users = await userRes.json()
                const tags = await tagRes.json()


                popupData.Search.Users = users.data.map(user => ({
                    displayName: user.displayName,
                    avatarUrl: user.avatarPath,
                    bio: user.bio || '這位使用者尚未填寫個人簡介。'
                }))

                popupData.Search.Hashtags = tags.data
                console.log('🎯 搜尋結果資料：', popupData.Search)
            } catch (err) {
                console.error('Search API Error:', err)
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
            } finally {
                isLoading.value = false
            }
        }


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

            console.log('🧠 開啟 popup：', popupState.type)

            if (type === 'Search') {
                searchQuery.value = ''
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            isLoading.value = true   // 👈 加上這行：開始 loading

            try {
                const res = await fetch('/api/' + type.toLowerCase(), {
                    method: 'GET',
                    credentials: 'include'  // ✅ 加這行就會自動帶 cookie
                })
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

        console.log('✅ setup() 成功初始化，searchQuery =', searchQuery.value)
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

            isAppReady,

            // hooks
            formatDate,
            timeAgo,
            searchQuery,
            manualSearch,
            // menu functions (spread from useMenu)
            ...menuFunctions,

            // profile functions (only available on profile page)
            ...profileFunctions,
        }
    }
})