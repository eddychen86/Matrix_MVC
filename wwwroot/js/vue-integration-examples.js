/**
 * Vue 相容適配器整合範例
 * 展示如何與現有的 main.js 和 profile.js 搭配使用
 */

//#region 整合方式說明

/**
 * 現有架構分析：
 * 
 * 1. main.js (Vue3 setup()) - 全域應用掛載到 #app
 *    - 全域用戶狀態管理
 *    - Popup 功能
 *    - Menu 整合
 *    - Profile 頁面功能整合 (透過 useProfile)
 * 
 * 2. profile.js (Vue3 Composition API) - useProfile() 函數
 *    - Profile 數據管理
 *    - 文章載入和無限滾動
 *    - 編輯模式和檔案上傳
 * 
 * 3. 相容適配器 - 增強和擴展機制
 *    - 檢測現有應用
 *    - 無縫增強功能
 *    - 創建獨立應用
 */

//#endregion

//#region 使用範例 1: 自動增強現有應用

// 在 _Layout.cshtml 中添加：
// <script src="~/js/vue-adapter-compatible.js"></script>

// 適配器會自動：
// 1. 檢測現有的 globalApp
// 2. 為 Profile 頁面自動添加增強功能
// 3. 不影響現有的 profile.js 功能

//#endregion

//#region 使用範例 2: 手動增強現有應用

// 在任何頁面的 @section Scripts 中使用：
window.whenAppReady((app) => {
    // 增強現有應用，添加新功能
    enhanceApp({
        state: {
            customFeatures: {
                notifications: [],
                preferences: {
                    theme: 'dark',
                    language: 'zh-TW'
                }
            }
        },
        
        methods: {
            // 新增通知功能
            addNotification(message, type = 'info') {
                this.customFeatures.notifications.push({
                    id: Date.now(),
                    message,
                    type,
                    timestamp: new Date()
                })
            },
            
            // 新增偏好設定
            updatePreference(key, value) {
                this.customFeatures.preferences[key] = value
                localStorage.setItem(`pref_${key}`, value)
            },
            
            // 載入偏好設定
            loadPreferences() {
                const theme = localStorage.getItem('pref_theme')
                const language = localStorage.getItem('pref_language')
                
                if (theme) this.customFeatures.preferences.theme = theme
                if (language) this.customFeatures.preferences.language = language
            }
        }
    })
    
    // 使用新增的功能
    app.loadPreferences()
    app.addNotification('應用已增強！', 'success')
})

//#endregion

//#region 使用範例 3: Profile 頁面專用增強

// 只在 Profile/Index.cshtml 的 @section Scripts 中使用：
window.whenAppReady((app) => {
    // 檢查是否為 Profile 頁面
    if (window.location.pathname.includes('/profile')) {
        enhanceApp({
            state: {
                profileEnhancement: {
                    statisticsVisible: false,
                    advancedSearch: {
                        tags: [],
                        dateRange: null,
                        contentType: 'all'
                    }
                }
            },
            
            methods: {
                // 切換統計資料顯示
                toggleStatistics() {
                    this.profileEnhancement.statisticsVisible = !this.profileEnhancement.statisticsVisible
                },
                
                // 進階搜尋文章
                searchPosts(criteria) {
                    const { tags, dateRange, contentType } = criteria
                    
                    // 過濾現有的 posts（不影響原有的 posts 資料）
                    let filteredPosts = [...this.posts]
                    
                    if (tags.length > 0) {
                        filteredPosts = filteredPosts.filter(post => 
                            tags.some(tag => post.content.includes(tag))
                        )
                    }
                    
                    if (dateRange) {
                        filteredPosts = filteredPosts.filter(post => {
                            const postDate = new Date(post.createTime)
                            return postDate >= dateRange.start && postDate <= dateRange.end
                        })
                    }
                    
                    return filteredPosts
                },
                
                // 匯出 Profile 數據
                async exportProfileData() {
                    const data = {
                        profile: this.profile,
                        posts: this.posts,
                        statistics: {
                            totalPosts: this.counts,
                            totalLikes: this.posts.reduce((sum, post) => sum + post.praiseCount, 0)
                        }
                    }
                    
                    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
                    const url = URL.createObjectURL(blob)
                    
                    const a = document.createElement('a')
                    a.href = url
                    a.download = `profile-${this.profile.displayName}-${new Date().toISOString().split('T')[0]}.json`
                    a.click()
                    
                    URL.revokeObjectURL(url)
                }
            }
        })
        
        console.log('Profile 頁面專用增強已載入')
    }
})

//#endregion

//#region 使用範例 4: 創建獨立應用（不影響 main.js）

