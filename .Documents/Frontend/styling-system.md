# 樣式系統技術文件

**技術分類**: 前端樣式與設計系統  
**複雜度**: 基礎到中級  
**適用情境**: 現代化 UI 設計、響應式佈局、組件化樣式  

## 技術概述

Matrix 專案採用 Tailwind CSS + DaisyUI + SCSS 的混合樣式架構，結合組件化設計理念，提供一致且高效的使用者介面。

## 基礎技術

### 1. 技術棧組合
- **Tailwind CSS**: 實用程式優先的 CSS 框架
- **DaisyUI**: Tailwind CSS 的語義化組件庫  
- **SCSS**: CSS 預處理器，用於複雜樣式邏輯
- **Lucide Icons**: 現代化圖示庫

### 2. 設定檔案結構
```
wwwroot/
├── scss/                     # SCSS 源檔案
│   ├── main.scss            # 主要樣式進入點
│   └── components/          # 組件樣式
│       ├── _global.scss     # 全域樣式
│       ├── _home.scss       # 首頁樣式
│       ├── _auth.scss       # 認證頁樣式
│       └── dashboard/       # 管理後台樣式
├── css/                     # 編譯後的 CSS
│   ├── tailwind.css         # Tailwind 編譯結果
│   ├── site.css            # 網站主要樣式
│   └── components.css       # 組件樣式
└── lib/                     # 第三方樣式庫
    ├── daisyui/            # DaisyUI 組件
    └── lucide/             # 圖示庫
```

### 3. Tailwind CSS 設定 (tailwind.config.js:1-25)
```javascript
module.exports = {
  content: [
    "./Views/**/*.{cshtml,html}",      # Razor 視圖
    "./Areas/**/*.{cshtml,html}",      # Area 視圖  
    "./wwwroot/scss/**/*.scss",        # SCSS 檔案
  ],
  theme: {
    extend: {
      colors: {
        primary: '#1e40af',             # 主色調
        secondary: '#1f2937',           # 次要色調
      },
      spacing: {
        '18': '4.5rem',                # 自定義間距
      }
    },
  },
  plugins: [
    require("./wwwroot/lib/daisyui/daisyui.js"),
    require("./wwwroot/lib/daisyui/daisyui-theme.js")
  ],
}
```

## 進階技術

### 1. DaisyUI 組件系統
```html
<!-- 按鈕組件 -->
<button class="btn btn-primary btn-lg">
  主要按鈕
</button>

<!-- 卡片組件 -->
<div class="card bg-base-100 shadow-xl">
  <div class="card-body">
    <h2 class="card-title">卡片標題</h2>
    <p>卡片內容</p>
    <div class="card-actions justify-end">
      <button class="btn btn-primary">確認</button>
    </div>
  </div>
</div>

<!-- 模態框組件 -->
<dialog class="modal modal-open">
  <div class="modal-box">
    <h3 class="font-bold text-lg">模態框標題</h3>
    <p class="py-4">模態框內容</p>
    <div class="modal-action">
      <button class="btn">關閉</button>
    </div>
  </div>
</dialog>
```

### 2. SCSS 模組化架構
```scss
// scss/main.scss - 主要進入點
@import 'components/global';
@import 'components/home';  
@import 'components/auth';
@import 'components/dashboard/overview';

// scss/components/_global.scss - 全域樣式
.loading-spinner {
  @apply animate-spin rounded-full h-8 w-8 border-b-2 border-primary;
}

.gradient-bg {
  background: linear-gradient(135deg, theme('colors.primary'), theme('colors.secondary'));
}

// scss/components/_home.scss - 首頁特定樣式  
.hero-section {
  @apply min-h-screen flex items-center justify-center;
  background-image: url('/static/img/landingBG.jpg');
  background-size: cover;
  background-position: center;
  
  &::before {
    content: '';
    @apply absolute inset-0 bg-black bg-opacity-50;
  }
}

.post-card {
  @apply bg-white rounded-lg shadow-md p-4 mb-4 transition-shadow duration-300;
  
  &:hover {
    @apply shadow-lg;
  }
  
  .post-meta {
    @apply text-sm text-gray-500 mb-2;
  }
  
  .post-content {
    @apply text-gray-800 leading-relaxed;
  }
}
```

### 3. 響應式設計系統
```scss
// 響應式斷點
$breakpoints: (
  'sm': 640px,
  'md': 768px,  
  'lg': 1024px,
  'xl': 1280px,
  '2xl': 1536px
);

// 響應式 Mixin
@mixin responsive($breakpoint) {
  @media (min-width: map-get($breakpoints, $breakpoint)) {
    @content;
  }
}

// 使用範例
.sidebar {
  @apply w-full;
  
  @include responsive('lg') {
    @apply w-64 fixed left-0 top-0 h-full;
  }
}

// Tailwind 響應式類別
.responsive-grid {
  @apply grid grid-cols-1 gap-4;
  @apply md:grid-cols-2;
  @apply lg:grid-cols-3;
  @apply xl:grid-cols-4;
}
```

