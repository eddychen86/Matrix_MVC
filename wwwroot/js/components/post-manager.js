// 文章小管家：幫你拿文章、排排隊、看到底還有沒有更多
// 這份是從 main.js 分出來的，讓事情更清楚

import { usePostActions } from '/js/hooks/usePostActions.js'

export const usePostManager = (currentUser) => {
    const { ref } = Vue

    //#region PostList Data (共用於所有使用 PostList ViewComponent 的頁面)

    // 這些是文章清單的狀態（像是進度條、頁碼）
    // TODO: 請保持命名清楚，方便看懂
    const posts = ref([])
    const postListLoading = ref(false)
    const hasMorePosts = ref(true)
    const currentPage = ref(1)
    let infiniteScrollObserver = null

    // 去看看某位使用者的個人頁
    const gotoUserPrpfile = authorName => {
        window.herf = `/Profile/${authorName}`
    }

    // PostList 相關的方法 - 使用統一的文章操作 hook
    const postActions = usePostActions()
    
    const stateFunc = async (action, articleId, item = null) => {
        return await postActions.stateFunc(action, articleId, item)
    }

    // 幫我去後端拿文章回來（通用版）
    // TODO: 如果失敗，記得優雅處理不要卡住畫面
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

    // 往下滑就幫你多抓一頁（無限滾動）
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

    // 設定無限滾動（看到觸發點就再抓）
    // TODO: 頁面可能沒有觸發點，要能安靜跳過
    const setupInfiniteScroll = (uid = null, isProfilePage = false) => {
        // 清理之前的 Observer
        if (infiniteScrollObserver) {
            infiniteScrollObserver.disconnect()
        }

        // 找到觸發元素（PostList ViewComponent 會放一個）
        const triggerElement = document.querySelector('.infinite-scroll-trigger')
        if (!triggerElement) {
            // 某些頁面（如無文章列表）不會有觸發元素，屬正常情況
            console.debug && console.debug('Infinite scroll trigger element not found')
            return
        }

        // console.log('Setting up infinite scroll...', { uid, isProfilePage })

        // 使用 IntersectionObserver 盯著觸發點
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

    // 把觀察器收起來（換頁或離開時）
    const cleanupInfiniteScroll = () => {
        if (infiniteScrollObserver) {
            infiniteScrollObserver.disconnect()
            infiniteScrollObserver = null
        }

        // 移除觸發元素
        const triggers = document.querySelectorAll('.infinite-scroll-trigger')
        triggers.forEach(trigger => trigger.remove())
    }

    // 一鍵重置狀態（像是重開機）
    const resetPostState = () => {
        posts.value = []
        currentPage.value = 1
        hasMorePosts.value = true
        postListLoading.value = false
        cleanupInfiniteScroll()
    }

    // 進入首頁時先抓第一頁，然後把無限滾動也準備好
    const initializeHomePosts = async () => {
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
        return result
    }

    // 有新貼文進來時，幫你塞到最上面（不重整頁面）
    const setupPostRefreshListener = () => {
        window.addEventListener('post:listRefresh', (event) => {
            const { action, newArticle } = event.detail;

            if (action === 'prepend' && newArticle) {
                // 將新貼文插入到列表頂部
                posts.value.unshift(newArticle);

                // 可選：閃一下當提示「新貼文來了！」
                Vue.nextTick(() => {
                    const firstPost = document.querySelector('.post-item:first-child, .article-item:first-child');
                    if (firstPost) {
                        firstPost.classList.add('new-post-highlight');
                        // 3秒後移除高亮
                        setTimeout(() => {
                            firstPost.classList.remove('new-post-highlight');
                        }, 3000);
                    }
                });

                console.log('新貼文已添加到列表頂部');
            }
        });
    }

    //#endregion

    return {
        // 狀態
        posts,
        postListLoading,
        hasMorePosts,
        currentPage,

        // 方法
        gotoUserPrpfile,
        stateFunc,
        loadPosts,
        loadMorePosts,
        setupInfiniteScroll,
        cleanupInfiniteScroll,
        resetPostState,
        initializeHomePosts,
        setupPostRefreshListener
    }
}

// 單獨導出創建函數
export const createPostManager = usePostManager
