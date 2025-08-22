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
        Collects: []
    })

    const isLoading = ref(false)

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
            console.error('❌ 載入通知失敗', err)
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

    // Update popup data
    const updatePopupData = (type, data) => {
        if (popupData[type] !== undefined) popupData[type] = data
    }

    // Popup click
    const openPopup = async type => {
        // 每次打開 popup 都清空搜尋欄位
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
        
        // 向後兼容的別名
        isOpen: Vue.computed(() => popupState.isVisible),
        closeCollectModal: closePopup
    }
}

// 為了向後兼容，也導出單獨的函數
export const createPopupManager = usePopupManager