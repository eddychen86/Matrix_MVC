const globalApp = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue not ready, retrying...')
        return
    } else {
        lucide.createIcons()
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => window.globalApp = Vue.createApp(content).mount('#app'))
        } else {
            // DOM 已經載入完成
            window.globalApp = Vue.createApp(content).mount('#app')
        }
    }
}

globalApp({
    setup() {
        const { reactive, computed, onMounted } = Vue
        const { formatDate, timeAgo } = useFormatting()

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
        
        // 如果是 Profile 頁面且 useProfile 存在，則載入 Profile 功能
        if (isProfilePage && typeof useProfile === 'function') {
            profileFunctions = useProfile()
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

            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()

                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            }
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