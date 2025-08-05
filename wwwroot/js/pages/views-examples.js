/**
 * Views 頁面 Vue 適配器使用範例
 * 展示各種頁面類型的使用方式
 */

//#region Auth 頁面範例 (Login.cshtml, Register.cshtml)

// 1. 零配置 - 自動提供登入/註冊功能
document.addEventListener('DOMContentLoaded', () => {
    createPageApp() // 自動檢測頁面類型並提供相應功能
})

// 2. 自訂登入頁面
createPageApp({
    data() {
        return {
            // 覆蓋預設表單結構
            form: {
                email: '',
                password: '',
                rememberMe: false
            }
        }
    },
    
    methods: {
        // 自訂驗證邏輯
        validateForm() {
            this.clearErrors()
            
            if (!this.form.email) {
                this.setError('email', '請輸入電子郵件')
                return false
            }
            
            if (!this.form.password || this.form.password.length < 6) {
                this.setError('password', '密碼至少需要6個字符')
                return false
            }
            
            return true
        },
        
        // 覆蓋預設提交邏輯
        async submitForm() {
            if (!this.validateForm()) return
            
            // 自訂提交邏輯
            this.setLoading(true)
            try {
                // 自訂 API 調用
                const response = await fetch('/auth/custom-login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(this.form)
                })
                
                if (response.ok) {
                    this.setSuccess(true)
                    window.location.href = '/dashboard'
                }
            } finally {
                this.setLoading(false)
            }
        }
    }
})

//#endregion

//#region Profile 頁面範例 (Profile/Index.cshtml)

// 1. 基本使用 - 自動載入個人資料
createPageApp()

// 2. 增強的個人資料頁面
createPageApp({
    // Vue3 Composition API - 處理複雜狀態
    setup() {
        const { ref, reactive, computed } = Vue
        
        // 頭像上傳狀態
        const avatarUpload = reactive({
            uploading: false,
            progress: 0,
            preview: null
        })
        
        // 表單驗證狀態
        const validation = ref({
            username: true,
            email: true,
            bio: true
        })
        
        const isFormValid = computed(() => 
            Object.values(validation.value).every(v => v)
        )
        
        return {
            avatarUpload,
            validation,
            isFormValid
        }
    },
    
    // Vue2 Options API - 基本操作
    data() {
        return {
            editForm: {
                username: '',
                email: '',
                bio: '',
                avatar: null
            }
        }
    },
    
    methods: {
        // 頭像上傳
        async uploadAvatar(file) {
            this.avatarUpload.uploading = true
            this.avatarUpload.progress = 0
            
            const formData = new FormData()
            formData.append('avatar', file)
            
            try {
                const response = await fetch('/api/profile/avatar', {
                    method: 'POST',
                    body: formData
                })
                
                if (response.ok) {
                    const result = await response.json()
                    this.user.avatar = result.avatarUrl
                    this.setSuccess(true)
                }
            } finally {
                this.avatarUpload.uploading = false
            }
        },
        
        // 儲存個人資料
        async saveProfile() {
            if (!this.isFormValid) return
            
            this.setLoading(true)
            try {
                const response = await fetch('/api/profile/update', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(this.editForm)
                })
                
                if (response.ok) {
                    this.user = { ...this.user, ...this.editForm }
                    this.isEditing = false
                    this.setSuccess(true)
                }
            } finally {
                this.setLoading(false)
            }
        }
    }
})

//#endregion

//#region Follow 頁面範例 (Follow/Follow.cshtml)

// 1. 基本關注頁面
createPageApp()

// 2. 增強的社交功能
createPageApp({
    setup() {
        const { ref, reactive } = Vue
        
        // 即時搜尋
        const searchState = reactive({
            query: '',
            results: [],
            searching: false
        })
        
        return { searchState }
    },
    
    data() {
        return {
            activeTab: 'followers', // followers 或 following
            sortBy: 'recent'
        }
    },
    
    methods: {
        // 即時搜尋用戶
        async searchUsers() {
            if (!this.searchState.query.trim()) {
                this.searchState.results = []
                return
            }
            
            this.searchState.searching = true
            try {
                const response = await fetch(`/api/users/search?q=${this.searchState.query}`)
                const result = await response.json()
                this.searchState.results = result.users
            } finally {
                this.searchState.searching = false
            }
        },
        
        // 批量關注
        async followMultiple(userIds) {
            this.setLoading(true)
            try {
                const response = await fetch('/api/follow/batch', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ userIds })
                })
                
                if (response.ok) {
                    await this.loadFollowData()
                    this.setSuccess(true)
                }
            } finally {
                this.setLoading(false)
            }
        }
    },
    
    computed: {
        sortedFollowers() {
            return this.followers.sort((a, b) => {
                if (this.sortBy === 'name') return a.name.localeCompare(b.name)
                return new Date(b.followedAt) - new Date(a.followedAt)
            })
        }
    }
})

