/**
 * 相容版 Vue 適配器 - 與現有 main.js 和 profile.js 完美共存
 * 可擴展現有 Vue 應用，無需修改現有代碼
 */

//#region 相容性適配器
class VueCompatAdapter {
    constructor() {
        this.existingApp = null
        this.enhancementRegistered = false
        this.pageEnhancements = new Map()
        
        // 延遲初始化，等待現有應用載入
        this.waitForExistingApp()
    }
    
    // 等待現有的 Vue 應用載入
    async waitForExistingApp() {
        // 輪詢檢查現有應用
        for (let i = 0; i < 50; i++) { // 最多等待5秒
            if (window.globalApp) {
                this.existingApp = window.globalApp
                console.log('檢測到現有 Vue 應用，準備增強功能')
                break
            }
            await new Promise(resolve => setTimeout(resolve, 100))
        }
        
        if (!this.existingApp) {
            console.log('未檢測到現有 Vue 應用，將建立新的應用')
        }
    }
    
    // 增強現有應用（不破壞現有功能）
    enhanceExistingApp(enhancements = {}) {
        if (!this.existingApp) {
            console.warn('沒有現有應用可以增強')
            return null
        }
        
        // 將增強功能混入現有應用
        try {
            // 添加新的 reactive 狀態
            if (enhancements.state) {
                Object.keys(enhancements.state).forEach(key => {
                    if (!this.existingApp[key]) {
                        this.existingApp[key] = Vue.reactive(enhancements.state[key])
                    }
                })
            }
            
            // 添加新的方法
            if (enhancements.methods) {
                Object.keys(enhancements.methods).forEach(key => {
                    if (!this.existingApp[key]) {
                        this.existingApp[key] = enhancements.methods[key].bind(this.existingApp)
                    }
                })
            }
            
            console.log('現有應用增強成功')
            return this.existingApp
            
        } catch (error) {
            console.error('增強現有應用失敗:', error)
            return null
        }
    }
    
    // 創建獨立的頁面應用（不與 main.js 衝突）
    createPageApp(config = {}) {
        const pageInfo = this.detectPageInfo()
        
        // 如果是 Profile 頁面且現有應用已處理，則增強現有應用
        if (pageInfo.isProfile && this.existingApp && typeof useProfile === 'function') {
            return this.enhanceProfileApp(config)
        }
        
        // 否則創建獨立應用
        return this.createIndependentApp(config, pageInfo)
    }
    
    // 增強 Profile 頁面的現有功能
    enhanceProfileApp(config = {}) {
        const enhancements = {
            state: {
                // 新增的 reactive 狀態
                pageEnhancement: {
                    advancedFilters: false,
                    realTimeUpdates: false,
                    customTheme: 'default',
                    ...config.enhancedState
                }
            },
            
            methods: {
                // 新增的方法
                toggleAdvancedFilters() {
                    this.pageEnhancement.advancedFilters = !this.pageEnhancement.advancedFilters
                },
                
                enableRealTimeUpdates() {
                    this.pageEnhancement.realTimeUpdates = true
                    // 可以在這裡加入 WebSocket 連接邏輯
                },
                
                setCustomTheme(theme) {
                    this.pageEnhancement.customTheme = theme
                    document.body.className = `theme-${theme}`
                },
                
                // 用戶自定義方法
                ...config.methods
            }
        }
        
        return this.enhanceExistingApp(enhancements)
    }
    
    // 創建獨立應用（用於非 Profile 頁面或無現有應用的情況）
    createIndependentApp(config, pageInfo) {
        const appConfig = this.buildIndependentAppConfig(config, pageInfo)
        
        // 使用不同的掛載點，避免與 main.js 衝突
        const mountPoint = config.mountPoint || '#page-specific-app'
        
        // 檢查掛載點是否存在
        let element = document.querySelector(mountPoint)
        if (!element && mountPoint !== '#app') {
            // 如果自定義掛載點不存在，動態創建
            element = document.createElement('div')
            element.id = mountPoint.substring(1)
            document.body.appendChild(element)
        }
        
        if (!element) {
            console.error('無法找到或創建掛載點:', mountPoint)
            return null
        }
        
        try {
            const app = Vue.createApp(appConfig)
            return app.mount(mountPoint)
        } catch (error) {
            console.error('創建獨立應用失敗:', error)
            return null
        }
    }
    
    // 建構獨立應用配置
    buildIndependentAppConfig(config, pageInfo) {
        return {
            setup() {
                const { reactive, ref } = Vue
                
                // 基本狀態
                const pageState = reactive({
                    loading: false,
                    error: null,
                    pageType: pageInfo.type,
                    ...config.defaultState
                })
                
                // 用戶自定義 setup
                const userSetup = config.setup ? config.setup() : {}
                
                return {
                    pageState,
                    ...userSetup
                }
            },
            
            data() {
                return {
                    pageInfo: pageInfo,
                    ...(config.data ? config.data.call(this) : {})
                }
            },
            
            methods: {
                setLoading(state) { this.pageState.loading = state },
                setError(error) { this.pageState.error = error },
                clearError() { this.pageState.error = null },
                ...(config.methods || {})
            },
            
            computed: config.computed || {},
            
            async mounted() {
                console.log(`獨立頁面應用已載入: ${pageInfo.type}`)
                if (config.mounted) {
                    await config.mounted.call(this)
                }
            }
        }
    }
    
    // 檢測頁面資訊
    detectPageInfo() {
        const path = window.location.pathname.toLowerCase()
        
        return {
            type: path.includes('/profile') ? 'profile' : 
                  path.includes('/auth') ? 'auth' :
                  path.includes('/dashboard') ? 'dashboard' : 'general',
            isProfile: path.includes('/profile'),
            isDashboard: path.includes('/dashboard'),
            path: path
        }
    }
}
//#endregion

//#region 全域實例與便利函數
const vueAdapter = new VueCompatAdapter()

// 增強現有應用的便利函數
window.enhanceApp = (enhancements) => {
    return vueAdapter.enhanceExistingApp(enhancements)
}

// 創建頁面應用的便利函數
window.createCompatibleApp = (config) => {
    return vueAdapter.createPageApp(config)
}

// 等待現有應用並執行回調
window.whenAppReady = async (callback) => {
    await vueAdapter.waitForExistingApp()
    if (vueAdapter.existingApp) {
        callback(vueAdapter.existingApp)
    }
}

console.log('相容版 Vue 適配器已載入')
//#endregion

//#region 自動增強機制
// 根據頁面類型自動提供增強功能
document.addEventListener('DOMContentLoaded', async () => {
    // 等待現有應用載入
    await vueAdapter.waitForExistingApp()
    
    const pageInfo = vueAdapter.detectPageInfo()
    
    // 自動為 Profile 頁面提供增強功能
    if (pageInfo.isProfile && vueAdapter.existingApp) {
        vueAdapter.enhanceProfileApp({
            enhancedState: {
                autoSave: true,
                darkMode: false
            },
            methods: {
                toggleAutoSave() {
                    this.pageEnhancement.autoSave = !this.pageEnhancement.autoSave
                    console.log('自動儲存:', this.pageEnhancement.autoSave ? '開啟' : '關閉')
                },
                
                toggleDarkMode() {
                    this.pageEnhancement.darkMode = !this.pageEnhancement.darkMode
                    document.body.classList.toggle('dark-mode', this.pageEnhancement.darkMode)
                }
            }
        })
        
        console.log('Profile 頁面自動增強完成')
    }
})
//#endregion