// 在非 Profile 頁面創建獨立的 Vue 應用：
createCompatibleApp({
    // 自定義掛載點（不使用 #app）
    mountPoint: '#custom-feature',
    
    // Vue3 setup
    setup() {
        const { ref, reactive, onMounted } = Vue
        
        const chatState = reactive({
            messages: [],
            connected: false,
            typing: false
        })
        
        const newMessage = ref('')
        
        // WebSocket 聊天功能
        let ws = null
        
        const connectChat = () => {
            ws = new WebSocket('/ws/chat')
            
            ws.onopen = () => {
                chatState.connected = true
                console.log('聊天連接已建立')
            }
            
            ws.onmessage = (event) => {
                const message = JSON.parse(event.data)
                chatState.messages.push(message)
            }
            
            ws.onclose = () => {
                chatState.connected = false
                console.log('聊天連接已關閉')
            }
        }
        
        const sendMessage = () => {
            if (newMessage.value.trim() && ws) {
                ws.send(JSON.stringify({
                    type: 'message',
                    content: newMessage.value,
                    timestamp: new Date().toISOString()
                }))
                newMessage.value = ''
            }
        }
        
        onMounted(() => {
            connectChat()
        })
        
        return {
            chatState,
            newMessage,
            sendMessage
        }
    },
    
    // Vue2 data (可選)
    data() {
        return {
            chatSettings: {
                soundEnabled: true,
                showTimestamps: true
            }
        }
    },
    
    methods: {
        toggleSound() {
            this.chatSettings.soundEnabled = !this.chatSettings.soundEnabled
        }
    }
})

//#endregion

//#region 使用範例 5: 條件式增強

// 根據用戶權限或頁面內容動態增強：
window.whenAppReady(async (app) => {
    // 檢查用戶權限
    const isAdmin = app.currentUser?.isAdmin
    const isProfile = window.location.pathname.includes('/profile')
    
    if (isAdmin && isProfile) {
        // 管理員在 Profile 頁面的專用功能
        enhanceApp({
            state: {
                adminTools: {
                    moderationMode: false,
                    auditLog: []
                }
            },
            
            methods: {
                toggleModerationMode() {
                    this.adminTools.moderationMode = !this.adminTools.moderationMode
                    console.log('內容審核模式:', this.adminTools.moderationMode ? '開啟' : '關閉')
                },
                
                async banUser(userId) {
                    try {
                        const response = await fetch(`/api/admin/ban-user/${userId}`, {
                            method: 'POST'
                        })
                        
                        if (response.ok) {
                            this.adminTools.auditLog.push({
                                action: 'ban',
                                userId: userId,
                                timestamp: new Date(),
                                admin: this.currentUser.username
                            })
                            
                            alert('用戶已被禁用')
                        }
                    } catch (error) {
                        console.error('禁用用戶失敗:', error)
                    }
                },
                
                async deletePost(postId) {
                    if (confirm('確定要刪除此文章？')) {
                        try {
                            const response = await fetch(`/api/admin/delete-post/${postId}`, {
                                method: 'DELETE'
                            })
                            
                            if (response.ok) {
                                // 從現有的 posts 中移除
                                const index = this.posts.findIndex(p => p.articleId === postId)
                                if (index > -1) {
                                    this.posts.splice(index, 1)
                                }
                                
                                this.adminTools.auditLog.push({
                                    action: 'delete_post',
                                    postId: postId,
                                    timestamp: new Date(),
                                    admin: this.currentUser.username
                                })
                            }
                        } catch (error) {
                            console.error('刪除文章失敗:', error)
                        }
                    }
                }
            }
        })
        
        console.log('管理員工具已載入')
    }
})

//#endregion

//#region 整合到 Layout 的建議

/**
 * 在 Views/Shared/_Layout.cshtml 中的整合方式：
 * 
 * 原有的 Scripts 順序保持不變：
 * <script src="~/js/pages/profile.js"></script>  <!-- 保持原有 -->
 * <script src="~/js/main.js"></script>            <!-- 保持原有 -->
 * 
 * 添加相容適配器：
 * <script src="~/js/vue-adapter-compatible.js"></script>
 * 
 * 可選：添加全域增強（在最後）：
 * <script>
 *     // 全域增強功能
 *     window.whenAppReady((app) => {
 *         // 添加全站通用的增強功能
 *         enhanceApp({
 *             state: {
 *                 globalEnhancement: {
 *                     version: '2.0',
 *                     features: ['darkMode', 'notifications']
 *                 }
 *             }
 *         })
 *     })
 * </script>
 * 
 * 在個別頁面的 @section Scripts 中：
 * @section Scripts {
 *     <script>
 *         // 頁面特定的增強功能
 *         window.whenAppReady((app) => {
 *             // 頁面專用邏輯
 *         })
 *     </script>
 * }
 */

//#endregion

console.log('Vue 整合範例已載入')