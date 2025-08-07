const useProfile = () => {
    const { ref, reactive, onMounted } = Vue
    const { timeAgo } = useFormatting()

    //#region Reactive State
    const rand = ref(1)
    const createTime = ref([])
    const content = ref([])
    const editMode = ref(false)
    const isPublic = ref(false)
    const profile = reactive({
        bannerPath: '',
        avatarPath: '',
        displayName: '',
        bio: '',
        email: '',
        password: '',
        website1: '',
        website2: '',
        website3: '',
        isPrivate: false
    })
    const updatProfile = reactive({
        bannerPath: '',
        avatarPath: '',
        displayName: '',
        bio: '',
        email: '',
        password: '',
        website1: '',
        website2: '',
        website3: '',
        isPrivate: false
    })
    const posts = ref([])
    const counts = ref(0)
    const currentPage = ref(1)
    const isLoading = ref(false)
    const hasMorePosts = ref(true)
    
    // 密碼驗證相關狀態
    const passwordValidation = reactive({
        isValid: true,
        message: '',
        isValidating: false
    })
    
    // 密碼顯示狀態
    const showPassword = ref(false)
    //#endregion

    //#region Profile Management
    const toggleIcon = () => {
        isPublic.value = !isPublic.value
    }

    const cancel = () => {
        editMode.value = false
        if (profile.backup) {
            // Restore from backup
            Object.assign(profile, profile.backup);
            delete profile.backup
        }
    }

    const startEdit = () => {
        editMode.value = true
        // Create a deep copy for backup
        Object.assign(updatProfile, JSON.parse(JSON.stringify(profile)))
    }

    const update = async () => {
        try {
            // 檢查密碼是否有效（如果有輸入密碼的話）
            if (updatProfile.password && updatProfile.password.trim() !== '') {
                const isPasswordValid = await validatePassword(updatProfile.password)
                if (!isPasswordValid) {
                    alert('密碼不符合規則，請檢查後重新提交')
                    return
                }
            }

            // Server端會自動從認證中獲取UserId，不需要傳遞
            const data = {
                bio: updatProfile.bio,
                displayName: updatProfile.displayName,
                email: updatProfile.email,
                // 只有當密碼不為空時才傳送密碼更新
                ...(updatProfile.password && updatProfile.password.trim() !== '' && { password: updatProfile.password }),
                isPrivate: updatProfile.isPrivate ? 1 : 0,
                website1: updatProfile.website1,
                website2: updatProfile.website2,
                website3: updatProfile.website3,
            }

            const response = await fetch('/api/Profile/personal', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            alert(result.message || "Profile updated successfully!");
            
            // 更新 profile 資料
            Object.assign(profile, updatProfile);
            
            editMode.value = false
            rand.value = new Date().getTime() // Force re-render
        } catch (err) {
            alert("更新失敗")
            console.error("Failed to update profile:", err)
        }
    }

    // 密碼驗證函數
    const validatePassword = async (password) => {
        if (!password || password.trim() === '') {
            passwordValidation.isValid = true
            passwordValidation.message = ''
            passwordValidation.isValidating = false
            return true
        }

        passwordValidation.isValidating = true

        try {
            const response = await fetch('/api/Profile/validate-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ password: password })
            })

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const result = await response.json()
            passwordValidation.isValid = result.isValid
            passwordValidation.message = result.message || ''
            
            return result.isValid
        } catch (error) {
            console.error('密碼驗證失敗:', error)
            passwordValidation.isValid = false
            passwordValidation.message = '驗證過程中發生錯誤，請稍後再試'
            return false
        } finally {
            passwordValidation.isValidating = false
        }
    }

    // 防抖動密碼驗證
    let passwordValidationTimeout = null
    const validatePasswordWithDebounce = (password) => {
        if (passwordValidationTimeout) {
            clearTimeout(passwordValidationTimeout)
        }
        
        // 如果密碼為空，立即重設驗證狀態
        if (!password || password.trim() === '') {
            passwordValidation.isValid = true
            passwordValidation.message = ''
            passwordValidation.isValidating = false
            return
        }
        
        passwordValidationTimeout = setTimeout(() => {
            validatePassword(password)
        }, 500) // 500ms 後執行驗證
    }

    // 切換密碼顯示/隱藏
    const togglePasswordVisibility = () => {
        showPassword.value = !showPassword.value
    }
    //#endregion

    //#region File Handling

    const updateFile = type => {
        // console.log('updateFile called with type:', type)
        
        // 根據類型選擇對應的 input 元素
        let inputId
        if (type === 'avatar') {
            inputId = 'avatar-file-input'
        } else if (type === 'banner') {
            inputId = 'banner-file-input'
        } else {
            console.error('Invalid file type:', type)
            return
        }
        
        // 找到對應的 input 元素並觸發點擊
        const input = document.getElementById(inputId)
        if (input) {
            input.addEventListener('change', (event) => handleFileUpload(event, type), { once: true })
            input.click()
        } else {
            console.error('File input not found:', inputId)
        }
    }

    const handleFileUpload = async (event, type) => {
        const file = event.target.files[0]
        if (!file) return

        // 檢查是否為圖片
        if (!file.type.startsWith('image/')) {
            alert('請選擇圖片檔案')
            event.target.value = ''
            return
        }

        // 檢查檔案大小 (限制 5MB)
        const maxSize = 5 * 1024 * 1024 // 5MB
        if (file.size > maxSize) {
            alert('檔案大小不可超過 5MB')
            event.target.value = ''
            return
        }

        try {
            // 建立 FormData
            const formData = new FormData()
            formData.append('file', file)
            formData.append('type', type)

            // 發送上傳請求
            const response = await fetch('/api/Profile/upload-image', {
                method: 'POST',
                credentials: 'include',
                body: formData
            })

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const result = await response.json()
            
            if (result.success) {
                // 更新本地 profile 資料
                if (type === 'avatar') {
                    profile.avatarPath = result.data.filePath
                } else if (type === 'banner') {
                    profile.bannerPath = result.data.filePath
                }
                
                // 強制更新顯示
                rand.value = new Date().getTime()
                
                console.log(`${type} uploaded successfully:`, result.data.filePath)
            } else {
                throw new Error(result.message || '上傳失敗')
            }
        } catch (error) {
            console.error('File upload failed:', error)
            alert('上傳失敗，請稍後再試')
        } finally {
            // 清空 input 值，允許重複上傳同一檔案
            event.target.value = ''
        }
    }

    //#endregion

    //#region User & Profile Loading
    
    const GetPostsAsync = async (page = 1, append = false, pageSize = 10) => {
        // console.log(`載入第 ${page} 頁文章，每頁 ${pageSize} 篇`)

        if (isLoading.value || (!hasMorePosts.value && append)) return null
        
        try {
            isLoading.value = true
            
            const res = await fetch('/api/post', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ page, pageSize })
            })

            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`)
            }

            let data = await res.json()
            const formattedArticles = data.articles.map(m => ({
                ...m, 
                createTime: timeAgo(m.createTime),
                authorName: m.author.displayName,
                authorAvator: m.author.avatarPath,
            }))
            
            if (append) {
                // 追加新文章到現有列表
                posts.value = [...posts.value, ...formattedArticles]
            } else {
                // 替換整個列表（初次載入）
                posts.value = formattedArticles
            }
            
            counts.value = data.totalCount
            
            // 檢查是否還有更多文章
            const totalLoaded = posts.value.length
            hasMorePosts.value = totalLoaded < data.totalCount
            
            if (append) {
                currentPage.value = page
            }
            
            return data
        } catch (err) {
            console.error('載入文章失敗:', err)
            return null
        } finally {
            isLoading.value = false
        }
    }

    const loadProfile = async () => {
        try {
            const response = await fetch('/api/Profile', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // 重置分頁狀態
            currentPage.value = 1
            hasMorePosts.value = true
            posts.value = []
            
            // 載入個人資料
            const data = await response.json()
            Object.assign(profile, data)
            isPublic.value = !data.isPrivate
        } catch (err) {
            console.error('載入 Profile 失敗:', err)
        }
    }

    // 載入更多文章
    const loadMorePosts = async () => {
        if (!hasMorePosts.value || isLoading.value) return
        
        const nextPage = currentPage.value + 1
        await GetPostsAsync(nextPage, true)
    }

    // 手動載入更多按鈕邏輯
    const setupManualLoad = () => {
        // 提供手動載入更多文章的按鈕功能
        const showLoadMoreButton = Vue.computed(() => {
            return hasMorePosts.value && !isLoading.value && posts.value.length > 0
        })
        
        return {
            showLoadMoreButton,
            loadMore: loadMorePosts
        }
    }
    //#endregion

    //#region Article Interaction
    const togglePraise = async (aid) => {
        try {
            const response = await fetch(`/api/post/${aid}/toggle-praise`, { method: 'POST' });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            console.log('Praise toggled:', data);
        } catch (error) {
            console.error('Error toggling praise:', error);
        }
    };

    const toggleCollect = async (aid) => {
        try {
            const response = await fetch(`/api/post/${aid}/toggle-collect`, { method: 'POST' });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            console.log('Collect toggled:', data);
        } catch (error) {
            console.error('Error toggling collect:', error);
        }
    };

    const toggleComment = async (aid) => {
        // TODO: Implement comment functionality, e.g., opening a comment modal
        console.log('Comment button clicked for article:', aid);
    };
    
    const share = (aid) => {
        // TODO: Implement share functionality
        console.log('Share button clicked for article:', aid);
    }

    const stateFunc = (type, aid) => {
        const actions = {
            praise: togglePraise,
            collect: toggleCollect,
            comment: toggleComment,
            share: share
        };

        if (actions[type]) {
            actions[type](aid);
        }
    }
    //#endregion

    //#region Lifecycle
    let manualLoadFunctions = null
    
    onMounted(async () => {
        loadProfile()
        // 載入第一頁文章（限制 10 篇）
        await GetPostsAsync(1, false, 10)
        manualLoadFunctions = setupManualLoad()
    })
    
    // 清理函數（如果需要）
    const cleanup = () => {
        // 清理邏輯（如果需要）
    }
    //#endregion

    //#region Exposed API
    return {
        // Reactive State
        rand,
        createTime,
        content,
        editMode,
        isPublic,
        profile,
        updatProfile,
        posts,
        counts,
        isLoading,
        hasMorePosts,
        passwordValidation,
        showPassword,
        
        // Methods
        toggleIcon,
        cancel,
        startEdit,
        update,
        updateFile,
        loadProfile,
        stateFunc,
        loadMorePosts,
        cleanup,
        validatePassword,
        validatePasswordWithDebounce,
        togglePasswordVisibility,
        
        // Manual load functions
        showLoadMoreButton: manualLoadFunctions?.showLoadMoreButton || Vue.ref(false),
        loadMore: manualLoadFunctions?.loadMore || (() => {})
    }
    //#endregion
}

// Export for global usage
window.useProfile = useProfile
