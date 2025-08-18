import { useFormatting } from '/js/hooks/useFormatting.js'
import { useMenu } from '/js/components/menu.js'
import { useHome } from '/js/pages/home/home.js'
import { useProfile } from '/js/pages/profile/profile.js'
import { useReply } from '/js/components/reply.js'
import loginPopupManager from '/js/auth/login-popup.js'

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

// 將需要被內嵌 HTML 調用的單例暴露到全域
window.loginPopupManager = loginPopupManager

globalApp({
    setup() {
        const { ref, reactive, computed, watch, onMounted, onUnmounted } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)
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
        //#endregion

        //#region Friends Data
        const friends = ref([])
        const friendsLoading = ref(false)
        const friendsTotal = ref(0)
        const friendsStatus = ref('accepted')

        const getUsernameFromPath = () => {
            const parts = window.location.pathname.split('/').filter(Boolean)
            if (parts[0]?.toLowerCase() === 'profile' && parts[1] && parts[1].toLowerCase() !== 'index') {
                return parts[1]
            }
            return null
        }

        const loadFriends = async (page = 1, pageSize = 20, username = null, status = friendsStatus.value) => {
            try {
                friendsLoading.value = true
                const { friendsService } = await import('/js/components/friends.js')

                const targetUsername = username || getUsernameFromPath() || currentUser.username || null
                const result = await friendsService.getFriends(page, pageSize, targetUsername, status)

                if (result.success) {
                    friends.value = result.friends
                    friendsTotal.value = result.totalCount
                } else if (result.unauthorized) {
                    friends.value = []
                    friendsTotal.value = 0
                } else {
                    console.error('Failed to load friends:', result.error)
                }
            } catch (err) {
                console.error('Error loading friends:', err)
            } finally {
                friendsLoading.value = false
            }
        }
        const changeFriendsStatus = (status) => {
            friendsStatus.value = status
            // 預設重新載入列表
            loadFriends(1, 20, null, friendsStatus.value)
        }
        //#endregion

        //#region 獲取用戶信息

        const getCurrentUser = async () => {
            try {
                const { authService } = await import('/js/services/AuthService.js')
                if (authService) {
                    const authStatus = await authService.getAuthStatus()

                    if (authStatus.success && authStatus.data.authenticated) {
                        const user = authStatus.data.user

                        Object.assign(currentUser, {
                            isAuthenticated: true,
                            userId: user.id,
                            username: user.username,
                            email: user.email,
                            role: user.role || 0,
                            status: user.status || 0,
                            isAdmin: user.isAdmin || false,
                            isMember: user.isMember || true
                        })
                    } else {
                        // 未認證狀態
                        Object.assign(currentUser, {
                            isAuthenticated: false,
                            userId: null
                        })
                    }
                } else {
                    console.warn('AuthService not available, using direct API call')
                    // Fallback 直接 API 呼叫（理論上不會進入）
                    const response = await fetch('/api/auth/status')
                    const data = await response.json()

                    if (data.success && data.data.authenticated) {
                        const user = data.data.user
                        Object.assign(currentUser, {
                            isAuthenticated: true,
                            userId: user.id,
                            username: user.username,
                            email: user.email,
                            role: user.role || 0,
                            status: user.status || 0,
                            isAdmin: user.isAdmin || false,
                            isMember: user.isMember || true
                        })
                    } else {
                        // 未認證狀態
                        Object.assign(currentUser, {
                            isAuthenticated: false,
                            userId: null
                        })
                    }
                }
            } catch (err) {
                console.error('獲取用戶信息失敗:', err)
                Object.assign(currentUser, {
                    isAuthenticated: false,
                    userId: null
                })
            }
        }

        // 將 currentUser 設為全局可訪問
        window.currentUser = currentUser

        //#endregion
        const isAppReady = ref(false)

        onMounted(() => {
            isAppReady.value = true

            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''

        })


        //#region 匯入各頁面的 Vue 模組（ESM）

        const LoadingPage = (pattern, useFunc) => {
            const path = window.location.pathname.toLowerCase()
            const matched = pattern instanceof RegExp
                ? pattern.test(path)
                : path.includes(String(pattern).toLowerCase())

            if (!matched) return {}
            try {
                return typeof useFunc === 'function' ? useFunc() : {}
            } catch (error) {
                console.error('頁面模組載入失敗:', error)
                return {}
            }
        }

        // 路徑偵測（供後續邏輯使用）
        const currentPath = window.location.pathname.toLowerCase()
        const isHomePage = /^\/(?:home(?:\/|$))?$|^\/$/.test(currentPath)
        const isProfilePage = /^\/profile(?:\/|$)/.test(currentPath)

        // 組件/頁面模組
        const Menu = (typeof useMenu === 'function') ? useMenu() : {}
        const Home = LoadingPage(/^\/(?:home(?:\/|$))?$|^\/$/i, useHome)
        const Profile = LoadingPage(/^\/profile(?:\/|$)/i, useProfile)
        const Reply = LoadingPage(/^\/reply(?:\/|$)/i, useReply)
        // const Friends = LoadingPage(/^\/friends(?:\/|$)/i, useFriends)

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
            const { postListService } = await import('/js/components/PostListService.js')
            if (!postListService) return { success: false, articles: [] }

            postListLoading.value = true

            try {
                const result = await postListService.getPosts(page, pageSize, uid, isProfilePage)

                if (result.success) {
                    const formattedArticles = postListService.formatArticles(result.articles)
                    return { success: true, articles: formattedArticles, totalCount: result.totalCount }
                } else if (result.requireLogin) {
                    // 將需要登入的訊息往上回傳，由呼叫端處理提示
                    return { success: false, requireLogin: true, message: result.message, articles: [] }
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
                // 訪客模式：後端在第二次請求返回 403，前端顯示提示
                if (result.requireLogin || !currentUser.isAuthenticated) {
                    alert(result?.message || '請登入以繼續瀏覽更多內容')
                }
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
                        // console.log('Loading more posts...', { currentPage: currentPage.value })
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
                    personId: user.personId,
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

        //#region Global Action Functions

        // 定義全域動作函數供 PostList 使用
        window.praize = (articleId) => {
            console.log('Praise action for article:', articleId)
            // TODO: Implement praise API call
        }

        window.comment = (articleId) => {
            console.log('Comment action for article:', articleId)
            // TODO: Implement comment functionality
        }

        window.collect = (articleId) => {
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

            // 若頁面包含好友列表區塊，載入好友
            if (document.querySelector('.friends-list')) {
                loadFriends(1, 20, null, friendsStatus.value)
            }
        })

        //#endregion

        console.log('✅ setup() 成功初始化，searchQuery =', searchQuery.value)
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
            // 向後相容
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            isAppReady,

            // hooks
            formatDate,
            timeAgo,
            searchQuery,
            manualSearch,
            // menu functions (spread from useMenu)
            ...Menu,
            ...Profile,
            ...Home,
            ...Reply,
            // friends
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus,
        }
    }
})
