/**
 * 超極簡 Dashboard Vue 適配器 - 70行版本
 * 統一混合語法，自動支援 Vue2/Vue3
 */

function createDashboardApp(config = {}) {
    // 頁面檢測
    const path = window.location.pathname.toLowerCase()
    if (!path.includes('/dashboard/')) {
        console.log('非 Dashboard 頁面')
        return null
    }
    
    // 統一混合配置 - 同時支援 Vue2 和 Vue3 語法
    const appConfig = {
        // Vue3 Composition API
        setup() {
            const { reactive } = Vue
            const state = reactive({
                loading: false,
                items: [],
                currentPage: 1,
                searchQuery: ''
            })
            
            // 用戶自定義 setup
            const userSetup = config.setup ? config.setup() : {}
            
            return { ...state, ...userSetup }
        },
        
        // Vue2 Options API
        data() {
            return {
                pageSize: 20,
                selectedItems: [],
                ...(config.data ? config.data.call(this) : {})
            }
        },
        
        methods: {
            // 內建方法
            setLoading(state) { this.loading = state },
            async loadData() {
                this.setLoading(true)
                try {
                    const controller = path.split('/')[2] || 'overview'
                    const response = await fetch(`/api/dashboard/${controller}`)
                    const data = await response.json()
                    this.items = data.items || []
                } catch (error) {
                    console.error('載入失敗:', error)
                } finally {
                    this.setLoading(false)
                }
            },
            goToPage(page) {
                this.currentPage = page
                this.loadData()
            },
            // 用戶自定義方法
            ...(config.methods || {})
        },
        
        computed: config.computed || {},
        
        async mounted() {
            console.log('Dashboard 已載入')
            if (config.mounted) await config.mounted.call(this)
            await this.loadData()
        }
    }
    
    // 掛載應用
    try {
        const app = Vue.createApp(appConfig)
        const mountPoint = config.mountPoint || '#dashboard-content'
        const element = document.querySelector(mountPoint)
        
        if (!element) {
            console.error('掛載點不存在:', mountPoint)
            return null
        }
        
        return app.mount(mountPoint)
    } catch (error) {
        console.error('掛載失敗:', error)
        return null
    }
}

// 導出
window.createDashboardApp = createDashboardApp
console.log('超極簡 Dashboard 適配器已載入')

/* 使用範例：
// 最簡單
createDashboardApp()

// Vue2 風格
createDashboardApp({
    data: () => ({ posts: [] }),
    methods: { deletePo st(id) { ... } }
})

// Vue3 風格  
createDashboardApp({
    setup() {
        const users = ref([])
        return { users }
    }
})

// 混合風格
createDashboardApp({
    setup() { return { adminData: reactive({}) } },
    data: () => ({ uiState: 'ready' }),
    methods: { handleClick() { ... } }
})
*/