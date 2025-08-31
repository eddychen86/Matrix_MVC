# Web API + Vue 前端整合（超新手友善）

本文件教你把前一份 Web API（/api/todos）接到一個最簡單的 Vue 介面上，支援清單顯示與新增，並處理載入與錯誤。

---

## 1. 前置準備
- 已有可執行的 API（見 Backend/webapi-from-zero.md）
  - 範例端點：GET/POST `http://localhost:5087/api/todos`
- API 已啟用 CORS（AllowAnyOrigin/Method/Header），否則不同來源呼叫會被擋

---

## 2. 最快上手：用 CDN 建一個簡單頁面

建立 `index.html`，內容如下，直接用 Vue 3 + fetch 呼叫 API：
```
<!DOCTYPE html>
<html lang="zh-Hant">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Todos</title>
  <style>
    body { font-family: system-ui, sans-serif; max-width: 720px; margin: 32px auto; }
    input, button { padding: 8px; }
    .error { color: #b00020; }
  </style>
</head>
<body>
  <div id="app"></div>

  <script type="importmap">
  {
    "imports": {
      "vue": "https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js"
    }
  }
  </script>

  <script type="module">
    import { createApp, ref, onMounted } from 'vue'

    const API = 'http://localhost:5087/api/todos'

    createApp({
      setup() {
        const todos = ref([])
        const title = ref('')
        const loading = ref(false)
        const error = ref('')

        const load = async () => {
          loading.value = true; error.value = ''
          try {
            const res = await fetch(API)
            if (!res.ok) throw new Error('讀取失敗: ' + res.status)
            todos.value = await res.json()
          } catch (e) {
            error.value = e.message
          } finally {
            loading.value = false
          }
        }

        const add = async () => {
          if (!title.value.trim()) return
          loading.value = true; error.value = ''
          try {
            const res = await fetch(API, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ title: title.value, isDone: false })
            })
            if (!res.ok) throw new Error('新增失敗: ' + res.status)
            title.value = ''
            await load()
          } catch (e) {
            error.value = e.message
          } finally {
            loading.value = false
          }
        }

        onMounted(load)
        return { todos, title, loading, error, load, add }
      },
      template: `
        <h1>我的待辦清單</h1>
        <p v-if=\"error\" class=\"error\">{{ error }}</p>
        <p v-else-if=\"loading\">載入中…</p>

        <form @submit.prevent=\"add\" style=\"margin: 12px 0;\">
          <input v-model=\"title\" placeholder=\"輸入待辦內容\" />
          <button :disabled=\"!title.trim() || loading\">新增</button>
        </form>

        <ul>
          <li v-for=\"t in todos\" :key=\"t.id\">{{ t.title }}</li>
        </ul>
      `
    }).mount('#app')
  </script>
</body>
</html>
```

直接雙擊開啟或用簡易伺服器（例如 VS Code Live Server 擴充）即可測試。

---

## 3. 與 ASP.NET Core 整合（放在 wwwroot）
- 在 `wwwroot/` 建 `app/todos.js`，貼上上面 `<script type="module">` 內的程式碼（把 `createApp(...)` 那段放入）
- 在 View（例如 `Views/Home/Index.cshtml`）加入：
```
<div id="app"></div>
<script type="module" src="~/app/todos.js"></script>
```
- 若 API 與頁面同源（同一 ASP.NET Core 專案），把 `API` 改為相對路徑：`const API = '/api/todos'`

---

## 4. axios 版本（可選）
```
<script src="https://cdn.jsdelivr.net/npm/axios@1.7.7/dist/axios.min.js"></script>
<script type="module">
  import { createApp, ref, onMounted } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js'
  const API = 'http://localhost:5087/api/todos'
  // 將 fetch 呼叫改為 axios.get/post 即可
</script>
```

---

## 5. 常見錯誤（超新手排查）
- CORS 錯誤：請確認 API 有 `AllowAnyOrigin/Method/Header` 或設定允許的前端網址
- 404：請確認 API 路徑正確（/api/todos）
- 415 Unsupported Media Type：POST 記得加 `Content-Type: application/json`
- 混合內容（Mixed Content）：HTTPS 網站呼叫 HTTP API 會被擋，請一致使用 HTTPS 或在本機同源開發

---

## 6. 下一步
- 新增刪除/完成狀態（PUT/DELETE）
- 表單驗證與錯誤提示
- 把 API URL 抽成設定檔，依環境切換
- 用元件化把清單項目抽成子元件

恭喜！你已完成 Web API 與 Vue 的最小整合！