//#endregion

//#region Notify 頁面範例 (Notify/Notify.cshtml)

// 1. 基本通知頁面
createPageApp()

// 2. 即時通知系統
createPageApp({
    setup() {
        const { ref, reactive, onMounted, onUnmounted } = Vue
        
        // WebSocket 連接
        let ws = null
        const realTimeNotifications = ref([])
        
        onMounted(() => {
            // 建立 WebSocket 連接
            ws = new WebSocket('/ws/notifications')
            ws.onmessage = (event) => {
                const notification = JSON.parse(event.data)
                realTimeNotifications.value.unshift(notification)
                this.unreadCount++
            }
        })
        
        onUnmounted(() => {
            if (ws) ws.close()
        })
        
        return { realTimeNotifications }
    },
    
    data() {
        return {
            filter: 'all', // all, read, unread
            autoMarkRead: true
        }
    },
    
    methods: {
        // 標記全部為已讀
        async markAllAsRead() {
            this.setLoading(true)
            try {
                await fetch('/api/notifications/mark-all-read', { method: 'POST' })
                this.notifications.forEach(n => n.isRead = true)
                this.unreadCount = 0
            } finally {
                this.setLoading(false)
            }
        },
        
        // 刪除通知
        async deleteNotification(notificationId) {
            await fetch(`/api/notifications/${notificationId}`, { method: 'DELETE' })
            this.notifications = this.notifications.filter(n => n.id !== notificationId)
        }
    },
    
    computed: {
        filteredNotifications() {
            if (this.filter === 'read') return this.notifications.filter(n => n.isRead)
            if (this.filter === 'unread') return this.notifications.filter(n => !n.isRead)
            return this.notifications
        }
    }
})

//#endregion

//#region Home 頁面範例 (Home/Index.cshtml)

// 1. 簡單首頁
createPageApp()

// 2. 社交媒體風格首頁
createPageApp({
    setup() {
        const { ref, reactive, computed } = Vue
        
        // 無限捲動狀態
        const scrollState = reactive({
            loading: false,
            hasMore: true,
            page: 1
        })
        
        // 新貼文發布
        const newPost = ref({
            content: '',
            images: [],
            publishing: false
        })
        
        return { scrollState, newPost }
    },
    
    data() {
        return {
            feed: 'following', // following, discover, trending
            showNewPostForm: false
        }
    },
    
    methods: {
        // 發布新貼文
        async publishPost() {
            if (!this.newPost.content.trim()) return
            
            this.newPost.publishing = true
            try {
                const response = await fetch('/api/posts', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        content: this.newPost.content,
                        images: this.newPost.images
                    })
                })
                
                if (response.ok) {
                    const result = await response.json()
                    this.posts.unshift(result.post)
                    this.newPost.content = ''
                    this.newPost.images = []
                    this.showNewPostForm = false
                }
            } finally {
                this.newPost.publishing = false
            }
        },
        
        // 無限捲動載入
        async loadMorePosts() {
            if (this.scrollState.loading || !this.scrollState.hasMore) return
            
            this.scrollState.loading = true
            try {
                const response = await fetch(`/api/posts?page=${this.scrollState.page + 1}&feed=${this.feed}`)
                const result = await response.json()
                
                if (result.posts.length > 0) {
                    this.posts.push(...result.posts)
                    this.scrollState.page++
                } else {
                    this.scrollState.hasMore = false
                }
            } finally {
                this.scrollState.loading = false
            }
        },
        
        // 按讚功能
        async toggleLike(postId) {
            const post = this.posts.find(p => p.id === postId)
            if (!post) return
            
            const response = await fetch(`/api/posts/${postId}/like`, { method: 'POST' })
            if (response.ok) {
                post.isLiked = !post.isLiked
                post.likesCount += post.isLiked ? 1 : -1
            }
        }
    },
    
    mounted() {
        // 設定無限捲動
        window.addEventListener('scroll', () => {
            const { scrollTop, scrollHeight, clientHeight } = document.documentElement
            if (scrollTop + clientHeight >= scrollHeight - 1000) {
                this.loadMorePosts()
            }
        })
    }
})

//#endregion

//#region 整合到 Layout 的建議

/**
 * 在 Views/Shared/_Layout.cshtml 中全域載入：
 * 
 * <script src="~/js/pages/vue-universal.js"></script>
 * <script>
 *     document.addEventListener('DOMContentLoaded', () => {
 *         // 全域初始化，自動檢測頁面類型
 *         createPageApp()
 *     })
 * </script>
 * 
 * 或在個別頁面中自訂：
 * 
 * @section Scripts {
 *     <script>
 *         createPageApp({
 *             // 頁面特定配置
 *         })
 *     </script>
 * }
 */

//#endregion