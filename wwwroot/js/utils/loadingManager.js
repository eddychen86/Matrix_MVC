/**
 * 全域載入狀態管理器
 * 用於追蹤所有 API 請求的載入狀態
 */

class LoadingManager {
    constructor() {
        this.activeRequests = new Set()
        this.isLoading = Vue.ref(false)
        this.pendingRequests = Vue.ref(0)
    }

    /**
     * 開始一個新的載入狀態
     * @param {string} requestId - 唯一的請求標識符
     */
    startLoading(requestId = null) {
        const id = requestId || `request_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
        this.activeRequests.add(id)
        this.updateLoadingState()
        return id
    }

    /**
     * 結束一個載入狀態
     * @param {string} requestId - 請求標識符
     */
    finishLoading(requestId) {
        this.activeRequests.delete(requestId)
        this.updateLoadingState()
    }

    /**
     * 更新整體載入狀態
     */
    updateLoadingState() {
        this.pendingRequests.value = this.activeRequests.size
        this.isLoading.value = this.activeRequests.size > 0
    }

    /**
     * 清除所有載入狀態（用於頁面切換或錯誤處理）
     */
    clearAll() {
        this.activeRequests.clear()
        this.updateLoadingState()
    }

    /**
     * 包裝 fetch 請求以自動管理載入狀態
     * @param {string} url - 請求 URL
     * @param {object} options - fetch 選項
     * @param {string} requestId - 可選的請求 ID
     * @returns {Promise} fetch promise
     */
    async fetch(url, options = {}, requestId = null) {
        const id = this.startLoading(requestId)
        
        try {
            const response = await fetch(url, options)
            return response
        } finally {
            this.finishLoading(id)
        }
    }

    /**
     * 包裝任意異步操作以自動管理載入狀態
     * @param {Function} asyncOperation - 異步操作函數
     * @param {string} requestId - 可選的請求 ID
     * @returns {Promise} 包裝後的 promise
     */
    async withLoading(asyncOperation, requestId = null) {
        const id = this.startLoading(requestId)
        
        try {
            const result = await asyncOperation()
            return result
        } finally {
            this.finishLoading(id)
        }
    }
}

// 創建全域單例實例
const globalLoadingManager = new LoadingManager()

// 為了向後相容，也可以直接使用函數形式
export const useGlobalLoading = () => {
    return {
        isLoading: globalLoadingManager.isLoading,
        pendingRequests: globalLoadingManager.pendingRequests,
        startLoading: (id) => globalLoadingManager.startLoading(id),
        finishLoading: (id) => globalLoadingManager.finishLoading(id),
        fetch: (url, options, id) => globalLoadingManager.fetch(url, options, id),
        withLoading: (operation, id) => globalLoadingManager.withLoading(operation, id),
        clearAll: () => globalLoadingManager.clearAll()
    }
}

// 將單例暴露到全域
window.globalLoadingManager = globalLoadingManager

export default globalLoadingManager