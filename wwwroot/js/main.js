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
        const { ref, reactive, computed, watch, onMounted } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)
        const isAppReady = ref(false)


        onMounted(() => {
            isAppReady.value = true

            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''
        })


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

            isAppReady,

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