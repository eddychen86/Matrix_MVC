import { useFormatting } from '/js/hooks/useFormatting.js'
import { useMenu } from '/js/components/menu.js'
import { useHome } from '/js/pages/home/home.js'
import { useProfile } from '/js/pages/profile/profile.js'
import { useAbout } from '/js/pages/about/about.js'
import { useReply } from '/js/components/reply.js'
import { useCreatePost } from '/js/components/create-post.js'
import { createCKEditor } from '/js/components/ckeditor5.js'
import loginPopupManager from '/js/auth/login-popup.js'
import { useGlobalLoading } from '/js/utils/loadingManager.js'

// 新朋友們：模組化小工具們都在這
import { useUserManager } from '/js/components/user-manager.js'
import { usePopupManager } from '/js/components/popup-manager.js'
import { useSearchService } from '/js/components/search-service.js'
import { useFriendsManager } from '/js/components/friends-manager.js'
import { usePostManager } from '/js/components/post-manager.js'
import { useChat } from '/js/components/chat.js'
import { useImgError } from '/js/hooks/useImgError.js'

const globalApp = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue not ready, retrying...')
        return
    } else {
        lucide.createIcons()

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                const app = Vue.createApp(content)
                // 這裡把一些無傷大雅的警告忽略掉，不要吵
                app.config.warnHandler = (msg) => {
                    // 忽略常見的無害警告
                    const ignoredWarnings = [
                        'Tags with side effect',
                        'are ignored in client component templates',
                        'CKEditor',
                        'ClassicEditor'
                    ]
                    
                    if (ignoredWarnings.some(warning => msg.includes(warning))) {
                        return // 忽略這些警告
                    }
                    console.warn(msg)
                }
                window.globalApp = app.use(window.CKEditor?.default || window.CKEditor).mount('#app')
                // 標記 Vue 應用程式已載入
                document.getElementById('app').setAttribute('data-v-app', 'true')
            })
        } else {
            // DOM 已經載入完成
            const app = Vue.createApp(content)
            // 同上：別被小雜音影響
            app.config.warnHandler = (msg) => {
                // 忽略常見的無害警告
                const ignoredWarnings = [
                    'Tags with side effect',
                    'are ignored in client component templates',
                    'CKEditor',
                    'ClassicEditor'
                ]
                
                if (ignoredWarnings.some(warning => msg.includes(warning))) {
                    return // 忽略這些警告
                }
                console.warn(msg)
            }
            window.globalApp = app.use(window.CKEditor?.default || window.CKEditor).mount('#app')
        }
    }
}

// 把登入彈窗管家放到 window，HTML 也能叫他
window.loginPopupManager = loginPopupManager

