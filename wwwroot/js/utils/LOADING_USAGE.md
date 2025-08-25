# 全域載入狀態管理使用說明

## 概述
這個系統提供了一個集中化的載入狀態管理，確保當任何 API 請求正在進行時，`_Layout.cshtml` 中的載入動畫會顯示。

## 核心組件

### 1. LoadingManager (`loadingManager.js`)
- 追蹤所有活躍的 API 請求
- 提供 `isLoading` 響應式狀態
- 自動管理請求的開始和結束

### 2. useApiWithLoading Hook (`useApiWithLoading.js`)
- 便捷的 API 請求包裝器
- 自動集成載入狀態管理

## 使用方式

### 方式 1: 直接使用全域管理器 (推薦)

```javascript
// 在任何 JavaScript 檔案中
const loadingId = window.startLoading('my-api-call')

try {
    const response = await fetch('/api/data')
    const data = await response.json()
    // 處理數據...
} finally {
    window.finishLoading(loadingId)
}
```

### 方式 2: 使用 managedFetch (最簡單)

```javascript
// 將原本的 fetch
const response = await fetch('/api/posts')

// 改為
const response = await window.managedFetch('/api/posts')
// 載入狀態會自動管理
```

### 方式 3: 使用 withLoading 包裝複雜操作

```javascript
const result = await window.withLoading(async () => {
    const response = await fetch('/api/posts')
    const posts = await response.json()
    
    // 可以包含多個 API 調用
    const userResponse = await fetch('/api/user')
    const user = await userResponse.json()
    
    return { posts, user }
}, 'load-posts-and-user')
```

### 方式 4: 在 Vue 組件中使用 Hook

```javascript
import { useApiWithLoading } from '/js/hooks/useApiWithLoading.js'

export const useMyPage = () => {
    const { callApi, withLoading, isLoading } = useApiWithLoading()
    
    const loadData = async () => {
        // 使用 callApi
        const response = await callApi('/api/data')
        const data = await response.json()
        
        // 或使用 withLoading
        const processedData = await withLoading(async () => {
            // 複雜的處理邏輯
            return processData(data)
        })
        
        return processedData
    }
    
    return {
        loadData,
        isLoading // 本地載入狀態（可選）
    }
}
```

## 頁面整合範例

### Dashboard 頁面
```javascript
// dashboard.js
export const useDashboard = () => {
    const loadDashboardData = async () => {
        await window.withLoading(async () => {
            // 並行載入多個 API
            const [stats, users, activities] = await Promise.all([
                window.managedFetch('/api/stats').then(r => r.json()),
                window.managedFetch('/api/users').then(r => r.json()),
                window.managedFetch('/api/activities').then(r => r.json())
            ])
            
            // 處理數據...
        }, 'dashboard-init')
    }
    
    return { loadDashboardData }
}
```

### Profile 頁面
```javascript
// profile.js
export const useProfile = () => {
    const updateProfile = async (profileData) => {
        return await window.withLoading(async () => {
            const response = await fetch('/api/profile', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(profileData)
            })
            
            if (!response.ok) {
                throw new Error('Update failed')
            }
            
            return response.json()
        }, 'update-profile')
    }
    
    return { updateProfile }
}
```

## 現有程式碼遷移

### 1. 簡單的 fetch 調用
```javascript
// 舊的方式
const response = await fetch('/api/posts')

// 新的方式 (一行改動)
const response = await window.managedFetch('/api/posts')
```

### 2. 複雜的載入邏輯
```javascript
// 舊的方式
isLoading.value = true
try {
    const response = await fetch('/api/data')
    const data = await response.json()
    // 處理數據...
} finally {
    isLoading.value = false
}

// 新的方式
const data = await window.withLoading(async () => {
    const response = await fetch('/api/data')
    return response.json()
})
// isLoading 不再需要手動管理
```

## 載入狀態顯示

載入動畫會在以下情況下顯示：
- 任何使用 `managedFetch` 的請求
- 任何使用 `withLoading` 包裝的操作
- 任何手動調用 `startLoading`/`finishLoading` 的操作

## 除錯和監控

```javascript
// 檢查當前載入狀態
console.log('Is loading:', window.globalLoadingManager.isLoading.value)
console.log('Active requests:', window.globalLoadingManager.pendingRequests.value)

// 清除所有載入狀態（錯誤恢復）
window.globalLoadingManager.clearAll()
```

## 最佳實踐

1. **頁面初始化時使用 withLoading**
   ```javascript
   onMounted(async () => {
       await window.withLoading(async () => {
           await loadInitialData()
       }, 'page-init')
   })
   ```

2. **為長時間操作提供有意義的 ID**
   ```javascript
   await window.withLoading(longOperation, 'export-data')
   ```

3. **錯誤處理**
   ```javascript
   try {
       await window.managedFetch('/api/data')
   } catch (error) {
       // 錯誤會自動清除載入狀態
       handleError(error)
   }
   ```

4. **避免載入狀態重疊**
   - 系統會自動處理多個並行請求
   - 只要任何請求在進行中，載入動畫就會顯示

## 注意事項

1. 所有使用此系統的 API 請求都會影響全域載入狀態
2. 確保在錯誤情況下也會正確清除載入狀態
3. 避免在載入狀態中進行可能導致無限載入的操作
4. 如果需要局部載入狀態（不影響全域），使用組件內部的響應式狀態