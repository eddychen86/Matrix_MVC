// 這個小工具在幫你照看圖片，壞掉就記起來，畫面就不會亂掉
export const useImgError = () => {
    const { ref, computed } = Vue
    
    // 這裡像一本小筆記本，記下哪張圖壞了
    // TODO: 只記必要資訊，保持筆記本輕巧
    const errorMap = ref(new Map())
    
    // 幫每個項目做一個不重複的「身分證」
    const getErrorKey = (item, type) => {
        const id = item.id || item.articleId || item.fileId || item.userId || 'unknown'
        return `${id}_${type}`
    }
    
    // 如果圖片載不出來，就在筆記本打勾「壞了」
    // TODO: 發生錯誤就別再重試，省流量
    const handleImageError = (item, type = 'image') => {
        if (!item) return
        
        const key = getErrorKey(item, type)
        errorMap.value.set(key, true)
        
        // console.log(`圖片載入失敗: ${key}`)
    }
    
    // 問問筆記本：這張圖有壞掉嗎？
    const hasError = (item, type = 'image') => {
        if (!item) return false
        const key = getErrorKey(item, type)
        return errorMap.value.get(key) || false
    }
    
    // 把「壞掉」標記擦掉（如果之後又成功載入）
    const resetError = (item, type = 'image') => {
        if (!item) return
        const key = getErrorKey(item, type)
        errorMap.value.set(key, false)
    }
    
    // 一鍵清空整本筆記（全部重新來過）
    // TODO: 小心用，這會把所有狀態都清掉
    const clearAllErrors = () => {
        errorMap.value.clear()
    }
    
    // 幫一整批的項目先準備好錯誤狀態（預設都沒壞）
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

    // 幫你偷偷檢查圖片在不在，不在就幫你藏起來
    const testImgExist = async (items, types = ['image', 'avatar']) => {
        if (!Array.isArray(items)) return []
        
        let errors = []
        
        // 把要檢查的工作排成一列，大家一起去做
        // TODO: 批次執行，避免一個一個慢慢等
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

        // 等大家都檢查完再說
        await Promise.all(checkPromises)

        // 把哪些文章哪些類型壞了整理成表
        const errorMap = new Map()
        errors.forEach(error => {
            if (!errorMap.has(error.articleId)) {
                errorMap.set(error.articleId, new Set())
            }
            errorMap.get(error.articleId).add(error.type)
        })
        
        // 最後處理：真的壞掉的才藏起來，別亂刪
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
    
    // 幫每個項目加上小旗子（imageError / avatarError），好讓畫面知道要不要顯示
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
    
    // 有多少「壞掉」的數量（拿來觀察用）
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
