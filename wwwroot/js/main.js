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
        const { ref, reactive, computed,watch } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)
        


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
            Search: [],
            Notify: [],
            Follows: [],
            Collects: []
        })


        watch(searchQuery, (newVal) => {
            console.log('👀 searchQuery 改變：', newVal)
        })
        //Search Functionality
        watch(searchQuery, async (newKeyword) => {
            console.log('🔍 searchQuery 被修改為：', newKeyword)
            console.log('🧪 觸發 watch：newKeyword =', newKeyword)
            console.log('🧪 當前 popupState.type =', popupState.type)
            if (popupState.type !== 'Search')
            {
                console.log('⛔ 中止搜尋：popup type 不是 Search')
                return
            }
                        
            if (!newKeyword || newKeyword.trim().length < 1) {
                popupData.Search = []
                return
            }

            isLoading.value = true

            try {
                console.log('🌐 準備打 API:', `/api/search/users?keyword=${encodeURIComponent(newKeyword)}`)
                const res = await fetch(`/api/search/users?keyword=${encodeURIComponent(newKeyword)}`)
                const result = await res.json()

                popupData.Search = result.data.map(item => ({
                    displayName: item.displayName,
                    avatarUrl: item.avatarPath,
                    bio: item.bio || '這位使用者尚未填寫個人簡介。'
                }))
                console.log('🎯 搜尋結果資料：', popupData.Search)
            } catch (err) {
                console.error('Search API Error:', err)
                popupData.Search = []
            } finally {
                isLoading.value = false
            }
        })

        const manualSearch = async () => {
            console.log('🔍 手動搜尋按鈕觸發！')

            const keyword = searchQuery.value

            if (!keyword || keyword.trim().length < 1) {
                popupData.Search = []
                return
            }

            isLoading.value = true
            try {
                const res = await fetch(`/api/search/users?keyword=${encodeURIComponent(keyword)}`)
                const result = await res.json()
                popupData.Search = result.data.map(item => ({
                    displayName: item.displayName,
                    avatarUrl: item.avatarPath,
                    bio: item.bio || '這位使用者尚未填寫個人簡介。'
                }))
                console.log('🎯 搜尋結果資料：', popupData.Search)
            } catch (err) {
                console.error('Search API Error:', err)
                popupData.Search = []
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
                popupData.Search = []
                return
            }

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

        console.log('✅ setup() 成功初始化，searchQuery =', searchQuery.value)
        return {
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
            searchQuery,
            manualSearch,
            // menu functions (spread from useMenu)
            ...menuFunctions,
        }
    }
})