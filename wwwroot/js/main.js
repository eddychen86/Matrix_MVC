import { useFormatting } from '/js/hooks/useFormatting.js'
import { useMenu } from '/js/components/menu.js'
import { useHome } from '/js/pages/home/home.js'
import { useProfile } from '/js/pages/profile/profile.js'
import authManager from '/js/auth/auth-manager.js'
import loginPopupManager from '/js/auth/login-popup.js'
import { useCreatePost } from '/js/components/create-post.js'

const globalApp = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue not ready, retrying...')
        return
    } else {
        lucide.createIcons()
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                const app = Vue.createApp(content)

                if (window.CKEditor?.component) app.component('ckeditor', window.CKEditor.component)
                else if (window.CKEditor) app.use(window.CKEditor)

                // 配置警告處理器來忽略 script/style 標籤警告
                app.config.warnHandler = (msg, instance, trace) => {
                    if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
                        return
                    }
                    console.warn(msg)
                }
                window.globalApp = app.mount('#app')
            })
        } else {
            // DOM 已經載入完成
            const app = Vue.createApp(content)

            if (window.CKEditor) {
                app.use(window.CKEditor)
                console.log('[CKEditor] plugin installed')
            } else {
                console.warn('[CKEditor] wrapper not loaded - check script order')
            }

            // 配置警告處理器來忽略 script/style 標籤警告
            app.config.warnHandler = (msg, instance, trace) => {
                if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
                    return
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
        //#region 宣告變數
        const { reactive, ref, computed, onMounted } = Vue
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
                        Object.assign(currentUser, {
                            isAuthenticated: false,
                            userId: null
                        })
                    }
                } else {
                    console.warn('AuthService not available, using direct API call')
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
        //#endregion

        //#region PostList Data（供 PostList ViewComponent 的頁面共用）
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
            if (typeof action === 'function') {
                action(articleId)
            }
        }

        // 載入文章的通用方法
        const loadPosts = async (page = 1, pageSize = 10, uid = null, isProfile = false) => {
            const { postListService } = await import('/js/components/PostListService.js')
            if (!postListService) return { success: false, articles: [] }

            postListLoading.value = true
            try {
                const result = await postListService.getPosts(page, pageSize, uid, isProfile)
                if (result.success) {
                    const formatted = postListService.formatArticles(result.articles)
                    return { success: true, articles: formatted, totalCount: result.totalCount }
                } else if (result.requireLogin) {
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
        const loadMorePosts = async (uid = null, isProfile = false) => {
            if (!hasMorePosts.value || postListLoading.value) return

            const nextPage = currentPage.value + 1
            const result = await loadPosts(nextPage, 10, uid, isProfile)

            if (result.success && result.articles.length > 0) {
                posts.value = [...posts.value, ...result.articles]
                currentPage.value = nextPage
                hasMorePosts.value = result.articles.length === 10
            } else {
                if (result.requireLogin || !currentUser.isAuthenticated) {
                    alert(result?.message || '請登入以繼續瀏覽更多內容')
                }
                hasMorePosts.value = false
            }
        }

        // 設置無限滾動
        const setupInfiniteScroll = (uid = null, isProfile = false) => {
            if (infiniteScrollObserver) {
                infiniteScrollObserver.disconnect()
            }
            const triggerElement = document.querySelector('.infinite-scroll-trigger')
            if (!triggerElement) {
                console.warn('Infinite scroll trigger element not found')
                return
            }
            infiniteScrollObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting && hasMorePosts.value && !postListLoading.value) {
                        loadMorePosts(uid, isProfile)
                    }
                })
            }, {
                root: null,
                rootMargin: '200px',
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
            const triggers = document.querySelectorAll('.infinite-scroll-trigger')
            triggers.forEach(trigger => trigger.remove())
        }
        //#endregion

        //#region 發文功能（初始化在這裡，讓 onCreated 能操作 posts 與 infinite scroll）
        const postFns = useCreatePost({
            async onCreated(article) {
                try {
                    const { postListService } = await import('/js/components/PostListService.js')
                    const a = postListService.formatArticles([article])[0]
                    posts.value = [a, ...posts.value]

                    // 重新校正無限滾動
                    cleanupInfiniteScroll?.()
                    Vue.nextTick(() => setupInfiniteScroll(null, isProfilePage))
                } catch (e) {
                    // 備援：重抓第一頁；若仍失敗就整頁重載
                    const result = await loadPosts(1, 10, null, isProfilePage)
                    if (result.success) {
                        posts.value = result.articles
                        currentPage.value = 1
                        hasMorePosts.value = result.articles.length === 10
                        cleanupInfiniteScroll?.()
                        Vue.nextTick(() => setupInfiniteScroll(null, isProfilePage))
                    } else {
                        location.reload()
                    }
                }
            }
        })
        //#endregion

        //#region Pop-Up Events
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        const popupData = reactive({
            Search: [],
            Notify: [],
            Follows: [],
            Collects: []
        })

        const getPopupTitle = type => {
            const titles = {
                'Search': '搜尋',
                'Notify': '通知',
                'Follows': '追蹤',
                'Collects': '收藏'
            }
            return titles[type] || '視窗'
        }

        const updatePopupData = (type, data) => {
            if (popupData[type] !== undefined) popupData[type] = data
        }

        const openPopup = async type => {
            popupState.type = type
            popupState.title = getPopupTitle(type)
            popupState.isVisible = true
            isLoading.value = true
            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()
                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            } finally {
                isLoading.value = false
            }
        }

        const closePopup = () => {
            popupState.isVisible = false
            popupState.type = ''
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()
        //#endregion

        //#region Global Action Functions
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
        onMounted(async () => {
            await getCurrentUser()

            // 如果是首頁，初始化文章列表
            if (isHomePage) {
                const result = await loadPosts(1, 10, null, false)
                if (result.success) {
                    posts.value = result.articles
                    currentPage.value = 1
                    hasMorePosts.value = result.articles.length === 10

                    Vue.nextTick(() => {
                        setupInfiniteScroll(null, false)
                    })
                }
            }

            // 若頁面包含好友列表區塊，載入好友
            if (document.querySelector('.friends-list')) {
                loadFriends(1, 20, null, friendsStatus.value)
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
            isLoading: computed(() => postListLoading.value || isLoading.value),
            getPopupTitle,
            openPopup,
            closePopup,
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            // hooks
            formatDate,
            timeAgo,

            // menu functions (spread from useMenu)
            ...Menu,
            ...Profile,
            ...Home,

            // friends
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus,

            // 發文功能
            ...postFns,
        }
    }
})
