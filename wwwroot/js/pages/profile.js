const useProfile = () => {
    const { ref, reactive, onMounted } = Vue
    const { timeAgo } = useFormatting()

    //#region Reactive State
    const rand = ref(1)
    const createTime = ref([])
    const content = ref([])
    const editMode = ref(false)
    const isPublic = ref(false)
    const profile = reactive({})
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
            const userId = await getCurrentUserId()
            if (!userId) {
                console.warn("User ID not found, cannot update profile.");
                return;
            }
            
            const data = {
                bio: profile.bio,
                displayName: profile.displayName,
                email: profile.email,
                password: profile.password,
                website1: profile.website1,
                website2: profile.website2,
                website3: profile.website3,
                userId: userId
            }

            const response = await fetch(`/api/Profile/${userId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
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
    
    // Memoized promise to prevent multiple calls
    let userIdPromise = null;
    const getCurrentUserId = () => {
        if (userIdPromise) {
            return userIdPromise;
        }

        userIdPromise = (async () => {
            // 1. Check global state immediately
            if (window.currentUser?.userId) {
                return window.currentUser.userId;
            }

            // 2. Poll a few times for the global state to initialize (more efficient than long polling)
            for (let i = 0; i < 10; i++) {
                await new Promise(res => setTimeout(res, 50));
                if (window.currentUser?.userId) {
                    return window.currentUser.userId;
                }
            }

            // 3. Fallback to API call
            try {
                const response = await fetch('/api/auth/status');
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data = await response.json();
                if (data.success && data.data.authenticated && data.data.user.id) {
                    window.currentUser = window.currentUser || {};
                    window.currentUser.userId = data.data.user.id;
                    return data.data.user.id;
                }
            } catch (error) {
                console.error('無法從 API 獲取用戶 ID:', error);
            }

            // 4. Final fallback
            console.warn('無法獲取當前用戶 ID。');
            return null;
        })();

        return userIdPromise;
    };

    const GetPostsAsync = async (page = 1, authorId = null, append = false) => {
        if (isLoading.value || (!hasMorePosts.value && append)) return null
        
        try {
            isLoading.value = true
            
            const res = await fetch('/api/post', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ page, authorId })
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
            const userId = await getCurrentUserId()
            if (!userId) {
                console.warn("User ID not found, cannot load profile.");
                return;
            }

            const response = await fetch('/api/Profile/get', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: userId })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // 重置分頁狀態
            currentPage.value = 1
            hasMorePosts.value = true
            posts.value = []
            
            // 載入第一頁文章
            await GetPostsAsync(1, userId, false)
            
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
        
        const userId = await getCurrentUserId()
        if (!userId) return
        
        const nextPage = currentPage.value + 1
        await GetPostsAsync(nextPage, userId, true)
    }

    // 無限滾動邏輯
    const setupInfiniteScroll = () => {
        let observer = null
        
        const observeLastPost = () => {
            if (observer) observer.disconnect()
            
            // 找到最後一個文章元素
            const lastPost = document.querySelector('.profile-content li:last-child')
            if (!lastPost) return
            
            observer = new IntersectionObserver((entries) => {
                const lastEntry = entries[0]
                if (lastEntry.isIntersecting && hasMorePosts.value && !isLoading.value) {
                    console.log('偵測到滾動到底部，載入更多文章...')
                    loadMorePosts()
                }
            }, {
                root: null,
                rootMargin: '100px', // 提前100px開始載入
                threshold: 0.1
            })
            
            observer.observe(lastPost)
        }
        
        // 監聽 posts 變化，重新設置觀察者
        Vue.watch(posts, () => {
            Vue.nextTick(() => {
                observeLastPost()
            })
        }, { flush: 'post' })
        
        return () => {
            if (observer) observer.disconnect()
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
    let cleanupInfiniteScroll = null
    
    onMounted(() => {
        loadProfile()
        cleanupInfiniteScroll = setupInfiniteScroll()
    })
    
    // 清理函數（如果需要）
    const cleanup = () => {
        if (cleanupInfiniteScroll) {
            cleanupInfiniteScroll()
        }
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
        cleanup
    }
    //#endregion
}

// Export for global usage
window.useProfile = useProfile
