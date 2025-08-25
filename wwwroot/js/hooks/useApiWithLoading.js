/**
 * API 請求 Hook - 自動管理載入狀態
 * 
 * 使用方式：
 * const { callApi, isLoading } = useApiWithLoading()
 * 
 * // 方式 1: 使用 callApi
 * const result = await callApi('/api/posts', { method: 'GET' })
 * 
 * // 方式 2: 使用 withLoading
 * const result = await withLoading(async () => {
 *     const response = await fetch('/api/posts')
 *     return response.json()
 * })
 */

export const useApiWithLoading = () => {
    // 如果全域載入管理器已經載入，使用它；否則創建本地狀態
    if (window.globalLoadingManager) {
        return {
            callApi: (url, options = {}, requestId = null) => {
                return window.globalLoadingManager.fetch(url, options, requestId)
            },
            withLoading: (operation, requestId = null) => {
                return window.globalLoadingManager.withLoading(operation, requestId)
            },
            isLoading: window.globalLoadingManager.isLoading,
            pendingRequests: window.globalLoadingManager.pendingRequests
        }
    } else {
        // 如果全域管理器尚未載入，創建本地載入狀態
        const isLoading = Vue.ref(false)
        const pendingRequests = Vue.ref(0)
        const activeRequests = new Set()

        const updateLoadingState = () => {
            pendingRequests.value = activeRequests.size
            isLoading.value = activeRequests.size > 0
        }

        const callApi = async (url, options = {}, requestId = null) => {
            const id = requestId || `local_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
            activeRequests.add(id)
            updateLoadingState()

            try {
                const response = await fetch(url, options)
                return response
            } finally {
                activeRequests.delete(id)
                updateLoadingState()
            }
        }

        const withLoading = async (operation, requestId = null) => {
            const id = requestId || `local_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
            activeRequests.add(id)
            updateLoadingState()

            try {
                const result = await operation()
                return result
            } finally {
                activeRequests.delete(id)
                updateLoadingState()
            }
        }

        return {
            callApi,
            withLoading,
            isLoading,
            pendingRequests
        }
    }
}

/**
 * 為現有的 fetch 調用快速添加載入狀態管理
 * 
 * 使用方式：
 * // 將原本的 fetch
 * const response = await fetch('/api/data')
 * 
 * // 改為
 * const response = await managedFetch('/api/data')
 */
export const managedFetch = (url, options = {}, requestId = null) => {
    if (window.globalLoadingManager) {
        return window.globalLoadingManager.fetch(url, options, requestId)
    } else {
        // 回退到普通 fetch
        console.warn('Global loading manager not available, using regular fetch')
        return fetch(url, options)
    }
}

/**
 * 為現有的異步操作快速添加載入狀態管理
 * 
 * 使用方式：
 * const result = await withGlobalLoading(async () => {
 *     const response = await fetch('/api/data')
 *     const data = await response.json()
 *     // 其他處理邏輯
 *     return data
 * })
 */
export const withGlobalLoading = (operation, requestId = null) => {
    if (window.globalLoadingManager) {
        return window.globalLoadingManager.withLoading(operation, requestId)
    } else {
        // 回退到直接執行
        console.warn('Global loading manager not available, executing operation directly')
        return operation()
    }
}