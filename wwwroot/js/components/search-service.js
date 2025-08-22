/**
 * Search Service - 處理所有搜尋相關功能
 * 從 main.js 中抽離出來的搜尋功能
 */

export const useSearchService = (initialPopupData, initialPopupState) => {
    const { ref, watch, nextTick } = Vue

    const searchQuery = ref('')
    const isLoading = ref(false)

    // personId -> { followers, following } 的快取
    const statsCache = Object.create(null)
    
    // 允許延遲設定 popupData 和 popupState
    let popupData = initialPopupData
    let popupState = initialPopupState

    // 延遲設定 popupData 和 popupState
    const setPopupData = (newPopupData, newPopupState) => {
        popupData = newPopupData
        popupState = newPopupState
    }

    // ------- 新增：點 hashtag 相關 -------
    let onTagClickHandler = null
    const setTagClickHandler = (fn) => { onTagClickHandler = fn }

    // 點 hashtag：關閉搜尋彈窗、清空清單、載入第 1 頁，再綁無限滾動
    const goTag = async (tag) => {
        if (!tag) return
        
        // 關閉搜尋彈窗
        if (popupState) {
            popupState.isVisible = false
            popupState.type = ''
        }

        // 重置文章清單
        posts.value = []
        currentPage.value = 1
        hasMorePosts.value = true

        // 直接呼叫 Search 區的端點（不動 Post 區）
        const res = await fetch(`/api/search/tags/${encodeURIComponent(tag)}/posts?page=1&pageSize=10`, {
            credentials: 'include'
        })
        const json = await res.json()
        const list = Array.isArray(json?.articles) ? json.articles : []

        // 用你現有的格式化器（若沒載入也能 fallback）
        const { postListService } = await import('/js/components/PostListService.js')
        const firstPage = postListService?.formatArticles
            ? postListService.formatArticles(list)
            : list

        posts.value = firstPage
        hasMorePosts.value = firstPage.length === 10

        // 設定以 tag 為條件的無限滾動
        nextTick(() => setupInfiniteScrollForTag(tag))
    }

    const setupInfiniteScrollForTag = (tag) => {
        // 清掉舊的 observer
        cleanupInfiniteScroll()

        const trigger = document.querySelector('.infinite-scroll-trigger')
        if (!trigger) return

        infiniteScrollObserver = new IntersectionObserver(async entries => {
            for (const e of entries) {
                if (!e.isIntersecting || !hasMorePosts.value || postListLoading.value) continue
                postListLoading.value = true
                try {
                    const next = currentPage.value + 1
                    const res = await fetch(`/api/search/tags/${encodeURIComponent(tag)}/posts?page=${next}&pageSize=10`, {
                        credentials: 'include'
                    })
                    const json = await res.json()
                    const list = Array.isArray(json?.articles) ? json.articles : []

                    const { postListService } = await import('/js/components/PostListService.js')
                    const more = postListService?.formatArticles
                        ? postListService.formatArticles(list)
                        : list

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

        infiniteScrollObserver.observe(trigger)
    }

    // ------- 新增：點 hashtag 相關 END -------

    // 通用搜尋功能
    const manualSearch = async () => {
        if (!popupData) {
            console.warn('popupData not initialized yet')
            return
        }
        // console.log('🔍 手動搜尋按鈕觸發！', searchQuery)

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
                bio: user.bio || '這位使用者尚未填寫個人簡介。',
                isFollowed: user.isFollowed,     // ✅ 已有
                personId: user.personId          // ✅ 需要這個來傳給 API
            }))

            popupData.Search.Hashtags = tags.data
            // console.log('🎯 搜尋結果資料：', popupData.Search)
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
        if (!popupData) {
            console.warn('popupData not initialized yet')
            return
        }

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
        if (!popupState) {
            console.warn('popupState not initialized yet')
            return
        }

        if (popupState.type === 'Follows') {
            return manualFollowSearch()
        }
        return manualSearch()
    }

    // 搜尋查詢變化監聽
    const setupSearchWatcher = (fetchFollowsCallback) => {
        watch(searchQuery, (val) => {
            if (!popupState || !popupData) return

            const kw = (val || '').trim()
            // 只有在 Follows 視窗時才做自動刷新
            if (popupState.type === 'Follows' && kw === '') {
                // 清掉搜尋結果
                popupData.Search.Users = []
                // 重新抓追蹤清單
                if (typeof fetchFollowsCallback === 'function') {
                    fetchFollowsCallback()
                }
            }
        })
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

    // 清空搜尋結果
    const clearSearch = () => {
        searchQuery.value = ''
        if (popupData) {
            popupData.Search.Users = []
            popupData.Search.Hashtags = []
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

    // === 追蹤統計（滑過使用） ===
    const fetchUserStats = async (personId) => {
        if (!personId) return { followers: 0, following: 0 }
        if (statsCache[personId]) return statsCache[personId]

        const res = await fetch(`/api/search/stats/${personId}`, { credentials: 'include' })
        const json = await res.json()
        const stats = (json && json.data) ? json.data : { followers: 0, following: 0 }  // ✅ 不用 ?.

        statsCache[personId] = stats
        return stats
    }

    // 滑過使用者列時呼叫：設定 hover、載入統計（含快取）
    const onHoverUser = async (user) => {
        user._hover = true
        if (user.stats || user._loadingStats || !user.personId) return
        user._loadingStats = true
        try {
            user.stats = await fetchUserStats(user.personId)
        } catch (e) {
            console.warn('load stats failed', e)
        } finally {
            user._loadingStats = false
        }
    }

    return {
        // 狀態
        searchQuery,
        isLoading,

        // 方法
        openFollows,
        manualSearch,
        onHoverUser,
        manualFollowSearch,
        onSearchClick,
        setupSearchWatcher,
        clearSearch,
        setPopupData,
        goTag,
        setTagClickHandler
    }
}

// 單獨導出創建函數
export const createSearchService = useSearchService