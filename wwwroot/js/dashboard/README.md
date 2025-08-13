# Dashboard Vue.js 架構重構說明

## 新架構概述

### 架構分離
原本的單一應用架構已重構為多應用架構：

1. **Menu App** (`#dashboard-menu`)：專門處理選單和全域功能
2. **Page Apps** (`#page-specific-ids`)：各頁面獨立的 Vue 應用，使用對應的 `useXxx` 模組

### 檔案結構

```
wwwroot/js/dashboard/
├── d_main.js           # Menu App 主檔案
├── global-state.js     # 全域狀態管理
└── pages/
    ├── users/
    │   └── users.js    # useUsers 模組
    ├── posts/
    │   └── posts.js    # usePosts 模組
    ├── reports/
    │   └── reports.js  # useReports 模組
    ├── config/
    │   └── config.js   # useConfig 模組
    └── overview/
        └── overview.js # useOverview 模組
```

## 核心特色

### 1. 全域狀態管理 (`global-state.js`)
- **語系切換**：全域響應式語系管理
- **使用者狀態**：跨應用共享使用者資訊
- **主題管理**：深色/淺色主題切換
- **事件系統**：自定義事件在應用間通信

```javascript
// 使用範例
import { useGlobalState } from '/js/dashboard/global-state.js';

const { currentLanguage, setLanguage, currentUser } = useGlobalState()

// 切換語系
setLanguage('en-US') // 所有應用都會收到語系切換事件
```

### 2. Menu App (`d_main.js`)
- 專門綁定到 `#dashboard-menu`
- 負責選單功能、語系切換、使用者狀態顯示
- 不再載入各頁面的業務邏輯

### 3. 頁面獨立應用
每個 partial 頁面在自己的 `.cshtml` 檔案中使用對應的 `useXxx` 模組：

```html
@section Scripts{
    <script type="module">
        // 引入對應的頁面模組
        import { useUsers } from '/js/dashboard/pages/users/users.js';
        
        const { createApp, onUnmounted } = Vue
        
        const app = createApp({
            setup() {
                // 使用頁面模組獲取所有功能
                const usersModule = useUsers()
                
                // 設置 unmount 處理器
                usersModule.setupUnmountHandler(onUnmounted)
                
                // 返回所有模組功能
                return usersModule
            }
        })
        
        app.mount('#adminUser')
    </script>
}
```

### 4. 頁面模組架構 (`useXxx` 函數)
每個頁面模組都採用統一的結構：

```javascript
// users.js 範例
import { useGlobalState } from '/js/dashboard/global-state.js'

export const useUsers = () => {
  const { createApp, ref, computed, onMounted, onUnmounted } = Vue
  
  // 引入全域狀態
  const { currentLanguage, currentUser } = useGlobalState()
  
  // 頁面特定邏輯...
  
  // 語系切換監聽器
  let languageChangeHandler = null
  
  onMounted(() => {
    // 初始化邏輯
    languageChangeHandler = (event) => {
      console.log('語系已切換至', event.detail.language)
    }
    window.addEventListener('language-changed', languageChangeHandler)
  })
  
  // 生命週期輔助函數
  const setupUnmountHandler = (onUnmountedCallback) => {
    onUnmountedCallback(() => {
      if (languageChangeHandler) {
        window.removeEventListener('language-changed', languageChangeHandler)
      }
    })
  }
  
  return {
    // 全域狀態
    currentLanguage,
    currentUser,
    
    // 頁面功能...
    
    // 生命週期輔助
    setupUnmountHandler
  }
}
```

## 語系切換實現

### 全域語系切換
1. **Menu 觸發**：在選單中切換語系
2. **狀態更新**：全域狀態立即更新
3. **事件廣播**：發送 `language-changed` 自定義事件
4. **頁面響應**：各頁面監聽事件並更新內容

### 事件流程
```
Menu App (語系切換) 
    ↓
Global State (更新狀態)
    ↓
Custom Event (language-changed)
    ↓
All Page Apps (接收並更新)
```

## 優勢

### 1. 職責分離
- Menu：選單和全域功能
- Pages：頁面特定業務邏輯

### 2. 獨立性
- 各頁面 Vue 應用互不干擾
- 頁面載入錯誤不影響選單
- 更容易維護和除錯

### 3. 全域功能
- 語系切換影響所有應用
- 使用者狀態在所有頁面可用
- 主題設定全域生效

### 4. 效能優化
- 頁面按需載入邏輯
- 減少不必要的 JavaScript 載入
- 更好的快取策略

## 開發指南

### 新增頁面
1. 創建對應的 `useXxx.js` 模組文件
2. 在對應的 `.cshtml` 檔案中添加 `@section Scripts`
3. 引入對應的頁面模組
4. 使用統一的應用創建模式

### 頁面模組開發模式
```javascript
// 1. 創建 useXxx 模組
// pages/example/example.js
import { useGlobalState } from '/js/dashboard/global-state.js'

export const useExample = () => {
  const { ref, computed, onMounted } = Vue
  const { currentLanguage, currentUser } = useGlobalState()
  
  // 頁面特定狀態和邏輯
  const data = ref([])
  
  // 語系切換監聽
  let languageChangeHandler = null
  onMounted(() => {
    languageChangeHandler = (event) => {
      // 處理語系切換
    }
    window.addEventListener('language-changed', languageChangeHandler)
  })
  
  const setupUnmountHandler = (onUnmountedCallback) => {
    onUnmountedCallback(() => {
      if (languageChangeHandler) {
        window.removeEventListener('language-changed', languageChangeHandler)
      }
    })
  }
  
  return {
    currentLanguage,
    currentUser,
    data,
    setupUnmountHandler
  }
}

// 2. 在 .cshtml 中使用
// @section Scripts{
//     <script type="module">
//         import { useExample } from '/js/dashboard/pages/example/example.js';
//         
//         const app = Vue.createApp({
//             setup() {
//                 const exampleModule = useExample()
//                 exampleModule.setupUnmountHandler(Vue.onUnmounted)
//                 return exampleModule
//             }
//         })
//         app.mount('#example')
//     </script>
// }
```

### 使用全域狀態
```javascript
import { useGlobalState } from '/js/dashboard/global-state.js';

const { 
    currentLanguage,    // 當前語系
    currentUser,        // 使用者資訊
    theme,              // 主題設定
    setLanguage,        // 切換語系
    setTheme           // 切換主題
} = useGlobalState()
```

## 注意事項

1. **`#dashboard-content` 不綁定 Vue**：此區域由各 partial 頁面自行管理
2. **全域事件監聽**：記得在 `onUnmounted` 中移除事件監聽器
3. **狀態同步**：使用全域狀態而非本地狀態來處理跨應用數據
4. **模組化引入**：使用 ES6 模組語法引入全域狀態