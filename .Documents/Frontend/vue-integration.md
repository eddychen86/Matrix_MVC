# Vue.js 前端整合技術文件

**技術分類**: 前端框架整合  
**複雜度**: 中級  
**適用情境**: ASP.NET Core MVC + Vue.js 混合開發  

## 技術概述

Matrix 專案採用 ASP.NET Core MVC 搭配 Vue.js 的混合開發模式，利用 LibMan 管理前端套件，Vue.js 作為頁面互動邏輯的核心。

## 基礎技術

### 1. Vue.js 版本與設定
- **版本**: Vue.js 3.5.17
- **載入方式**: CDN + LibMan 管理 (libman.json:6-13)
- **掛載方式**: 全域應用程式實例

### 2. 檔案結構
```
wwwroot/js/
├── main.js                   # 主要進入點
├── components/               # Vue 組件
│   ├── menu.js              # 選單組件
│   ├── create-post.js       # 發文組件
│   ├── user-manager.js      # 用戶管理
│   └── popup-manager.js     # 彈窗管理
├── hooks/                   # 自定義 Hook
│   ├── useFormatting.js     # 格式化工具
│   └── useApiWithLoading.js # API 載入狀態
├── pages/                   # 頁面特定邏輯
│   ├── home/                # 首頁
│   ├── profile/             # 個人檔案
│   └── about/               # 關於頁面
└── utils/                   # 工具函數
    └── loadingManager.js    # 載入管理器
```

### 3. 初始化設定 (main.js:19-51)
```javascript
const globalApp = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue not ready, retrying...')
        return
    } else {
        lucide.createIcons() // 圖示庫初始化
        
        const app = Vue.createApp(content)
        
        // 警告處理器設定
        app.config.warnHandler = (msg) => {
            if (msg.includes('Tags with side effect')) {
                return // 忽略特定警告
            }
            console.warn(msg)
        }
        
        // 掛載應用程式並整合 CKEditor
        window.globalApp = app.use(window.CKEditor?.default || window.CKEditor).mount('#app')
        
        // 標記應用程式已載入
        document.getElementById('app').setAttribute('data-v-app', 'true')
    }
}
```

## 進階技術

### 1. 模組化組件系統
```javascript
// components/user-manager.js 範例
export const useUserManager = () => {
    const currentUser = Vue.ref(null)
    const isLoggedIn = Vue.computed(() => currentUser.value !== null)
    
    const login = async (credentials) => {
        // 登入邏輯
    }
    
    const logout = () => {
        // 登出邏輯
    }
    
    return {
        currentUser,
        isLoggedIn,
        login,
        logout
    }
}
```

### 2. Composition API Hook 模式
```javascript
// hooks/useApiWithLoading.js
export const useApiWithLoading = () => {
    const loading = Vue.ref(false)
    const error = Vue.ref(null)
    
    const execute = async (apiCall) => {
        loading.value = true
        error.value = null
        
        try {
            const result = await apiCall()
            return result
        } catch (err) {
            error.value = err.message
            throw err
        } finally {
            loading.value = false
        }
    }
    
    return {
        loading: Vue.readonly(loading),
        error: Vue.readonly(error),
        execute
    }
}
```

### 3. 響應式資料管理
```javascript
// 全域狀態管理範例
const globalState = Vue.reactive({
    user: null,
    notifications: [],
    posts: [],
    loading: false
})

// 計算屬性
const unreadNotifications = Vue.computed(() => 
    globalState.notifications.filter(n => !n.isRead)
)
```

## 與 ASP.NET Core 整合

### 1. Razor 視圖整合
```html
<!-- Views/Shared/_Layout.cshtml -->
<div id="app">
    <!-- Vue.js 掛載點 -->
    @RenderBody()
</div>

<script src="~/lib/vue/dist/vue.global.js"></script>
<script type="module" src="~/js/main.js"></script>
```

### 2. 資料傳遞機制
```html
<!-- 從 C# 傳遞資料到 Vue -->
<script>
    window.initialData = {
        user: @Html.Raw(Json.Serialize(ViewBag.CurrentUser)),
        settings: @Html.Raw(Json.Serialize(ViewBag.AppSettings))
    };
</script>
```

### 3. CSRF Token 整合
```javascript
// 從 ASP.NET Core 取得 CSRF Token
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

// 在 Ajax 請求中使用
const response = await fetch('/api/posts', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': token
    },
    body: JSON.stringify(data)
});
```

## 使用流程

### 1. 建立新的 Vue 組件
```javascript
// 1. 建立組件檔案 components/new-component.js
export const useNewComponent = () => {
    // 響應式資料
    const data = Vue.ref([])
    const loading = Vue.ref(false)
    
    // 方法
    const fetchData = async () => {
        loading.value = true
        try {
            const response = await fetch('/api/data')
            data.value = await response.json()
        } finally {
            loading.value = false
        }
    }
    
    // 生命週期
    Vue.onMounted(() => {
        fetchData()
    })
    
    return {
        data: Vue.readonly(data),
        loading: Vue.readonly(loading),
        fetchData
    }
}

// 2. 在 main.js 中匯入
import { useNewComponent } from '/js/components/new-component.js'

// 3. 在頁面中使用
const content = {
    setup() {
        const { data, loading, fetchData } = useNewComponent()
        
        return {
            data,
            loading,
            fetchData
        }
    }
}
```

