# API 端點詳細說明

## 📋 概覽

本文檔詳細說明所有與 SignalR 即時更新整合的 API 端點，包括請求格式、回應格式和使用範例。

## 👍 按讚功能 API

### 切換按讚狀態
```http
POST /api/praise/toggle/{articleId}
```

#### 參數
- `articleId` (Guid): 文章 ID

#### 回應格式
```json
{
  "success": true,
  "isLiked": true,
  "newCount": 42,
  "message": "已按讚"
}
```

#### 使用範例
```javascript
const response = await fetch('/api/praise/toggle/123e4567-e89b-12d3-a456-426614174000', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    }
});

const result = await response.json();
console.log(result.isLiked ? '已按讚' : '已取消按讚');
```

### 獲取按讚狀態
```http
GET /api/praise/status/{articleId}
```

#### 回應格式
```json
{
  "success": true,
  "isLiked": false,
  "totalCount": 42,
  "articleId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### 批量按讚操作
```http
POST /api/praise/batch
```

#### 請求格式
```json
{
  "articleIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "987fcdeb-51a2-43d7-8765-123456789abc"
  ]
}
```

#### 回應格式
```json
{
  "success": true,
  "results": {
    "123e4567-e89b-12d3-a456-426614174000": {
      "success": true,
      "action": "praise",
      "newCount": 43
    }
  },
  "totalProcessed": 2,
  "successCount": 2
}
```

### 我的按讚列表
```http
GET /api/praise/my-praises?page=1&pageSize=20
```

## 📌 收藏功能 API

### 切換收藏狀態
```http
POST /api/collects/toggle/{articleId}
```

#### 回應格式
```json
{
  "success": true,
  "isCollected": true,
  "newCount": 15,
  "message": "已收藏"
}
```

### 獲取收藏狀態
```http
GET /api/collects/status/{articleId}
```

#### 回應格式
```json
{
  "success": true,
  "isCollected": true,
  "totalCount": 15,
  "articleId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### 批量收藏操作
```http
POST /api/collects/batch
```

### 我的收藏列表
```http
GET /api/collects/my-collections?page=1&pageSize=20
```

### 熱門收藏文章
```http
GET /api/collects/popular?count=10
```

#### 回應格式
```json
{
  "success": true,
  "data": [
    {
      "articleId": "123e4567-e89b-12d3-a456-426614174000",
      "title": "這是一篇很棒的文章...",
      "collectCount": 156,
      "praiseCount": 234,
      "authorId": "456e7890-a12b-34c5-6789-012345678def",
      "createTime": "2024-01-15T10:30:00Z"
    }
  ],
  "count": 10
}
```

## 👥 追蹤功能 API

### 切換追蹤狀態（新增）
```http
POST /api/follows/toggle/{targetId}
```

#### 參數
- `targetId` (Guid): 目標用戶 PersonId

#### 回應格式
```json
{
  "success": true,
  "isFollowing": true,
  "message": "已追蹤"
}
```

#### 使用範例
```javascript
const toggleFollow = async (targetId) => {
    try {
        const response = await fetch(`/api/follows/toggle/${targetId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            // 自動觸發 SignalR 更新
            console.log(result.message);
            return result.isFollowing;
        }
    } catch (error) {
        console.error('追蹤操作失敗:', error);
    }
};
```

### 原有端點（保持不變）
```http
POST /api/follows/{targetId}       # 追蹤用戶
DELETE /api/follows/{targetId}     # 取消追蹤
GET /api/follows/is-following?targetId={targetId}  # 檢查追蹤狀態
GET /api/follows                   # 我的追蹤列表
GET /api/follows/search?keyword={keyword}  # 搜尋用戶
```

## ⚡ SignalR 即時更新

### 觸發事件

#### 1. 互動更新事件
所有按讚/收藏操作會觸發：
```javascript
// 前端自動接收
window.matrixSignalR.on('interactionUpdate', (update) => {
    console.log('收到互動更新:', update);
    /*
    update = {
        articleId: "123e4567-e89b-12d3-a456-426614174000",
        action: "praise",  // 或 "collect"
        newCount: 43,
        targetUserId: "author-id"  // 可選，文章作者會收到個人通知
    }
    */
});
```

#### 2. 追蹤更新事件
追蹤操作會觸發：
```javascript
window.matrixSignalR.on('followUpdate', (update) => {
    console.log('收到追蹤更新:', update);
    /*
    update = {
        followerId: "follower-id",
        followedId: "followed-id", 
        isFollowing: true
    }
    */
});
```

#### 3. 個人通知事件
重要操作會發送個人通知：
```javascript
window.matrixSignalR.on('newNotification', (notification) => {
    console.log('收到新通知:', notification);
    /*
    notification = {
        type: "interaction",
        title: "有人按讚了您的文章",
        message: "您的文章獲得了新的按讚！",
        timestamp: "2024-01-15T10:30:00Z",
        actionUrl: "/article/123"
    }
    */
});
```

## 🔐 認證要求

### JWT Token
所有 API 都需要有效的 JWT Token：

```javascript
// Token 會從 Cookie 自動讀取，無需手動設定
// 如果收到 401 錯誤，表示需要重新登入
```

### 錯誤回應
```json
{
  "success": false,
  "message": "用戶未登入"
}
```

## 🚀 前端整合範例

### Vue.js 組件範例
```javascript
export default {
    data() {
        return {
            articleId: '123e4567-e89b-12d3-a456-426614174000',
            isLiked: false,
            praiseCount: 0,
            loading: false
        }
    },
    
    async mounted() {
        // 獲取初始狀態
        await this.loadPraiseStatus();
        
        // 監聽 SignalR 更新
        window.matrixSignalR.on('interactionUpdate', this.handleInteractionUpdate);
    },
    
    methods: {
        async loadPraiseStatus() {
            try {
                const response = await fetch(`/api/praise/status/${this.articleId}`);
                const result = await response.json();
                
                if (result.success) {
                    this.isLiked = result.isLiked;
                    this.praiseCount = result.totalCount;
                }
            } catch (error) {
                console.error('載入按讚狀態失敗:', error);
            }
        },
        
        async togglePraise() {
            if (this.loading) return;
            
            this.loading = true;
            try {
                const response = await fetch(`/api/praise/toggle/${this.articleId}`, {
                    method: 'POST'
                });
                
                const result = await response.json();
                
                if (result.success) {
                    // SignalR 會自動更新，但也可以立即更新 UI
                    this.isLiked = result.isLiked;
                    this.praiseCount = result.newCount;
                }
            } catch (error) {
                console.error('按讚操作失敗:', error);
            } finally {
                this.loading = false;
            }
        },
        
        handleInteractionUpdate(update) {
            // 只處理當前文章的更新
            if (update.articleId === this.articleId && update.action === 'praise') {
                this.praiseCount = update.newCount;
            }
        }
    },
    
    beforeUnmount() {
        // 清理事件監聽
        window.matrixSignalR.off('interactionUpdate', this.handleInteractionUpdate);
    }
}
```

### 原生 JavaScript 範例
```javascript
class PraiseButton {
    constructor(articleId, buttonElement) {
        this.articleId = articleId;
        this.button = buttonElement;
        this.init();
    }
    