## 設計系統

### 1. 色彩系統
```scss
// 主色調
:root {
  --color-primary: #1e40af;        // 主要藍色
  --color-primary-hover: #1e3a8a;  // 主要藍色懸停
  --color-secondary: #1f2937;      // 次要灰色
  --color-accent: #10b981;         // 強調綠色
  --color-warning: #f59e0b;        // 警告黃色
  --color-error: #ef4444;          // 錯誤紅色
}

// 語義化色彩
.text-success { @apply text-green-600; }
.text-warning { @apply text-yellow-600; }
.text-error { @apply text-red-600; }
.bg-success { @apply bg-green-100 border-green-500; }
.bg-warning { @apply bg-yellow-100 border-yellow-500; }
.bg-error { @apply bg-red-100 border-red-500; }
```

### 2. 字體系統
```scss
// 字體家族
.font-primary {
  font-family: 'SourceSans3-Regular', 'Microsoft JhengHei', sans-serif;
}

.font-display {
  font-family: 'RubikGlitch-Regular', monospace;
}

// 字體大小階層
.text-xs { font-size: 0.75rem; }      // 12px
.text-sm { font-size: 0.875rem; }     // 14px  
.text-base { font-size: 1rem; }       // 16px
.text-lg { font-size: 1.125rem; }     // 18px
.text-xl { font-size: 1.25rem; }      // 20px
.text-2xl { font-size: 1.5rem; }      // 24px
.text-3xl { font-size: 1.875rem; }    // 30px

// 行高設定
.leading-tight { line-height: 1.25; }
.leading-normal { line-height: 1.5; }
.leading-relaxed { line-height: 1.625; }
```

### 3. 間距系統
```scss
// Tailwind 間距系統擴展
.space-y-18 > :not([hidden]) ~ :not([hidden]) {
  margin-top: 4.5rem;
}

// 自定義間距類別
.section-padding { @apply py-16 px-4; }
.container-padding { @apply px-4 md:px-8 lg:px-12; }
.card-padding { @apply p-6; }
.button-padding { @apply px-4 py-2; }
```

## 組件樣式實作

### 1. 按鈕組件
```scss
// 基礎按鈕樣式
.btn-base {
  @apply inline-flex items-center justify-center rounded-md font-medium transition-colors;
  @apply focus:outline-none focus:ring-2 focus:ring-offset-2;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
}

// 按鈕變體
.btn-primary {
  @apply btn-base bg-primary text-white;
  @apply hover:bg-primary-hover focus:ring-primary;
}

.btn-secondary {
  @apply btn-base bg-gray-200 text-gray-900;
  @apply hover:bg-gray-300 focus:ring-gray-500;
}

.btn-outline {
  @apply btn-base border-2 border-primary text-primary bg-transparent;
  @apply hover:bg-primary hover:text-white focus:ring-primary;
}

// 按鈕大小
.btn-sm { @apply text-sm px-3 py-1.5; }
.btn-md { @apply text-base px-4 py-2; }
.btn-lg { @apply text-lg px-6 py-3; }
```

### 2. 表單組件
```scss
// 輸入框樣式
.form-input {
  @apply block w-full px-3 py-2 border border-gray-300 rounded-md;
  @apply focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary;
  @apply disabled:bg-gray-100 disabled:cursor-not-allowed;
  
  &.error {
    @apply border-red-500 focus:ring-red-500 focus:border-red-500;
  }
  
  &.success {
    @apply border-green-500 focus:ring-green-500 focus:border-green-500;
  }
}

// 標籤樣式
.form-label {
  @apply block text-sm font-medium text-gray-700 mb-1;
  
  &.required::after {
    content: '*';
    @apply text-red-500 ml-1;
  }
}

// 錯誤訊息
.form-error {
  @apply text-sm text-red-600 mt-1;
}
```

### 3. 卡片組件
```scss
.card {
  @apply bg-white rounded-lg shadow-sm border border-gray-200;
  @apply transition-shadow duration-200;
  
  &:hover {
    @apply shadow-md;
  }
  
  .card-header {
    @apply px-6 py-4 border-b border-gray-200;
    
    .card-title {
      @apply text-lg font-semibold text-gray-900;
    }
    
    .card-subtitle {
      @apply text-sm text-gray-500 mt-1;
    }
  }
  
  .card-body {
    @apply px-6 py-4;
  }
  
  .card-footer {
    @apply px-6 py-4 border-t border-gray-200 bg-gray-50;
  }
}
```

## 動畫與互動效果

