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
    const posts = ref([])
    const counts = ref(0)
    const currentPage = ref(1)
    const isLoading = ref(false)
    const hasMorePosts = ref(true)
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
        profile.backup = JSON.parse(JSON.stringify(profile))
    }

    const update = async () => {
        try {
            const data = {
                bio: profile.bio,
                displayName: profile.displayName,
                email: profile.email,
                password: profile.password,
                website1: profile.website1,
                website2: profile.website2,
                website3: profile.website3
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
            editMode.value = false
            delete profile.backup; // Clear backup after successful update
            rand.value = new Date().getTime() // Force re-render
        } catch (err) {
            alert("更新失敗")
            console.error("Failed to update profile:", err)
        }
    }
    //#endregion

    //#region File Handling
    const editFileChange = (inputTypeFile) => {
        readURL(inputTypeFile, inputTypeFile.parentElement.previousSibling, document.getElementById("update"))
    }

    const readURL = (inputTypeFile, img, btn) => {
        if (!inputTypeFile.files || inputTypeFile.files.length === 0) return;

        const file = inputTypeFile.files[0]
        const allowTypes = /^image\/.*/
        
        if (allowTypes.test(file.type)) {
            if (btn) btn.disabled = false
            const reader = new FileReader()
            reader.onload = (e) => {
                if (img) {
                    img.src = e.target.result
                    img.title = file.name
                }
            }
            reader.readAsDataURL(file)
        } else {
            alert("不允許的檔案上傳類型！")
            if (btn) btn.disabled = true
            inputTypeFile.value = ""
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
        posts,
        counts,
        isLoading,
        hasMorePosts,
        
        // Methods
        toggleIcon,
        cancel,
        startEdit,
        update,
        editFileChange,
        readURL,
        loadProfile,
        stateFunc,
        loadMorePosts,
        cleanup,
        
        // Manual load functions
        showLoadMoreButton: manualLoadFunctions?.showLoadMoreButton || Vue.ref(false),
        loadMore: manualLoadFunctions?.loadMore || (() => {})
    }
    //#endregion
}

// Export for global usage
window.useProfile = useProfile
