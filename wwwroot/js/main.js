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

        // 小工具：把搜尋使用者轉成 Follows 清單的資料形狀
        const mapSearchUserToFollowItem = (u) => ({
            personId: u.personId,
            senderName: u.displayName,
            senderAvatarUrl: u.avatarUrl || '/static/img/cute.png',
            followTime: new Date().toISOString()
        })

        const toggleFollow = async (targetPersonId, currentStatus) => {
            if (!currentUser.isAuthenticated) {
                alert('請先登入才能追蹤')
                return
            }

            // ✅ 防呆：targetPersonId 必須存在
            if (!targetPersonId) {
                console.warn('toggleFollow: targetPersonId 為空，取消請求')
                return
            }
            try {
                const method = currentStatus ? 'DELETE' : 'POST'
                const res = await fetch(`/api/follows/${targetPersonId}`, {
                    method,
                    credentials: 'include'
                })

                // ✅ 500/HTML 錯誤頁防呆
                const ct = res.headers.get('content-type') || ''
                let result
                if (ct.includes('application/json')) {
                    result = await res.json()
                } else {
                    const text = await res.text()
                    console.error('Follow API raw:', text)
                    alert('伺服器錯誤：' + text.slice(0, 140))
                    return
                }

                if (!res.ok || !result?.success) {
                    alert(result?.message || '操作失敗，請稍後再試')
                    return
                }

                // ✅ 同步更新「搜尋結果」按鈕狀態
                const u = popupData.Search.Users.find(u => u.personId === targetPersonId)
                if (u) u.isFollowed = !currentStatus

                if (currentStatus === true) {
                    // ✅ 取消追蹤：從清單移除
                    popupData.Follows = popupData.Follows.filter(f => f.personId !== targetPersonId)
                } else {
                    // ✅ 追蹤：若清單沒有，樂觀加入一筆
                    const exists = popupData.Follows.some(f => f.personId === targetPersonId)
                    if (!exists) {
                        const item = u
                            ? {
                                personId: u.personId,
                                senderName: u.displayName,
                                senderAvatarUrl: u.avatarUrl || '/static/img/cute.png',
                                followTime: new Date().toISOString()
                            }
                            : {
                                personId: targetPersonId,
                                senderName: '已追蹤使用者',
                                senderAvatarUrl: '/static/img/cute.png',
                                followTime: new Date().toISOString()
                            }
                        popupData.Follows.unshift(item)
                    }
                }

                // ✅ 若此時關鍵字已清空、且身在 Follows 視窗 → 重抓一次清單（確保與後端一致）
                const kw = (searchQuery.value || '').trim()
                if (popupState.type === 'Follows' && kw === '' && typeof fetchFollows === 'function') {
                    await fetchFollows()
                }
            } catch (err) {
                console.error('追蹤操作錯誤：', err)
            }
        }

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
                    bio: user.bio || '這位使用者尚未填寫個人簡介。',
                    isFollowed: user.isFollowed,     // ✅ 已有
                    personId: user.personId          // ✅ 需要這個來傳給 API
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

        // ✅ 專用於 Follows 浮窗：只抓使用者
        const manualFollowSearch = async () => {
            const keyword = (searchQuery.value || '').trim()
            // 空字串就清空
            if (!keyword) {
                popupData.Search.Users = []
                if(popupState.type==='Follow')fetchFollows() //讓畫面立刻顯示最新清單
                return
            }

            isLoading.value = true
            try {
                const res = await fetch(`/api/follows/search?keyword=${encodeURIComponent(keyword)}`, {
                    credentials: 'include'
                })
                const json = await res.json()
                const users = Array.isArray(json?.data) ? json.data : []

                popupData.Search.Users = users.map(u => ({
                    personId: u.personId,
                    displayName: u.displayName,
                    avatarUrl: u.avatarPath,
                    isFollowed: u.isFollowed,
                    bio: u.bio || '這位使用者尚未填寫個人簡介。'
                }))

                // ✅ 不處理/不改動 Hashtags（保持為空）
                // popupData.Search.Hashtags = []
            } catch (e) {
                console.error('Follow popup search error:', e)
                popupData.Search.Users = []
            } finally {
                isLoading.value = false
            }
        }

        // 共用搜尋按鈕的 handler：Follows 視窗 → 只搜使用者；其它 → 原本搜尋
        const onSearchClick = () => {
            if (popupState.type === 'Follows') {
                return manualFollowSearch()
            }
            return manualSearch()
        }

        // 取代你 openFollows 裡的抓資料段，抽成可重用方法
        const fetchFollows = async () => {
            isLoading.value = true
            try {
                const res = await fetch(`/api/follows?page=1&pageSize=10`, { credentials: 'include' })
                const result = await res.json()
                const list = Array.isArray(result?.data) ? result.data : (result?.data?.items ?? [])
                popupData.Follows = list
            } catch (e) {
                console.error('❌ 載入 Follows 失敗', e)
                popupData.Follows = []
            } finally {
                isLoading.value = false
            }
        }

        const openFollows = async () => {
            popupState.type = 'Follows'
            popupState.title = getPopupTitle('Follows')
            popupState.isVisible = true
            popupData.Search.Hashtags = []
            await fetchFollows()

            isLoading.value = true

            try {
                const res = await fetch(`/api/follows?page=1&pageSize=10`, {
                    method: 'GET',
                    credentials: 'include'
                })
                const result = await res.json()

                // ✅ 容錯：同時支援 data 是陣列或是物件(items)
                const list = Array.isArray(result?.data)
                    ? result.data
                    : (result?.data?.items ?? [])

                // ✅ 做大小寫兼容 & 預設值
                popupData.Follows = list
            } catch (e) {
                console.error('❌ 載入 Follows 失敗', e)
                popupData.Follows = []
            } finally {
                isLoading.value = false
            }
        }
        watch(searchQuery, (val) => {
            const kw = (val || '').trim()
            // 只有在 Follows 視窗時才做自動刷新
            if (popupState.type === 'Follows' && kw === '') {
                // 清掉搜尋結果
                popupData.Search.Users = []
                // 重新抓追蹤清單
                fetchFollows()
            }
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
            if (type === 'Follows') {
                return openFollows()  // 👈 直接走新流程
            }

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
            fetchFollows,
            isAppReady,
            onSearchClick,

            manualFollowSearch,
            toggleFollow,

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