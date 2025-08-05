/**
 * 通用 Vue 適配器 - 適用於所有 Views 頁面
 * 支援 Auth、Profile、Home、Follow、Notify 等頁面
 */

function createPageApp(config = {}) {
    // 檢測頁面類型和資訊
    const pageInfo = detectPageType()
    
    if (!pageInfo.isValidPage) {
        console.log('無法識別的頁面類型')
        return null
    }
    
    console.log(`偵測到頁面類型: ${pageInfo.type}`)
    
    // 根據頁面類型建立配置
    const appConfig = buildPageConfig(pageInfo, config)
    
    // 掛載應用
    return mountPageApp(appConfig, config.mountPoint || '#app')
}

// 檢測頁面類型
function detectPageType() {
    const path = window.location.pathname.toLowerCase()
    const segments = path.split('/').filter(s => s)
    
    // 頁面類型映射
    const pageTypes = {
        'auth': { type: 'auth', features: ['validation', 'form'] },
        'login': { type: 'auth', subtype: 'login', features: ['validation', 'form'] },
        'register': { type: 'auth', subtype: 'register', features: ['validation', 'form'] },
        'profile': { type: 'profile', features: ['user', 'edit', 'upload'] },
        'follow': { type: 'follow', features: ['user', 'social'] },
        'notify': { type: 'notify', features: ['realtime', 'list'] },
        'home': { type: 'home', features: ['feed', 'realtime'] },
        '': { type: 'home', features: ['feed', 'realtime'] } // 根目錄當作首頁
    }
    
    const controller = segments[0] || ''
    const pageConfig = pageTypes[controller]
    
    if (!pageConfig) {
        return { isValidPage: false }
    }
    
    return {
        isValidPage: true,
        type: pageConfig.type,
        subtype: pageConfig.subtype || null,
        controller: controller,
        action: segments[1] || 'index',
        features: pageConfig.features,
        path: path
    }
}

// 建立頁面配置
function buildPageConfig(pageInfo, userConfig) {
    // 根據頁面類型提供預設功能
    const pageDefaults = getPageDefaults(pageInfo)
    
    return {
        // Vue3 Composition API - 通用狀態
        setup() {
            const { reactive, ref } = Vue
            
            // 通用狀態
            const pageState = reactive({
                loading: false,
                errors: {},
                success: false,
                ...pageDefaults.state
            })
            
            // 用戶自定義 setup
            const userSetup = userConfig.setup ? userConfig.setup() : {}
            
            return { pageState, ...userSetup }
        },
        
        // Vue2 Options API - 基本功能
        data() {
            return {
                // 頁面基本資訊
                pageInfo: pageInfo,
                
                // 預設數據
                ...pageDefaults.data,
                
                // 用戶自定義數據
                ...(userConfig.data ? userConfig.data.call(this) : {})
            }
        },
        
        methods: {
            // 通用方法
            setLoading(state) { this.pageState.loading = state },
            setError(field, message) { this.pageState.errors[field] = message },
            clearErrors() { this.pageState.errors = {} },
            setSuccess(state) { this.pageState.success = state },
            
            // 頁面特定方法
            ...pageDefaults.methods,
            
            // 用戶自定義方法
            ...(userConfig.methods || {})
        },
        
        computed: {
            hasErrors() {
                return Object.keys(this.pageState.errors).length > 0
            },
            ...(userConfig.computed || {})
        },
        
        async mounted() {
            console.log(`${pageInfo.type} 頁面已載入`)
            
            // 頁面特定初始化
            if (pageDefaults.mounted) {
                await pageDefaults.mounted.call(this)
            }
            
            // 用戶自定義初始化
            if (userConfig.mounted) {
                await userConfig.mounted.call(this)
            }
        }
    }
}

// 取得頁面預設配置
function getPageDefaults(pageInfo) {
    const defaults = {
        auth: {
            state: { rememberMe: false },
            data: { 
                form: { email: '', password: '' },
                validationRules: {}
            },
            methods: {
                async submitForm() {
                    this.clearErrors()
                    this.setLoading(true)
                    
                    try {
                        const endpoint = pageInfo.subtype === 'register' ? '/api/auth/register' : '/api/auth/login'
                        const response = await fetch(endpoint, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(this.form)
                        })
                        
                        const result = await response.json()
                        
                        if (result.success) {
                            this.setSuccess(true)
                            setTimeout(() => window.location.href = result.redirectUrl || '/', 1000)
                        } else {
                            this.setError('general', result.message)
                        }
                    } catch (error) {
                        this.setError('general', '操作失敗，請稍後再試')
                    } finally {
                        this.setLoading(false)
                    }
                }
            }
        },
        
        profile: {
            data: { 
                user: {},
                isEditing: false 
            },
            methods: {
                async loadProfile() {
                    this.setLoading(true)
                    try {
                        const response = await fetch('/api/profile')
                        const result = await response.json()
                        this.user = result.user
                    } finally {
                        this.setLoading(false)
                    }
                },
                toggleEdit() { this.isEditing = !this.isEditing }
            },
            mounted() { return this.loadProfile() }
        },
        
        follow: {
            data: { 
                followers: [],
                following: [] 
            },
            methods: {
                async loadFollowData() {
                    this.setLoading(true)
                    try {
                        const response = await fetch('/api/follow')
                        const result = await response.json()
                        this.followers = result.followers
                        this.following = result.following
                    } finally {
                        this.setLoading(false)
                    }
                },
                async toggleFollow(userId) {
                    const response = await fetch('/api/follow/toggle', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ userId })
                    })
                    if (response.ok) await this.loadFollowData()
                }
            },
            mounted() { return this.loadFollowData() }
        },
        
        notify: {
            data: { 
                notifications: [],
                unreadCount: 0 
            },
            methods: {
                async loadNotifications() {
                    this.setLoading(true)
                    try {
                        const response = await fetch('/api/notifications')
                        const result = await response.json()
                        this.notifications = result.notifications
                        this.unreadCount = result.unreadCount
                    } finally {
                        this.setLoading(false)
                    }
                },
                async markAsRead(notificationId) {
                    await fetch(`/api/notifications/${notificationId}/read`, { method: 'POST' })
                    await this.loadNotifications()
                }
            },
            mounted() { return this.loadNotifications() }
        },
        
        home: {
            data: { 
                posts: [],
                currentPage: 1 
            },
            methods: {
                async loadPosts() {
                    this.setLoading(true)
                    try {
                        const response = await fetch(`/api/posts?page=${this.currentPage}`)
                        const result = await response.json()
                        this.posts = result.posts
                    } finally {
                        this.setLoading(false)
                    }
                },
                loadMore() {
                    this.currentPage++
                    this.loadPosts()
                }
            },
            mounted() { return this.loadPosts() }
        }
    }
    
    return defaults[pageInfo.type] || { state: {}, data: {}, methods: {} }
}

// 掛載頁面應用
function mountPageApp(appConfig, mountPoint) {
    try {
        const app = Vue.createApp(appConfig)
        const element = document.querySelector(mountPoint)
        
        if (!element) {
            console.error('掛載點不存在:', mountPoint)
            return null
        }
        
        return app.mount(mountPoint)
    } catch (error) {
        console.error('掛載應用失敗:', error)
        return null
    }
}

// 導出到全域
window.createPageApp = createPageApp
console.log('通用 Vue 適配器已載入')

/* 快速使用：
// 零配置 - 自動根據頁面類型提供功能
createPageApp()

// 自訂配置
createPageApp({
    data: () => ({ customData: 'hello' }),
    methods: { customMethod() {} },
    setup() { return { reactiveData: ref('world') } }
})
*/