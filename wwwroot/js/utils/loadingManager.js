// 全域載入狀態管理器
// 用來追蹤所有 API 請求是否在忙

class LoadingManager {
    constructor() {
        this.activeRequests = new Set()
        this.isLoading = Vue.ref(false)
        this.pendingRequests = Vue.ref(0)
    }

    // 開始一個新的載入狀態（回傳這次的 id）
    startLoading(requestId = null) {
        const id = requestId || `request_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
        this.activeRequests.add(id)
        this.updateLoadingState()
        return id
    }

    // 結束一個載入狀態
    finishLoading(requestId) {
        this.activeRequests.delete(requestId)
        this.updateLoadingState()
    }

    // 更新整體載入狀態
    updateLoadingState() {
        this.pendingRequests.value = this.activeRequests.size
        this.isLoading.value = this.activeRequests.size > 0
    }

    // 把所有載入清掉（切頁或出錯時）
    clearAll() {
        this.activeRequests.clear()
        this.updateLoadingState()
    }

    // 幫 fetch 外面套一層，進入就顯示載入、完成就關掉
    async fetch(url, options = {}, requestId = null) {
        const id = this.startLoading(requestId)
        
        try {
            const response = await fetch(url, options)
            return response
        } finally {
            this.finishLoading(id)
        }
    }

    // 幫任何非同步工作加上載入管理
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
