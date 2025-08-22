/**
 * Search Service - 處理所有搜尋相關功能
 * 從 main.js 中抽離出來的搜尋功能
 */

export const useSearchService = (initialPopupData, initialPopupState) => {
    const { ref, watch } = Vue

    const searchQuery = ref('')
    const isLoading = ref(false)
    
    // 允許延遲設定 popupData 和 popupState
    let popupData = initialPopupData
    let popupState = initialPopupState

    // 延遲設定 popupData 和 popupState
    const setPopupData = (newPopupData, newPopupState) => {
        popupData = newPopupData
        popupState = newPopupState
    }

    // 通用搜尋功能
    const manualSearch = async () => {
        if (!popupData) {
            console.warn('popupData not initialized yet')
            return
        }
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
        if (!popupData) {
            console.warn('popupData not initialized yet')
            return
        }

        const keyword = (searchQuery.value || '').trim()
        // 空字串就清空
        if (!keyword) {
            popupData.Search.Users = []
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

    // 清空搜尋結果
    const clearSearch = () => {
        searchQuery.value = ''
        if (popupData) {
            popupData.Search.Users = []
            popupData.Search.Hashtags = []
        }
    }

    return {
        // 狀態
        searchQuery,
        isLoading,

        // 方法
        manualSearch,
        manualFollowSearch,
        onSearchClick,
        setupSearchWatcher,
        clearSearch,
        setPopupData
    }
}

// 單獨導出創建函數
export const createSearchService = useSearchService