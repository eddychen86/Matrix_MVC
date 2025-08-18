import { useFormatting } from '/js/hooks/useFormatting.js'
import { useMenu } from '/js/components/menu.js'
import { useHome } from '/js/pages/home/home.js'
import { useProfile } from '/js/pages/profile/profile.js'
import { useAbout } from '/js/pages/about/about.js'
import { useReply } from '/js/components/reply.js'
import loginPopupManager from '/js/auth/login-popup.js'

// 導入新的模組化組件
import { useUserManager } from '/js/components/user-manager.js'
import { usePopupManager } from '/js/components/popup-manager.js'
import { useSearchService } from '/js/components/search-service.js'
import { useFriendsManager } from '/js/components/friends-manager.js'
import { usePostManager } from '/js/components/post-manager.js'

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
        const { ref, computed, onMounted, watch } = Vue
        const { formatDate, timeAgo } = useFormatting()

        //#region 模組化管理器初始化

        // 初始化用戶管理器
        const userManager = useUserManager()
        const { currentUser, getCurrentUser } = userManager

        // 初始化彈窗管理器
        const popupManager = usePopupManager()
        const { 
            popupState, 
            popupData, 
            openPopup, 
            closePopup, 
            fetchFollows,
            isLoading: popupLoading 
        } = popupManager

        // 初始化搜尋服務
        const searchService = useSearchService(popupData, popupState)
        const { 
            searchQuery, 
            onSearchClick, 
            manualFollowSearch,
            isLoading: searchLoading
        } = searchService

        // 設置搜尋監聽器
        searchService.setupSearchWatcher(fetchFollows)

        // 初始化好友管理器
        const friendsManager = useFriendsManager(currentUser)
        const {
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus,
            toggleFollow
        } = friendsManager

        // 初始化文章管理器
        const postManager = usePostManager(currentUser)
        const {
            posts,
            hasMorePosts,
            currentPage,
            stateFunc,
            loadPosts,
            setupInfiniteScroll,
            cleanupInfiniteScroll,
            initializeHomePosts,
            postListLoading
        } = postManager

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
        const isAboutPage = /^\/about(?:\/|$)/.test(currentPath)

        // 組件/頁面模組
        const Menu = (typeof useMenu === 'function') ? useMenu() : {}
        const Home = LoadingPage(/^\/(?:home(?:\/|$))?$|^\/$/i, useHome)
        const Profile = LoadingPage(/^\/profile(?:\/|$)/i, useProfile)
        const Reply = LoadingPage(/^\/reply(?:\/|$)/i, useReply)
        const About = LoadingPage(/^\/about(?:\/|$)/i, useAbout)

        //#endregion

        // 包裝 toggleFollow 以提供必要的參數
        const handleToggleFollow = (targetPersonId, currentStatus) => {
            return toggleFollow(targetPersonId, currentStatus, popupData, fetchFollows)
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

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
            // 將 currentUser 設為全局可訪問
            window.currentUser = currentUser
            
            await getCurrentUser()

            // 如果是首頁，初始化文章列表
            if (isHomePage) {
                await initializeHomePosts()
            }

            // 若頁面包含好友列表區塊，載入好友
            if (document.querySelector('.friends-list')) {
                loadFriends(1, 20, null, friendsStatus.value)
            }
        })

        //#endregion

        console.log('✅ setup() 成功初始化，searchQuery =', searchQuery.value)

        // 合併所有loading狀態
        const isLoading = computed(() => 
            postListLoading.value || popupLoading.value || searchLoading.value
        )

        return {
            // 用戶相關
            currentUser,
            getCurrentUser,

            // 文章列表相關
            posts,
            hasMorePosts,
            currentPage,
            stateFunc,
            loadPosts,
            setupInfiniteScroll,
            cleanupInfiniteScroll,

            // 彈窗相關
            popupState,
            popupData,
            isLoading,
            getPopupTitle: popupManager.getPopupTitle,
            openPopup,
            closePopup,
            fetchFollows,
            
            // 向後相容
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,
            isAppReady,

            // 搜尋相關
            searchQuery,
            onSearchClick,
            manualFollowSearch,

            // 好友相關
            toggleFollow: handleToggleFollow,
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus,

            // hooks
            formatDate,
            timeAgo,

            // 頁面組件
            ...Menu,
            ...Profile,
            ...Home,
            ...About,
            ...Reply
        }
    }
})