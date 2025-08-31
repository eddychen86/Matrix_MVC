/**
 * Popup Manager - 管理所有彈窗相關的狀態和操作
 * 從 main.js 中抽離出來的彈窗管理功能
 */

export const usePopupManager = (clearSearchCallback = null) => {
    const { ref, reactive, watch } = Vue

    // Popup State
    const popupState = reactive({
        isVisible: false,
        type: '',
        title: ''
    })

    // Popup Data Storage
    const popupData = reactive({
        Search: {
            Users: [],
            Hashtags: []
        },
        Notify: [],
        Follows: [],
        Collects: [],
        Article:null
    })

    const isLoading = ref(false)

    // popup helper
    const getPopupTitle = type => {
        const titles = {
            'Search': '搜尋',
            'Notify': '通知',
            'Follows': '追蹤',
            'Collects': '收藏',
            'Article': '文章'
        }

        return titles[type] || '視窗'
    }


    // 依網址參數決定是否要打開文章彈窗
    const handlePostFromUrl = () => {
        const id = new URL(window.location.href).searchParams.get('post')
        if (id) openArticle(id)
        else if (popupState.type === 'Article') closePopup()
    }

    // 進入頁面檢查一次
    handlePostFromUrl()

    // 使用者按返回/前進時，同步彈窗狀態
    window.addEventListener('popstate', handlePostFromUrl)

    // 打 /api/post/{id} 並顯示到彈窗
    const openArticle = async (articleId) => {
        if (!articleId) return
        popupState.type = 'Article'
        popupState.title = getPopupTitle('Article')
        popupState.isVisible = true
        isLoading.value = true
        try {
            const res = await fetch(`/api/post/${articleId}`, { credentials: 'include' })
            const json = await res.json()
            popupData.Article = json?.article ?? null   // 對應你的 API 回傳 { success, article }
        } catch (e) {
            console.error('讀取文章失敗', e)
            popupData.Article = null
        } finally {
            isLoading.value = false
        }
    }
    // 點收藏卡片 → 進文章
    const goArticle = (articleId) => {
        if (!articleId) return

        // 記住當前彈窗（可能是 'Collects'）
        lastPopupType.value = popupState.type || lastPopupType.value
        rememberPopupScroll()

        // 寫入參數，但不離開頁面
        const url = new URL(window.location.href)
        url.searchParams.set('post', articleId)
        history.pushState({}, '', url)

        openArticle(articleId)
    }

    // 文章內的「返回」按鈕
    const backFromArticle = () => {
        // 1) 先拿掉網址的 post 參數（用 replaceState 不觸發 popstate）
        const url = new URL(window.location.href)
        if (url.searchParams.has('post')) {
            url.searchParams.delete('post')
            history.replaceState({}, '', url)
        }

        // 2) 切回上一個彈窗型態（例如 'Collects'），而不是關閉
        if (lastPopupType.value) {
            popupData.Article = null
            popupState.type = lastPopupType.value
            popupState.title = getPopupTitle(lastPopupType.value)
            popupState.isVisible = true
            restorePopupScroll()
        } else {
            // 沒記錄就保險關掉
            closePopup()
        }
    }
    // === 新增：記錄上一篇彈窗類型（用來返回時還原） ===
    const lastPopupType = Vue.ref(null)
    // （可選）記錄清單捲動位置
    const lastPopupScrollTop = Vue.ref(0)

    const rememberPopupScroll = () => {
        const el = document.querySelector('.popup-data')
        if (el) lastPopupScrollTop.value = el.scrollTop || 0
    }

    const restorePopupScroll = () => {
        Vue.nextTick(() => {
            const el = document.querySelector('.popup-data')
            if (el) el.scrollTop = lastPopupScrollTop.value || 0
        })
    }

    //--------Notify------
    const fetchNotify = async () => {
        isLoading.value = true
        try {
            const res = await fetch('/api/notify', { credentials: 'include' })
            const json = await res.json()
            if (json.success) {
                popupData.Notify = json.data
            }
        } catch (err) {
            console.error('載入通知失敗', err)
            popupData.Notify = []
        } finally {
            isLoading.value = false
        }
    }
    //--------Notify END------

    // 取代你 openFollows 裡的抓資料段，抽成可重用方法
    const fetchFollows = async () => {
        isLoading.value = true
        try {
            const res = await fetch(`/api/follows?page=1&pageSize=10`, { credentials: 'include' })
            const result = await res.json()
            const list = Array.isArray(result?.data) ? result.data : (result?.data?.items ?? [])
            popupData.Follows = list
        } catch (e) {
            console.error('載入 Follows 失敗', e)
            popupData.Follows = []
        } finally {
            isLoading.value = false
        }
    }

    const openFollows = async () => {
        popupState.type = 'Follows'
        popupState.title = getPopupTitle('Follows')
        popupState.isVisible = true
        
        // 顯示彈窗元件
        Vue.nextTick(() => {
            const overlayEl = document.getElementById('popup-overlay')
            if (overlayEl) overlayEl.style.display = ''
        })
        
        // 清空搜尋欄位
        if (typeof clearSearchCallback === 'function') {
            clearSearchCallback()
        }
        
        popupData.Search.Hashtags = []
        await fetchFollows()

        isLoading.value = true

        try {
            const res = await fetch(`/api/follows?page=1&pageSize=10`, {
                method: 'GET',
                credentials: 'include'
            })
            const result = await res.json()

            // 容錯：同時支援 data 是陣列或是物件(items)
            const list = Array.isArray(result?.data)
                ? result.data
                : (result?.data?.items ?? [])

            // 做大小寫兼容 & 預設值
            popupData.Follows = list
        } catch (e) {
            console.error('載入 Follows 失敗', e)
            popupData.Follows = []
        } finally {
            isLoading.value = false
        }
    }

    // Update popup data
    const updatePopupData = (type, data) => {
        if (popupData[type] !== undefined) popupData[type] = data
    }

    // Popup click
    const openPopup = async type => {
        // 每次打開 popup 都清空搜尋欄位
        if (type) lastPopupType.value = type
        if (typeof clearSearchCallback === 'function') {
            clearSearchCallback()
        }

        if (type === 'Follows') {
            return openFollows()  // 👈 直接走新流程
        }

        popupState.type = type
        popupState.title = getPopupTitle(type)
        popupState.isVisible = true

        // 顯示彈窗元件
        Vue.nextTick(() => {
            const overlayEl = document.getElementById('popup-overlay')
            if (overlayEl) overlayEl.style.display = ''
        })

        // console.log('🧠 開啟 popup：', popupState.type)
        if (type === 'Notify') {
            popupData.Notify = []
            await fetchNotify()
            return
        }

        if (type === 'Search') {
            // searchQuery 將由 search-service 管理
            popupData.Search.Users = []
            popupData.Search.Hashtags = []
            return
        }

        isLoading.value = true

        try {
            const res = await fetch('/api/' + type.toLowerCase(), {
                method: 'GET',
                credentials: 'include'
            })
            const data = await res.json()

            updatePopupData(type, data)
        } catch (err) {
            console.log('Fetch Error:', err)
        } finally {
            isLoading.value = false
        }
    }

    const closePopup = () => {
        // 隱藏彈窗元件
        const overlayEl = document.getElementById('popup-overlay')
        if (overlayEl) overlayEl.style.display = 'none'
        
        popupState.isVisible = false
        popupState.type = ''
    }

    // Global Methods
    const toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

    // 公開方法給外部使用
    return {
        // 狀態
        popupState,
        popupData,
        isLoading,
        
        // 方法
        getPopupTitle,
        fetchNotify,
        fetchFollows,
        openFollows,
        updatePopupData,
        openPopup,
        closePopup,
        toggleFunc,

        openArticle,
        goArticle,
        backFromArticle,

        // 向後兼容的別名
        isOpen: Vue.computed(() => popupState.isVisible),
        closeCollectModal: closePopup
    }
}

// 為了向後兼容，也導出單獨的函數
export const createPopupManager = usePopupManager
