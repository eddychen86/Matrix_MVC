// Vue2 個人資料頁面功能 - 初學者友善版本
// 使用簡單易懂的寫法，適合 Vue2 初學者學習
const useProfileVue2 = () => {
    // 從全域取得時間格式化功能
    const { timeAgo } = useFormatting()
    
    return {
        //#region Vue2 響應式數據 - data() 寫法
        data() {
            return {
                // 基本狀態
                editMode: false,        // 是否處於編輯模式
                isPublic: false,        // 個人資料是否公開
                isLoading: false,       // 是否正在載入數據
                
                // 個人資料數據
                profile: {
                    displayName: '',    // 顯示名稱
                    bio: '',           // 個人簡介
                    email: '',         // 電子郵件
                    avatarPath: '',    // 頭像路徑
                    bannerPath: '',    // 橫幅圖片路徑
                    website1: '',      // 網站連結 1
                    website2: '',      // 網站連結 2
                    website3: '',      // 網站連結 3
                    backup: null       // 編輯時的備份數據
                },
                
                // 文章相關數據
                posts: [],             // 文章列表
                currentPage: 1,        // 當前頁碼
                hasMorePosts: true,    // 是否還有更多文章
                
                // 內部使用的變數
                userIdCache: null      // 快取用戶ID
            }
        },
        //#endregion

        //#region Vue2 計算屬性 - computed 寫法
        computed: {
            // 顯示用的名稱（如果沒有設定則顯示預設值）
            displayName() {
                return this.profile.displayName || '未設定名稱'
            },
            
            // 是否有文章
            hasPosts() {
                return this.posts.length > 0
            },
            
            // 載入狀態的文字說明
            loadingText() {
                if (this.isLoading) {
                    return '載入中...'
                }
                if (!this.hasPosts) {
                    return '還沒有發布任何文章'
                }
                if (!this.hasMorePosts) {
                    return '已顯示所有文章'
                }
                return ''
            }
        },
        //#endregion

        //#region Vue2 方法 - methods 寫法
        methods: {
            //#region 工具函數 - 處理響應式值
            // 取得響應式值（處理 ref 和 reactive）
            getValue(obj) {
                return obj && typeof obj === 'object' && 'value' in obj ? obj.value : obj
            },
            
            // 設定響應式值
            setValue(obj, value) {
                if (obj && typeof obj === 'object' && 'value' in obj) {
                    obj.value = value
                } else {
                    return value
                }
            },
            //#endregion
            
            //#region 基本功能
            // 切換隱私狀態
            togglePrivacy() {
                this.isPublic = !this.isPublic
                console.log('隱私狀態:', this.isPublic ? '公開' : '私人')
            },

            // 開始編輯個人資料
            startEdit() {
                console.log('開始編輯個人資料')
                this.editMode = true
                // 備份當前數據，以便取消時還原
                this.profile.backup = JSON.parse(JSON.stringify(this.profile))
            },

            // 取消編輯（與模板中的 cancel 對應）
            cancel() {
                console.log('取消編輯')
                this.editMode = false
                
                // 如果有備份，就還原數據
                if (this.profile.backup) {
                    Object.assign(this.profile, this.profile.backup)
                    delete this.profile.backup
                }
            },
            //#endregion

            //#region 數據載入功能
            // 取得當前用戶的ID
            async getUserId() {
                console.log('開始取得用戶ID')
                
                // 如果已經有快取，就直接回傳
                const cachedValue = this.getValue(this.userIdCache)
                if (cachedValue) {
                    console.log('使用快取的用戶ID:', cachedValue)
                    return cachedValue
                }

                // 1. 先檢查全域用戶狀態
                console.log('檢查全域用戶狀態:', window.currentUser)
                if (window.currentUser && window.currentUser.userId) {
                    this.setValue(this.userIdCache, window.currentUser.userId)
                    console.log('從全域狀態取得用戶ID:', window.currentUser.userId)
                    return window.currentUser.userId
                }

                // 2. 如果沒有，就呼叫 API 取得
                console.log('呼叫 /api/auth/status')
                try {
                    const response = await fetch('/api/auth/status')
                    console.log('auth/status 回應狀態:', response.status)
                    
                    if (response.ok) {
                        const data = await response.json()
                        console.log('auth/status 回應數據:', data)
                        
                        if (data.success && data.data.user && data.data.user.id) {
                            const userId = data.data.user.id
                            this.setValue(this.userIdCache, userId)
                            console.log('從 API 取得用戶ID:', userId)
                            return userId
                        }
                    }
                } catch (error) {
                    console.error('取得用戶ID失敗:', error)
                }

                console.warn('無法取得用戶ID')
                return null
            },

            // 載入個人資料
            async loadProfile() {
                console.log('開始載入個人資料')
                
                const userId = await this.getUserId()
                if (!userId) {
                    console.warn('找不到用戶ID')
                    return
                }

                try {
                    console.log('發送 API 請求，用戶ID:', userId)
                    
                    // 發送請求取得個人資料
                    const response = await fetch('/api/Profile/get', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ id: userId })
                    })

                    console.log('API 回應狀態:', response.status)

                    if (response.ok) {
                        const profileData = await response.json()
                        console.log('收到個人資料數據:', profileData)
                        
                        // 更新個人資料數據（處理 reactive 對象）
                        if (this.profile && typeof this.profile === 'object' && 'value' in this.profile) {
                            // 如果 profile 是 ref 對象
                            Object.assign(this.profile.value, profileData)
                        } else {
                            // 如果 profile 是 reactive 對象或一般對象
                            Object.assign(this.profile, profileData)
                        }
                        
                        // 設定公開狀態（處理 reactive 值）
                        if (this.isPublic && typeof this.isPublic === 'object' && 'value' in this.isPublic) {
                            this.isPublic.value = !profileData.isPrivate
                        } else {
                            this.isPublic = !profileData.isPrivate
                        }
                        
                        // 載入文章
                        await this.loadPosts(userId)
                        
                        console.log('個人資料載入完成')
                    } else {
                        const errorText = await response.text()
                        console.error('API 請求失敗:', response.status, errorText)
                    }
                } catch (error) {
                    console.error('載入個人資料失敗:', error)
                }
            },

            // 載入文章列表
            async loadPosts(userId, page = 1, append = false) {
                if (this.isLoading) {
                    return // 如果正在載入，就不重複載入
                }

                this.isLoading = true
                console.log(`載入第 ${page} 頁文章`)

                try {
                    const response = await fetch('/api/post', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ page: page, authorId: userId })
                    })

                    if (response.ok) {
                        const data = await response.json()
                        
                        // 格式化文章數據（使用全域的 timeAgo 函數）
                        const formattedPosts = data.articles.map(article => ({
                            ...article,
                            createTime: timeAgo(article.createTime), // 使用 useFormatting 的 timeAgo
                            authorName: article.author.displayName,
                            authorAvator: article.author.avatarPath
                        }))

                        // 更新文章列表
                        if (append) {
                            // 追加到現有列表
                            this.posts = this.posts.concat(formattedPosts)
                        } else {
                            // 替換整個列表
                            this.posts = formattedPosts
                        }

                        // 更新分頁狀態
                        this.currentPage = page
                        this.hasMorePosts = this.posts.length < data.totalCount
                        
                        console.log(`載入了 ${formattedPosts.length} 篇文章`)
                    }
                } catch (error) {
                    console.error('載入文章失敗:', error)
                } finally {
                    this.isLoading = false
                }
            },

            // 載入更多文章
            async loadMorePosts() {
                if (!this.hasMorePosts || this.isLoading) {
                    return
                }

                const userId = await this.getUserId()
                if (userId) {
                    const nextPage = this.currentPage + 1
                    await this.loadPosts(userId, nextPage, true)
                }
            },
            //#endregion

            //#region 更新功能
            // 儲存個人資料變更（與模板中的 update 對應）
            async update() {
                console.log('開始儲存個人資料')
                
                const userId = await this.getUserId()
                if (!userId) {
                    alert('無法識別用戶身份，請重新登入')
                    return
                }

                try {
                    // 準備要送出的數據
                    const updateData = {
                        bio: this.profile.bio,
                        displayName: this.profile.displayName,
                        email: this.profile.email,
                        website1: this.profile.website1,
                        website2: this.profile.website2,
                        website3: this.profile.website3,
                        userId: userId
                    }

                    // 發送更新請求
                    const response = await fetch(`/api/Profile/${userId}`, {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(updateData)
                    })

                    if (response.ok) {
                        const result = await response.json()
                        alert(result.message || '個人資料更新成功！')
                        
                        // 關閉編輯模式並清除備份
                        this.editMode = false
                        delete this.profile.backup
                        
                        console.log('個人資料更新成功')
                    } else {
                        throw new Error('更新請求失敗')
                    }
                } catch (error) {
                    alert('更新失敗，請稍後再試')
                    console.error('更新個人資料失敗:', error)
                }
            },
            //#endregion

            //#region 文章互動功能
            // 按讚功能
            async togglePraise(articleId) {
                console.log('切換讚賞狀態，文章ID:', articleId)
                
                try {
                    const response = await fetch(`/api/post/${articleId}/toggle-praise`, {
                        method: 'POST'
                    })

                    if (response.ok) {
                        const result = await response.json()
                        console.log('讚賞狀態切換成功')
                        
                        // 更新本地數據
                        const post = this.posts.find(p => p.articleId === articleId)
                        if (post && result.praised !== undefined) {
                            if (result.praised) {
                                post.praiseCount = (post.praiseCount || 0) + 1
                            } else {
                                post.praiseCount = Math.max((post.praiseCount || 0) - 1, 0)
                            }
                        }
                    }
                } catch (error) {
                    console.error('切換讚賞狀態失敗:', error)
                    alert('操作失敗，請稍後再試')
                }
            },

            // 收藏功能
            async toggleCollect(articleId) {
                console.log('切換收藏狀態，文章ID:', articleId)
                
                try {
                    const response = await fetch(`/api/post/${articleId}/toggle-collect`, {
                        method: 'POST'
                    })

                    if (response.ok) {
                        const result = await response.json()
                        console.log('收藏狀態切換成功')
                        
                        // 更新本地數據
                        const post = this.posts.find(p => p.articleId === articleId)
                        if (post && result.collected !== undefined) {
                            if (result.collected) {
                                post.collectCount = (post.collectCount || 0) + 1
                            } else {
                                post.collectCount = Math.max((post.collectCount || 0) - 1, 0)
                            }
                        }
                    }
                } catch (error) {
                    console.error('切換收藏狀態失敗:', error)
                    alert('操作失敗，請稍後再試')
                }
            },

            // 留言功能（預留）
            showComments(articleId) {
                console.log('顯示留言，文章ID:', articleId)
                alert('留言功能即將推出')
            },

            // 分享功能（預留）
            sharePost(articleId) {
                console.log('分享文章，文章ID:', articleId)
                alert('分享功能即將推出')
            },

            // 統一的文章操作處理（與模板中的 stateFunc 對應）
            stateFunc(action, articleId) {
                console.log(`執行操作: ${action}，文章ID: ${articleId}`)
                
                if (action === 'praise') {
                    this.togglePraise(articleId)
                } else if (action === 'collect') {
                    this.toggleCollect(articleId)
                } else if (action === 'comment') {
                    this.showComments(articleId)
                } else if (action === 'share') {
                    this.sharePost(articleId)
                } else {
                    console.warn('未知的操作類型:', action)
                }
            },
            //#endregion

            //#region 檔案處理（簡化版本）
            // 處理圖片上傳預覽
            handleImagePreview(event) {
                const file = event.target.files[0]
                
                if (!file) {
                    return
                }

                // 檢查是否為圖片
                if (!file.type.startsWith('image/')) {
                    alert('請選擇圖片檔案')
                    event.target.value = ''
                    return
                }

                // 建立檔案讀取器
                const reader = new FileReader()
                
                reader.onload = function() {
                    // 這裡可以設定預覽圖片
                    console.log('圖片預覽已載入')
                    // 實際使用時，可以設定到對應的 img 元素
                }
                
                reader.readAsDataURL(file)
            }
            //#endregion
        },
        //#endregion

        //#region Vue2 生命週期
        // 元件掛載完成後執行
        async mounted() {
            console.log('個人資料頁面已載入')
            
            // 載入個人資料和文章
            await this.loadProfile()
            
            // 設定無限滾動（簡化版本）
            if (this.setupInfiniteScroll) {
                this.setupInfiniteScroll()
            }
            
            console.log('初始化完成')
        },

        // 元件銷毀前清理
        beforeDestroy() {
            console.log('清理個人資料頁面')
            this.userIdCache = null
        },
        //#endregion

        //#region 輔助功能
        // 設定無限滾動（簡化版本）
        setupInfiniteScroll() {
            // 簡單的滾動檢測
            window.addEventListener('scroll', () => {
                // 檢查是否滾動到接近底部
                const scrollPosition = window.innerHeight + window.scrollY
                const documentHeight = document.documentElement.offsetHeight
                
                // 如果滾動到距離底部 100px 以內，就載入更多
                if (scrollPosition >= documentHeight - 100) {
                    this.loadMorePosts()
                }
            })
        }
        //#endregion
    }
}

// 導出給全域使用
window.useProfileVue2 = useProfileVue2

// 初學者學習重點：
// 1. data() 回傳一個物件，包含所有響應式數據
// 2. computed 是計算屬性，會根據依賴的數據自動更新
// 3. methods 包含所有的函數方法
// 4. mounted() 是生命週期鉤子，在元件掛載後執行
// 5. async/await 用於處理非同步操作（如 API 呼叫）
// 6. 使用 fetch() 來呼叫後端 API