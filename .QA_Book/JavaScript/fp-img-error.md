# 問題 5: 函數式編程風格的圖片錯誤處理 Hook

**症狀**: 需要在多個頁面中處理圖片載入錯誤，避免重複編寫相同的錯誤處理邏輯，希望使用統一的圖片錯誤處理方案。

**原因**: 
- 各個組件都需要處理圖片載入失敗的情況
- 重複的錯誤處理代碼散布在不同檔案中，不易維護
- 需要統一的錯誤狀態管理和處理邏輯

**解決方案**: 
創建 `useImgError.js` hook 提供統一的圖片錯誤處理功能

```javascript
// wwwroot/js/hooks/useImgError.js
export const useImgError = () => {
    const { ref, computed } = Vue
    
    // 存錯誤狀態的地方
    const errorMap = ref(new Map())
    
    // 生成唯一的錯誤 key
    const getErrorKey = (item, type) => {
        const id = item.id || item.articleId || item.fileId || item.userId || 'unknown'
        return `${id}_${type}`
    }
    
    // 處理圖片錯誤
    const handleImageError = (item, type = 'image') => {
        if (!item) return
        
        const key = getErrorKey(item, type)
        errorMap.value.set(key, true)
        
        console.log(`圖片載入失敗: ${key}`)
    }
    
    // 檢查是否有錯誤
    const hasError = (item, type = 'image') => {
        if (!item) return false
        const key = getErrorKey(item, type)
        return errorMap.value.get(key) || false
    }
    
    // 初始化一堆項目的錯誤狀態
    const initErrorStates = (items, types = ['image', 'avatar']) => {
        if (!Array.isArray(items)) return
        
        items.forEach(item => {
            types.forEach(type => {
                const key = getErrorKey(item, type)
                if (!errorMap.value.has(key)) {
                    errorMap.value.set(key, false)
                }
            })
        })
    }
    
    // 幫項目加上錯誤狀態屬性 (比如 imageError, avatarError)
    const addErrorProps = (items, types = ['imageError', 'avatarError']) => {
        if (!Array.isArray(items)) return []
        
        return items.map(item => {
            const result = { ...item }
            
            types.forEach(propName => {
                const type = propName.replace('Error', '') // imageError -> image
                result[propName] = hasError(item, type)
            })
            
            return result
        })
    }
    
    return {
        handleImageError,
        hasError,
        resetError,
        clearAllErrors,
        initErrorStates,
        addErrorProps,
        totalErrors
    }
}
```

**使用方式**:

```javascript
// 在組件中使用
import { useImgError } from '/js/hooks/useImgError.js'

export const useHome = () => {
    // 初始化 hook
    const imgError = useImgError()

    const fetchHotList = async () => {
        const items = Array.isArray(data?.items) ? data.items : []
        
        // 初始化錯誤狀態並加上錯誤屬性
        imgError.initErrorStates(items, ['image', 'avatar'])
        hotlist.value = imgError.addErrorProps(items, ['imageError', 'avatarError'])
    }

    return {
        // 導出錯誤處理功能
        handleImageError: imgError.handleImageError,
        hasImageError: imgError.hasError,
        resetImageError: imgError.resetError
    }
}
```

**Razor 模板使用**:
```html
<!-- 主圖片 -->
<img v-if="item.image && item.image.filePath && !item.imageError" 
     :src="item.image.filePath" 
     @@error="handleImageError(item, 'image')" />

<!-- 頭像 -->
<img v-if="item.authorAvatar && !item.avatarError" 
     :src="item.authorAvatar" 
     @@error="handleImageError(item, 'avatar')" />
```

**技術特點**:
- 使用 Vue 3 Composition API
- Map 資料結構提供高效的錯誤狀態管理
- 支持多種圖片類型 (image, avatar, banner 等)
- 不可變數據處理，避免副作用
- 簡潔的 API 設計，易於使用和維護

**支持的圖片類型**:
- `image` - 一般圖片
- `avatar` - 用戶頭像  
- `banner` - 橫幅圖片
- 自訂類型 - 可根據需求添加

**相關檔案**: 
- `wwwroot/js/hooks/useImgError.js` - Hook 實作
- `wwwroot/js/pages/home/home.js:3,14,26-27,175-177` - 使用範例
- `Views/Home/Index.cshtml:25,42` - 模板使用