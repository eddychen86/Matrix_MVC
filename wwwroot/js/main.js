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
        const mount = () => {
            const app = Vue.createApp(content)
            // 忽略 Vue 對 <script>/<style> 的警告
            app.config.warnHandler = (msg) => {
                if (msg.includes('Tags with side effect') && msg.includes('are ignored in client component templates')) {
                    return
                }
                console.warn(msg)
            }
            window.globalApp = app.mount('#app')
            // icons 在 DOM mount 後初始化較穩
            if (typeof lucide !== 'undefined' && lucide.createIcons) {
                lucide.createIcons()
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', mount)
        } else {
            mount()
        }
    }
}

// 需要讓內嵌 HTML 能調用
window.loginPopupManager = loginPopupManager

globalApp({
    setup() {
        const { ref, reactive, computed, watch, onMounted } = Vue
        const { formatDate, timeAgo } = useFormatting()

        // NOTE: 給彈窗/搜尋等一般用途的 loading
        const uiLoading = ref(false)

        // 留言模組
        const { replyModal, openReply, closeReply, submitReply } = useReply()

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

        //#region 取得登入者
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
                        Object.assign(currentUser, { isAuthenticated: false, userId: null })
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
                        Object.assign(currentUser, { isAuthenticated: false, userId: null })
                    }
                }
            } catch (err) {
                console.error('獲取用戶信息失敗:', err)
                Object.assign(currentUser, { isAuthenticated: false, userId: null })
            }
        }

        // 對外可用
        window.currentUser = currentUser
        //#endregion

        const isAppReady = ref(false)
        onMounted(() => {
            isAppReady.value = true
            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''
        })

        //#region 路由/頁面模組
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

        const currentPath = window.location.pathname.toLowerCase()
        const isHomePage = /^\/(?:home(?:\/|$))?$|^\/$/.test(currentPath)
        const isProfilePage = /^\/profile(?:\/|$)/.test(currentPath)

        const Menu = (typeof useMenu === 'function') ? useMenu() : {}
        const Home = LoadingPage(/^\/(?:home(?:\/|$))?$|^\/$/i, useHome)
        const Profile = LoadingPage(/^\/profile(?:\/|$)/i, useProfile)
        //#endregion

        //#region PostList（共用）
        const posts = ref([])
        const postListLoading = ref(false)
        const hasMorePosts = ref(true)
        const currentPage = ref(1)
        let infiniteScrollObserver = null

        const stateFunc = (action, articleId) => {
            console.log(`Action: ${action?.name || action}, Article ID: ${articleId}`)
            if (!currentUser.isAuthenticated) {
                alert('請先登入才能進行此操作')
                return
            }
            if (typeof action === 'function') action(articleId)
        }

        const loadPosts = async (page = 1, pageSize = 10, uid = null, isProfile = false) => {
            const { postListService } = await import('/js/components/PostListService.js')
            if (!postListService) return { success: false, articles: [] }

            postListLoading.value = true
            try {
                const result = await postListService.getPosts(page, pageSize, uid, isProfile)
                if (result.success) {
                    const formattedArticles = postListService.formatArticles(result.articles)
                    return { success: true, articles: formattedArticles, totalCount: result.totalCount }
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

        const setupInfiniteScroll = (uid = null, isProfile = false) => {
            if (infiniteScrollObserver) infiniteScrollObserver.disconnect()
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
            }, { root: null, rootMargin: '200px', threshold: 0.1 })
            infiniteScrollObserver.observe(triggerElement)
        }

        const cleanupInfiniteScroll = () => {
            if (infiniteScrollObserver) {
                infiniteScrollObserver.disconnect()
                infiniteScrollObserver = null
            }
            const triggers = document.querySelectorAll('.infinite-scroll-trigger')
            triggers.forEach(trigger => trigger.remove())
        }
        //#endregion

        //#region Pop-Up（搜尋/通知/追蹤/收藏）
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        const searchQuery = ref('')

        const popupData = reactive({
            Search: { Users: [], Hashtags: [] },
            Notify: [],
            Follows: [],
            Collects: []
        })

        watch(searchQuery, (newVal) => {
            console.log('👀 searchQuery 改變：', newVal)
        })

        const manualSearch = async () => {
            const keyword = searchQuery.value
            if (!keyword || keyword.trim().length < 1) {
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            uiLoading.value = true
            try {
                const [userRes, tagRes] = await Promise.all([
                    fetch(`/api/search/users?keyword=${encodeURIComponent(keyword)}`),
                    fetch(`/api/search/hashtags?keyword=${encodeURIComponent(keyword)}`)
                ])

                const users = await userRes.json()
                const tags = await tagRes.json()

                popupData.Search.Users = (users.data || []).map(user => ({
                    personId: user.personId,
                    displayName: user.displayName,
                    avatarUrl: user.avatarPath,
                    bio: user.bio || '這位使用者尚未填寫個人簡介。'
                }))
                popupData.Search.Hashtags = tags.data || []
            } catch (err) {
                console.error('Search API Error:', err)
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
            } finally {
                uiLoading.value = false
            }
        }

        const getPopupTitle = type => ({
            Search: '搜尋',
            Notify: '通知',
            Follows: '追蹤',
            Collects: '收藏'
        }[type] || '視窗')

        const updatePopupData = (type, data) => {
            if (popupData[type] !== undefined) popupData[type] = data
        }

        const openPopup = async type => {
            popupState.type = type
            popupState.title = getPopupTitle(type)
            popupState.isVisible = true

            if (type === 'Search') {
                searchQuery.value = ''
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            uiLoading.value = true
            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()
                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            } finally {
                uiLoading.value = false
            }
        }

        const closePopup = () => {
            popupState.isVisible = false
            popupState.type = ''
        }

        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()
        //#endregion

        //#region Lifecycle
        onMounted(async () => {
            await getCurrentUser()

            // 首頁初始化文章
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

            // 若有好友列表區塊就載入
            if (document.querySelector('.friends-list')) {
                loadFriends(1, 20, null, friendsStatus.value)
            }
        })
        //#endregion

        // 供按鈕使用：先檢查登入再開啟留言面板
        const openReplyWithAuth = (articleId) => {
            if (!currentUser.isAuthenticated) {
                if (loginPopupManager?.open) loginPopupManager.open()
                else alert('請先登入')
                return
            }
            openReply(articleId)
        }

        console.log('✅ setup() 初始化完成')
        return {
            // user state
            currentUser,
            getCurrentUser,

            // PostList
            posts,
            hasMorePosts,
            currentPage,
            stateFunc,
            loadPosts,
            loadMorePosts,
            setupInfiniteScroll,
            cleanupInfiniteScroll,

            // reply
            replyModal,
            openReply,
            closeReply,
            submitReply,
            openReplyWithAuth,   // <<<<<< 這裡一定要有逗號！

            // popup
            popupState,
            popupData,
            isLoading: computed(() => postListLoading.value || uiLoading.value),
            getPopupTitle,
            openPopup,
            closePopup,
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            isAppReady,

            // hooks
            formatDate,
            timeAgo,
            searchQuery,
            manualSearch,

            // menu/page modules
            ...Menu,
            ...Profile,
            ...Home,

            // friends
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus
        }
    }
})