### 1. 過渡動畫
```scss
// 基礎過渡
.transition-base {
  @apply transition-all duration-200 ease-in-out;
}

// 載入動畫
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.loading {
  animation: spin 1s linear infinite;
}

// 淡入動畫
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.fade-in {
  animation: fadeIn 0.3s ease-in-out;
}

// 滑入動畫
@keyframes slideIn {
  from { transform: translateY(-10px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.slide-in {
  animation: slideIn 0.3s ease-out;
}
```

### 2. 懸停效果
```scss
// 按鈕懸停效果
.btn-hover-lift {
  @apply transition-transform duration-200;
  
  &:hover {
    @apply -translate-y-0.5 shadow-lg;
  }
}

// 卡片懸停效果
.card-hover {
  @apply transition-all duration-300;
  
  &:hover {
    @apply shadow-xl scale-105;
  }
}

// 圖片懸停效果
.image-hover {
  @apply transition-transform duration-300 overflow-hidden;
  
  img {
    @apply transition-transform duration-300;
  }
  
  &:hover img {
    @apply scale-110;
  }
}
```

## 實際應用情境

### 1. 首頁佈局
```html
<!-- 英雄區塊 -->
<section class="hero-section relative">
  <div class="hero-content relative z-10 text-center text-white">
    <h1 class="text-5xl font-bold mb-6">歡迎來到 Matrix</h1>
    <p class="text-xl mb-8">連接創意，分享靈感</p>
    <button class="btn btn-primary btn-lg">開始探索</button>
  </div>
</section>

<!-- 功能介紹 -->
<section class="section-padding bg-gray-50">
  <div class="container mx-auto">
    <div class="responsive-grid">
      <div class="card text-center">
        <div class="card-body">
          <i class="lucide lucide-users text-4xl text-primary mb-4"></i>
          <h3 class="card-title">社群互動</h3>
          <p>與志同道合的朋友交流</p>
        </div>
      </div>
      <!-- 更多卡片... -->
    </div>
  </div>
</section>
```

### 2. 表單設計
```html
<form class="max-w-md mx-auto">
  <div class="mb-6">
    <label class="form-label required">用戶名</label>
    <input type="text" class="form-input" placeholder="請輸入用戶名">
    <div class="form-error">用戶名為必填欄位</div>
  </div>
  
  <div class="mb-6">
    <label class="form-label required">密碼</label>
    <input type="password" class="form-input" placeholder="請輸入密碼">
  </div>
  
  <button type="submit" class="btn btn-primary w-full">
    登入
  </button>
</form>
```

### 3. 管理後台樣式
```scss
// Dashboard 特定樣式
.dashboard {
  @apply min-h-screen bg-gray-100;
  
  .sidebar {
    @apply w-64 bg-white shadow-sm fixed left-0 top-0 h-full;
    @apply border-r border-gray-200 z-40;
    
    .nav-item {
      @apply block px-4 py-2 text-gray-700;
      @apply hover:bg-gray-100 hover:text-primary;
      @apply transition-colors duration-200;
      
      &.active {
        @apply bg-primary text-white;
      }
    }
  }
  
  .main-content {
    @apply ml-64 p-6;
    
    .page-header {
      @apply mb-8;
      
      h1 {
        @apply text-3xl font-bold text-gray-900;
      }
    }
    
    .stats-grid {
      @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8;
      
      .stat-card {
        @apply bg-white rounded-lg p-6 shadow-sm;
        
        .stat-value {
          @apply text-3xl font-bold text-primary;
        }
        
        .stat-label {
          @apply text-sm text-gray-500 uppercase tracking-wide;
        }
      }
    }
  }
}
```

## 效能優化

### 1. CSS 優化策略
```scss
// 使用 CSS 自定義屬性減少重複
:root {
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.shadow-custom-sm { box-shadow: var(--shadow-sm); }
.shadow-custom-md { box-shadow: var(--shadow-md); }
.shadow-custom-lg { box-shadow: var(--shadow-lg); }

// 避免重複的動畫定義
.animate-shared {
  animation-duration: 0.3s;
  animation-timing-function: ease-in-out;
  animation-fill-mode: both;
}

.fade-in { 
  @extend .animate-shared;
  animation-name: fadeIn; 
}

.slide-in { 
  @extend .animate-shared;
  animation-name: slideIn; 
}
```

### 2. 載入優化
```scss
// 關鍵路徑 CSS 內聯
.critical {
  // 首屏必要樣式
  @apply text-base leading-normal;
}

// 非關鍵樣式延遲載入
.non-critical {
  // 可以延遲載入的裝飾性樣式
  @apply shadow-lg rounded-xl;
}
```

---

**建立日期**: 2025-08-29  
**適用版本**: Tailwind CSS 3.x, DaisyUI 4.x  
**相關檔案**: tailwind.config.js, wwwroot/scss/, wwwroot/css/  
**設計原則**: 實用性優先、組件化、響應式  
**學習資源**: [Tailwind CSS](https://tailwindcss.com), [DaisyUI](https://daisyui.com)