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
        const isHomePage = window.location.pathname === '/' || window.location.pathname.toLowerCase() === '/home'
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
        
        //#region PostList Data (共用於所有使用 PostList ViewComponent 的頁面)
        
        // PostList 相關的狀態
        const posts = ref([])
        const postListLoading = ref(false)
        const hasMorePosts = ref(true)
        const currentPage = ref(1)
        let infiniteScrollObserver = null
        
        // PostList 相關的方法
        const stateFunc = (action, articleId) => {
            console.log(`Action: ${action?.name || action}, Article ID: ${articleId}`)
            
            if (!currentUser.isAuthenticated) {
                alert('請先登入才能進行此操作')
                return
            }

            // Call appropriate action
            if (typeof action === 'function') {
                action(articleId)
            }
        }
        
        // 載入文章的通用方法
        const loadPosts = async (page = 1, pageSize = 10, uid = null, isProfilePage = false) => {
            if (!window.postListService) {
                console.error('PostListService not available')
                return { success: false, articles: [] }
            }

            postListLoading.value = true
            
            try {
                const result = await window.postListService.getPosts(page, pageSize, uid, isProfilePage)
                
                if (result.success) {
                    const formattedArticles = window.postListService.formatArticles(result.articles)
                    return { success: true, articles: formattedArticles, totalCount: result.totalCount }
                } else {
                    console.error('Failed to load posts:', result.error)
                    return { success: false, articles: [] }
                }
            } catch (error) {
                console.error('Error loading posts:', error)
                return { success: false, articles: [] }
            } finally {
                postListLoading.value = false
            }
        }

        // 載入更多文章（用於無限滾動）
        const loadMorePosts = async (uid = null, isProfilePage = false) => {
            if (!hasMorePosts.value || postListLoading.value) return
            
            const nextPage = currentPage.value + 1
            const result = await loadPosts(nextPage, 10, uid, isProfilePage)
            
            if (result.success && result.articles.length > 0) {
                // 追加新文章到現有列表
                posts.value = [...posts.value, ...result.articles]
                currentPage.value = nextPage
                
                // 檢查是否還有更多文章
                hasMorePosts.value = result.articles.length === 10
            } else {
                hasMorePosts.value = false
            }
        }

        // 設置無限滾動
        const setupInfiniteScroll = (uid = null, isProfilePage = false) => {
            // 清理之前的 Observer
            if (infiniteScrollObserver) {
                infiniteScrollObserver.disconnect()
            }

            // 尋找觸發元素（由 PostList ViewComponent 提供）
            const triggerElement = document.querySelector('.infinite-scroll-trigger')
            if (!triggerElement) {
                console.warn('Infinite scroll trigger element not found')
                return
            }

            // console.log('Setting up infinite scroll...', { uid, isProfilePage })

            // 設置 Intersection Observer
            infiniteScrollObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting && hasMorePosts.value && !postListLoading.value) {
                        console.log('Loading more posts...', { currentPage: currentPage.value })
                        loadMorePosts(uid, isProfilePage)
                    }
                })
            }, {
                root: null,
                rootMargin: '200px', // 提前 200px 開始載入
                threshold: 0.1
            })

            infiniteScrollObserver.observe(triggerElement)
        }

        // 清理無限滾動
        const cleanupInfiniteScroll = () => {
            if (infiniteScrollObserver) {
                infiniteScrollObserver.disconnect()
                infiniteScrollObserver = null
            }
            
            // 移除觸發元素
            const triggers = document.querySelectorAll('.infinite-scroll-trigger')
            triggers.forEach(trigger => trigger.remove())
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

        //#region Global Action Functions
        
        // 定義全域動作函數供 PostList 使用
        window.praize = function(articleId) {
            console.log('Praise action for article:', articleId)
            // TODO: Implement praise API call
        }
        
        window.comment = function(articleId) {
            console.log('Comment action for article:', articleId)
            // TODO: Implement comment functionality
        }
        
        window.collect = function(articleId) {
            console.log('Collect action for article:', articleId)
            // TODO: Implement collect API call
        }

        //#endregion
        
        //#region Lifecycle
        
        // 組件掛載時獲取用戶信息並初始化頁面數據
        onMounted(async () => {
            await getCurrentUser()
            
            // 如果是首頁，初始化文章列表
            if (isHomePage) {
                // console.log('Initializing Home page posts...')
                
                const result = await loadPosts(1, 10, null, false) // page=1, pageSize=10, uid=null, isProfilePage=false
                if (result.success) {
                    posts.value = result.articles
                    currentPage.value = 1
                    hasMorePosts.value = result.articles.length === 10
                    
                    // 設置無限滾動
                    Vue.nextTick(() => {
                        setupInfiniteScroll(null, false) // Home 頁面不篩選用戶
                    })
                }
            }
        })

        //#endregion

        return {
            // user state
            currentUser,
            getCurrentUser,
            
            // PostList data (共用狀態)
            posts,
            hasMorePosts,
            currentPage,
            stateFunc,
            loadPosts,
            loadMorePosts,
            setupInfiniteScroll,
            cleanupInfiniteScroll,
            
            // pop-up
            popupState,
            popupData,
            isLoading: computed(() => postListLoading.value || isLoading.value), // 合併所有 loading 狀態
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