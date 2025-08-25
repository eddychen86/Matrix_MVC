/**
 * Post Manager - 處理文章列表相關功能
 * 從 main.js 中抽離出來的文章管理功能
 */

import { usePostActions } from '/js/hooks/usePostActions.js'

export const usePostManager = (currentUser) => {
    const { ref } = Vue

    //#region PostList Data (共用於所有使用 PostList ViewComponent 的頁面)

    // PostList 相關的狀態
    const posts = ref([])
    const postListLoading = ref(false)
    const hasMorePosts = ref(true)
    const currentPage = ref(1)
    let infiniteScrollObserver = null

    // 跳轉到對象個人頁面
    const gotoUserPrpfile = id => {

        // window.herf = `/Profile/${}`
    }

    // PostList 相關的方法 - 使用統一的文章操作 hook
    const postActions = usePostActions()
    
    const stateFunc = async (action, articleId, item = null) => {
        return await postActions.stateFunc(action, articleId, item)
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

    // 重置文章列表狀態
    const resetPostState = () => {
        posts.value = []
        currentPage.value = 1
        hasMorePosts.value = true
        postListLoading.value = false
        cleanupInfiniteScroll()
    }

    // 初始化首頁文章列表
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

    // 監聽新貼文事件，實現局部刷新
    const setupPostRefreshListener = () => {
        window.addEventListener('post:listRefresh', (event) => {
            const { action, newArticle } = event.detail;

            if (action === 'prepend' && newArticle) {
                // 將新貼文插入到列表頂部
                posts.value.unshift(newArticle);

                // 可選：添加視覺提示
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