    async init() {
        // 載入初始狀態
        await this.loadStatus();
        
        // 綁定點擊事件
        this.button.addEventListener('click', () => this.toggle());
        
        // 監聽 SignalR 更新
        window.matrixSignalR.on('interactionUpdate', (update) => {
            if (update.articleId === this.articleId && update.action === 'praise') {
                this.updateUI(update.newCount);
            }
        });
    }
    
    async loadStatus() {
        const response = await fetch(`/api/praise/status/${this.articleId}`);
        const result = await response.json();
        
        if (result.success) {
            this.updateButton(result.isLiked);
            this.updateCount(result.totalCount);
        }
    }
    
    async toggle() {
        const response = await fetch(`/api/praise/toggle/${this.articleId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            this.updateButton(result.isLiked);
            this.updateCount(result.newCount);
        }
    }
    
    updateButton(isLiked) {
        this.button.classList.toggle('liked', isLiked);
        this.button.textContent = isLiked ? '👍 已按讚' : '👍 按讚';
    }
    
    updateCount(count) {
        const countElement = this.button.querySelector('.count');
        if (countElement) {
            countElement.textContent = count;
        }
    }
}

// 使用範例
document.addEventListener('DOMContentLoaded', () => {
    const praiseButtons = document.querySelectorAll('[data-praise-button]');
    praiseButtons.forEach(button => {
        const articleId = button.dataset.articleId;
        new PraiseButton(articleId, button);
    });
});
```

## 📊 效能考量

### 批量操作
```javascript
// 避免短時間內大量單獨請求
const batchPraise = async (articleIds) => {
    const response = await fetch('/api/praise/batch', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ articleIds })
    });
    
    return await response.json();
};
```

### 防抖處理
```javascript
// 防止用戶快速點擊
const debouncedToggle = _.debounce(async (articleId) => {
    await fetch(`/api/praise/toggle/${articleId}`, { method: 'POST' });
}, 300);
```

## 🔍 故障排除

### 常見錯誤

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "用戶未登入"
}
```
**解決方案**: 檢查 JWT Token 是否有效，必要時重新登入。

#### 404 Not Found
```json
{
  "success": false,
  "message": "文章不存在"
}
```
**解決方案**: 確認 ArticleId 正確且文章存在。

#### 400 Bad Request
```json
{
  "success": false,
  "message": "無法追蹤自己"
}
```
**解決方案**: 檢查業務邏輯限制。

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "系統錯誤，請稍後重試"
}
```
**解決方案**: 檢查服務器日誌，可能是併發衝突或資料庫問題。