globalApp({
    setup() {
        const { ref, computed, onMounted, watch } = Vue
        const { formatDate, timeAgo } = useFormatting()
        // 全頁共用：圖片錯誤處理（讓 PostList 等部分頁面也可使用）
        const { handleImageError, hasError } = useImgError()

        //#region 模組化管理器初始化（集合啦！）

        // 全域載入管理器：幫忙數還在跑的請求
        // TODO: 所有 fetch 盡量走這裡，方便控管
        const globalLoading = useGlobalLoading()
        const {
            isLoading: globalIsLoading,
            pendingRequests,
            startLoading,
            finishLoading,
            fetch: managedFetch,
            withLoading,
            clearAll: clearAllLoading
        } = globalLoading

        // 初始化用戶管理器
        const userManager = useUserManager()
        const { currentUser, getCurrentUser } = userManager

        // 搜尋服務：關鍵字、追蹤、彈窗互動
        const searchService = useSearchService(null, null) // 先初始化搜尋服務
        const {
            goTag,
            onHoverUser,
            manualSearch,
            searchQuery,
            onSearchClick,
            manualFollowSearch,
            clearSearch,
            setPopupData,
            setupSearchWatcher,
            isLoading: searchLoading
        } = searchService

        // 彈窗管理器：開關彈窗、載入內容
        const popupManager = usePopupManager(clearSearch)
        const {
            popupState,
            popupData,
            openPopup,
            closePopup,
            fetchFollows,
            isLoading: popupLoading,
            goArticle,        // 👈 加這個
            openArticle,       // 👈 想要從其他地方直接開文章彈窗時可用（可選）
            backFromArticle,
        } = popupManager

        // 好友管理器：追蹤、名單、狀態
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

        // 文章管理器：列表、無限滾動、互動
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
            setupPostRefreshListener,
            postListLoading,
            gotoUserPrpfile
        } = postManager

        // 聊天管理器：訊息、會話、未讀、連線
        const chatManager = useChat(currentUser)
        const {
            // 聊天狀態
            messages,
            conversations,
            unreadCount,
            currentConversation,
            isConnected,

            // 聊天 Popup 狀態
            isChatPopupOpen,
            newMessage,
            openChatPopup,
            closeChatPopup,
            toggleChatPopup,

            // 聊天方法
            sendMessage,
            loadConversation,
            loadConversations,
            markAsRead,
            markConversationAsRead,
            searchMessages,
            handleSendMessage,
            formatMessageTime,

            // SignalR 連接
            startConnection,
            stopConnection
        } = chatManager

        // 重新設定搜尋服務的 popupData 和 popupState
        setPopupData(popupData, popupState)

        // 幫搜尋裝上監聽器（輸入就查）
        setupSearchWatcher(fetchFollows)

        // 將 openChatPopup 暴露到全局，以便從非 Vue 環境調用
        window.openChatPopupGlobal = openChatPopup

        //#endregion

        const isAppReady = ref(false) // App 準備好了沒？

        // ==== 在 setup() 內，postManager 解構之後加上這段 ====

        // 供我們自己的 tag 無限捲動用的 observer（避免干擾 postManager 內建的）
        let tagObserver = null

        searchService.setTagClickHandler(async (tag) => {
            // 1) 停掉既有的無限滾動（postManager 那條）
            cleanupInfiniteScroll()

            // 2) 清空並載入第 1 頁
            posts.value = []
            currentPage.value = 1
            hasMorePosts.value = true

            const res = await fetch(`/api/search/tags/${encodeURIComponent(tag)}/posts?page=1&pageSize=10`, {
                credentials: 'include'
            })
            const json = await res.json()
            const raw = Array.isArray(json?.articles) ? json.articles : []

            const { postListService } = await import('/js/components/PostListService.js')
            const firstPage = postListService?.formatArticles
                ? postListService.formatArticles(raw)
                : raw

            posts.value = firstPage
            hasMorePosts.value = firstPage.length === 10

            // 3) 為這個標籤重新綁定無限滾動
            if (tagObserver) {
                tagObserver.disconnect()
                tagObserver = null
            }
            Vue.nextTick(() => {
                const trigger = document.querySelector('.infinite-scroll-trigger')
                if (!trigger) return

                tagObserver = new IntersectionObserver(async entries => {
                    for (const e of entries) {
                        if (!e.isIntersecting || !hasMorePosts.value || postListLoading.value) continue
                        postListLoading.value = true
                        try {
                            const next = currentPage.value + 1
                            const res2 = await fetch(`/api/search/tags/${encodeURIComponent(tag)}/posts?page=${next}&pageSize=10`, {
                                credentials: 'include'
                            })
                            const json2 = await res2.json()
                            const rawMore = Array.isArray(json2?.articles) ? json2.articles : []
                            const more = postListService?.formatArticles
                                ? postListService.formatArticles(rawMore)
                                : rawMore

                            if (more.length) {
                                posts.value = [...posts.value, ...more]
                                currentPage.value = next
                                hasMorePosts.value = more.length === 10
                            } else {
                                hasMorePosts.value = false
                            }
                        } finally {
                            postListLoading.value = false
                        }
                    }
                }, { root: null, rootMargin: '200px', threshold: 0.1 })

                tagObserver.observe(trigger)
            })
        })

        onMounted(() => {
            isAppReady.value = true

            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''
        })

        //#region 匯入各頁面的 Vue 模組（ESM）

        // 路徑偵測（供後續邏輯使用）
        const currentPath = window.location.pathname.toLowerCase()
        const isHomePage = /^\/(?:home(?:\/index)?)?$|^\/$/i.test(currentPath)
        const isProfilePage = /^\/profile/i.test(currentPath)
        const isAboutPage = /^\/about/i.test(currentPath)

        // 組件/頁面模組
        const Menu = (typeof useMenu === 'function') ? useMenu() : {}
        const Home = isHomePage && typeof useHome === 'function' ? useHome() : {}
        const Profile = isProfilePage && typeof useProfile === 'function' ? useProfile() : {}
        const About = isAboutPage && typeof useAbout === 'function' ? useAbout() : {}
        const Reply = (typeof useReply === 'function') ? useReply() : {}  // 全域載入，因為 ReplyPopup 在各頁面都會使用

        // 全域載入 CreatePost 組件，因為 CreatePostPopup 在各頁面都會使用
        const CreatePost = (typeof useCreatePost === 'function') ? useCreatePost() : {}
        
        // CKEditor 管理器
        let ckEditorManager = null

        //#endregion

        // 包裝 toggleFollow 以提供必要的參數
        const handleToggleFollow = (targetPersonId, currentStatus) => {
            return toggleFollow(targetPersonId, currentStatus, popupData, fetchFollows)
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

        // 暴露全域載入管理器到 window
        window.startLoading = startLoading
        window.finishLoading = finishLoading
        window.managedFetch = managedFetch
        window.withLoading = withLoading

        //#region Lifecycle

        // 掛載後：抓使用者、初始化頁面
        onMounted(async () => {
            // 將 currentUser 設為全局可訪問
            window.currentUser = currentUser

            await getCurrentUser()

            // 如果在 Home 頁面且有 CreatePost 模組，監聽 showPostModal 狀態
            if (isHomePage && CreatePost.showPostModal) {
                watch(CreatePost.showPostModal, (newValue) => {
                    if (newValue && !ckEditorManager && typeof createCKEditor === 'function') {
                        try {
                            // 當 showPostModal 為 true 且尚未初始化 CKEditor 時，創建實例
                            ckEditorManager = createCKEditor()
                            // 將 CKEditor 管理器暴露到全域
                            window.ckEditorManager = ckEditorManager
                            console.log('CKEditor 管理器已初始化並暴露到全域')
                        } catch (error) {
                            console.error('CKEditor 管理器初始化失敗:', error)
                        }
                    }
                }, { immediate: true })
            }

            // Vue 的 v-if 指令會自動控制彈窗的顯示狀態

            // 如果是首頁，準備文章列表與監聽新貼文
            if (isHomePage) {
                await initializeHomePosts()
                // 設置新貼文事件監聽器
                setupPostRefreshListener()
            }

            // 初始化 Lucide icons，確保 SSR 載入的組件圖標能正常顯示
            if (typeof lucide !== 'undefined' && lucide.createIcons) lucide.createIcons()
        })

        //#endregion

        // 把所有 loading 狀態合起來（讓畫面只看一個）
        const isLoading = computed(() => globalIsLoading.value || postListLoading.value || popupLoading.value || searchLoading.value)


        return {
            // Global state
            isAppReady,
            currentPath,
            isHomePage,
            isProfilePage,
            isAboutPage,

            // 全域載入狀態管理
            globalIsLoading,
            pendingRequests,
            startLoading,
            finishLoading,
            managedFetch,
            withLoading,
            clearAllLoading,

            // 用戶相關
            currentUser,
            getCurrentUser,

            // 搜尋相關
            searchQuery,
            manualSearch,
            onSearchClick,
            onHoverUser,
            clearSearch,
            manualFollowSearch,
            goTag,
            searchLoading,

            // 彈窗管理
            popupState,
            popupData,
            openPopup,
            closePopup,
            fetchFollows,
            popupLoading,
            getPopupTitle: popupManager.getPopupTitle,
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            // 好友管理
            friends,
            friendsLoading,
            friendsTotal,
            friendsStatus,
            loadFriends,
            changeFriendsStatus,
            toggleFollow: handleToggleFollow,

            // 文章管理
            posts,
            hasMorePosts,
            currentPage,
            stateFunc,
            loadPosts,
            setupInfiniteScroll,
            cleanupInfiniteScroll,
            initializeHomePosts,
            setupPostRefreshListener,
            postListLoading,
            gotoUserPrpfile,

            // 聊天管理
            messages,
            conversations,
            unreadCount,
            currentConversation,
            isConnected,

            // 聊天 Popup 控制
            isChatPopupOpen,
            openChatPopup,
            closeChatPopup,
            toggleChatPopup,

            // 聊天方法
            newMessage,
            sendMessage,
            loadConversation,
            loadConversations,
            markAsRead,
            markConversationAsRead,
            searchMessages,
            handleSendMessage,
            formatMessageTime,

            // SignalR 連接
            startConnection,
            stopConnection,

            // 合併 loading 狀態
            isLoading,

            // hooks
            formatDate,
            timeAgo,

            // 讓模板可以用 <div @click="goArticle(id)">
            goArticle,
            openArticle,
            backFromArticle,
            
            // CKEditor 管理器
            ckEditorManager,
            
            // 圖片錯誤處理（全域可用，供各頁面的清單使用）
            handleImageError,
            hasError,
            
            // 頁面組件
            ...Menu,
            ...Profile,
            ...Home,
            ...About,
            ...Reply,
            ...CreatePost
        }
    }
})