### 2. 頁面特定邏輯整合
```javascript
// pages/profile/profile.js
export const useProfile = () => {
    const { currentUser } = useUserManager()
    const { execute, loading } = useApiWithLoading()
    
    const updateProfile = async (profileData) => {
        return await execute(async () => {
            const response = await fetch('/api/profile', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(profileData)
            })
            return await response.json()
        })
    }
    
    return {
        currentUser,
        updateProfile,
        loading
    }
}
```

## 技術原理

### 1. ES6 模組系統
- **動態匯入**: `import()` 支援按需載入
- **模組封裝**: 每個組件獨立模組
- **依賴管理**: 明確的 import/export 關係

### 2. Vue 3 Composition API
- **setup()** 函數：組件邏輯組織
- **響應式系統**: ref(), reactive(), computed()
- **生命週期**: onMounted(), onUnmounted() 等

### 3. 響應式原理
```javascript
// Vue 3 Proxy 基礎響應式
const data = Vue.reactive({
    count: 0
})

// 自動追蹤依賴
const doubleCount = Vue.computed(() => data.count * 2)

// 響應式更新
data.count++ // 自動觸發相關更新
```

## 實際應用情境

### 1. 即時聊天功能
```javascript
// components/chat.js
export const useChat = () => {
    const messages = Vue.ref([])
    const connection = Vue.ref(null)
    
    const initSignalR = () => {
        connection.value = new signalR.HubConnectionBuilder()
            .withUrl('/matrixHub')
            .build()
            
        connection.value.on('ReceiveMessage', (message) => {
            messages.value.push(message)
        })
        
        connection.value.start()
    }
    
    const sendMessage = async (text) => {
        if (connection.value) {
            await connection.value.invoke('SendMessage', text)
        }
    }
    
    Vue.onMounted(initSignalR)
    Vue.onUnmounted(() => {
        if (connection.value) {
            connection.value.stop()
        }
    })
    
    return {
        messages: Vue.readonly(messages),
        sendMessage
    }
}
```

### 2. 表單驗證與提交
```javascript
// hooks/useForm.js
export const useForm = (initialData, validationRules) => {
    const formData = Vue.reactive({ ...initialData })
    const errors = Vue.reactive({})
    
    const validate = () => {
        const newErrors = {}
        
        Object.keys(validationRules).forEach(field => {
            const rule = validationRules[field]
            const value = formData[field]
            
            if (rule.required && !value) {
                newErrors[field] = `${field} 為必填欄位`
            }
        })
        
        Object.assign(errors, newErrors)
        return Object.keys(newErrors).length === 0
    }
    
    const submit = async (submitHandler) => {
        if (validate()) {
            await submitHandler(formData)
        }
    }
    
    return {
        formData,
        errors: Vue.readonly(errors),
        validate,
        submit
    }
}
```

### 3. 無限滾動載入
```javascript
// hooks/useInfiniteScroll.js
export const useInfiniteScroll = (fetchFunction) => {
    const items = Vue.ref([])
    const loading = Vue.ref(false)
    const hasMore = Vue.ref(true)
    const page = Vue.ref(1)
    
    const loadMore = async () => {
        if (loading.value || !hasMore.value) return
        
        loading.value = true
        try {
            const newItems = await fetchFunction(page.value)
            
            if (newItems.length === 0) {
                hasMore.value = false
            } else {
                items.value.push(...newItems)
                page.value++
            }
        } finally {
            loading.value = false
        }
    }
    
    // 監聽滾動事件
    Vue.onMounted(() => {
        const handleScroll = () => {
            const scrollPosition = window.innerHeight + window.scrollY
            const documentHeight = document.documentElement.scrollHeight
            
            if (scrollPosition >= documentHeight - 1000) {
                loadMore()
            }
        }
        
        window.addEventListener('scroll', handleScroll)
        loadMore() // 初始載入
        
        Vue.onUnmounted(() => {
            window.removeEventListener('scroll', handleScroll)
        })
    })
    
    return {
        items: Vue.readonly(items),
        loading: Vue.readonly(loading),
        hasMore: Vue.readonly(hasMore),
        loadMore
    }
}
```

## 效能優化

### 1. 延遲載入
```javascript
// 動態匯入組件
const loadComponent = async (componentName) => {
    const module = await import(`/js/components/${componentName}.js`)
    return module.default || module[`use${componentName}`]
}
```

### 2. 事件防抖
```javascript
// utils/debounce.js
export const debounce = (func, wait) => {
    let timeout
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout)
            func(...args)
        }
        clearTimeout(timeout)
        timeout = setTimeout(later, wait)
    }
}

// 使用範例
const debouncedSearch = debounce(searchFunction, 300)
```

### 3. 虛擬滾動
```javascript
// 大量資料渲染優化
export const useVirtualScroll = (items, itemHeight) => {
    const containerHeight = Vue.ref(400)
    const scrollTop = Vue.ref(0)
    
    const visibleItems = Vue.computed(() => {
        const startIndex = Math.floor(scrollTop.value / itemHeight)
        const endIndex = Math.min(
            startIndex + Math.ceil(containerHeight.value / itemHeight) + 1,
            items.length
        )
        
        return items.slice(startIndex, endIndex).map((item, index) => ({
            ...item,
            index: startIndex + index,
            top: (startIndex + index) * itemHeight
        }))
    })
    
    return {
        visibleItems,
        containerHeight,
        scrollTop
    }
}
```

---

**建立日期**: 2025-08-29  
**適用版本**: Vue.js 3.5.17  
**相關檔案**: wwwroot/js/main.js, libman.json  
**整合框架**: ASP.NET Core MVC  
**學習資源**: [Vue.js 3 官方文檔](https://vuejs.org/guide/)