// 圖片錯誤處理的 hook
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
        
        // console.log(`圖片載入失敗: ${key}`)
    }
    
    // 檢查是否有錯誤
    const hasError = (item, type = 'image') => {
        if (!item) return false
        const key = getErrorKey(item, type)
        return errorMap.value.get(key) || false
    }
    
    // 重置錯誤
    const resetError = (item, type = 'image') => {
        if (!item) return
        const key = getErrorKey(item, type)
        errorMap.value.set(key, false)
    }
    
    // 清除所有錯誤
    const clearAllErrors = () => {
        errorMap.value.clear()
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

    const testImgExist = async (items, types = ['image', 'avatar']) => {
        if (!Array.isArray(items)) return []
        
        let errors = []
        
        // 建立所有檢查的 Promise 陣列
        const checkPromises = []
        items.forEach(item => {
            types.forEach(type => {
                const file = item?.[type]
                const filePath = file && file?.filePath || ''
                if (filePath && !filePath.match('http')) {
                    const checkPromise = fetch(filePath)
                        .then(response => {
                            if (!response.ok) {
                                errors.push({ articleId: item.articleId, type: type })
                            }
                        })
                        .catch(() => {
                            errors.push({ articleId: item.articleId, type: type })
                        })
                    checkPromises.push(checkPromise)
                }
            })
        })

        // 等待所有檢查完成
        await Promise.all(checkPromises)

        // 建立錯誤映射 Map<articleId, Set<errorTypes>>
        const errorMap = new Map()
        errors.forEach(error => {
            if (!errorMap.has(error.articleId)) {
                errorMap.set(error.articleId, new Set())
            }
            errorMap.get(error.articleId).add(error.type)
        })
        
        // 處理結果：只清理確實失敗的圖片類型
        const result = items.map(item => {
            const errorTypes = errorMap.get(item.articleId)
            if (errorTypes) {
                const updatedItem = { ...item }
                if (errorTypes.has('image')) {
                    updatedItem.image = null
                }
                if (errorTypes.has('avatar')) {
                    updatedItem.authorAvatar = ''  // 頭像失敗設為空字串
                }
                return updatedItem
            }
            return item
        })

        return result
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
    
    // 總錯誤數量
    const totalErrors = computed(() => {
        let count = 0
        errorMap.value.forEach(hasErr => {
            if (hasErr) count++
        })
        return count
    })
    
    return {
        handleImageError,
        hasError,
        resetError,
        clearAllErrors,
        initErrorStates,
        testImgExist,
        addErrorProps,
        totalErrors,
        errorMap,
    }
}

export default useImgError