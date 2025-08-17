const { createApp, ref, reactive, computed, watch, onMounted } = Vue

window.mountPostsPage = function () {
  const app = createApp({
    compilerOptions: {
      isCustomElement: (tag) => tag.includes('calendar-')
    },
    setup() {
      // --- i18n：語系來源（一定要在最前面） ---
      // 取語系：優先用 uic=，再用 c=，最後預設 zh-TW
      // 讀取 .AspNetCore.Culture，支援編碼與未編碼兩種格式
      function getCulture() {
        // 先把整個 cookie 字串裡，該名稱的值抓出來
        const m = document.cookie.match(/(?:^|;\s*)\.AspNetCore\.Culture=([^;]+)/);
        if (!m) return 'zh-TW';

        // 可能是 c%3Den-US%7Cuic%3Den-US，要先 decode
        const raw = decodeURIComponent(m[1]); // 例如 "c=en-US|uic=en-US"

        // 從 raw 再取出 c= 後面的文化碼
        const m2 = raw.match(/(?:^|[\s;])c=([^|;]+)/i);
        return m2 ? m2[1] : 'zh-TW';
      }

      const culture = ref(getCulture());   // reactive 語系
      const dictRef = ref({});             // reactive 翻譯字典

      // --- Debug/全域存取用：把 i18n 狀態掛到 window ---
      window.__i18nStatePosts = {
        culture,   // Vue ref，可在 console 用 __i18nStatePosts.culture.value 看到目前語系
        dictRef,   // Vue ref，可在 console 看字典鍵值
        t,         // 你的 t()，可以直接試 __i18nStatePosts.t('Posts_Status_Normal')
        reload: () => loadTranslationsAndApply() // 需要時可強制重載字典
      };

      // 模板/函式通用的 t()：讀 culture.value 來建立依賴
      function t(key) {
        const _ = culture.value;                  // 這行讓使用 t() 的地方會跟著語系變動
        return (dictRef.value?.[key] ?? key);
      }

      const isLoading = ref(true)

      // posts後臺顯示資料
      const page = ref(1)
      const pageSize = ref(10)
      const totalPages = ref(0)
      const articles = ref([])
      const keyword = ref('')
      const filterStatus = ref(-1)
      const filterDate = ref('')

      // 狀態編輯用
      const editingId = ref(null)
      const editingStatus = ref(null)
      const savingStatus = ref(false)

      // 狀態多語系對照
      const statusText = computed(() => {
        const isEn = (culture.value || '').toLowerCase().startsWith('en')
        const d = dictRef.value || {}
        const fb = isEn
          ? { 0: 'Normal', 1: 'Hidden', 2: 'Deleted' }
          : { 0: '正常', 1: '隱藏', 2: '已刪除' }

        return (value) => ({
          0: d['Posts_Status_Normal'] || fb[0],
          1: d['Posts_Status_Hidden'] || fb[1],
          2: d['Posts_Status_Deleted'] || fb[2],
        }[value] ?? (d['Unknown'] || '未知'))
      })

      const formatDate = (datetime) => {
        const date = new Date(datetime)
        return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
      }

      const findArticle = (id) => articles.value.find(a => a.articleId === id) || null

      function toggleDate(e) {
        const val = e.target.value
        if (filterDate.value === val) {
          filterDate.value = ''
          e.target.value = ''
        } else {
          filterDate.value = val
        }
        page.value = 1
        loadArticles()
      }

      function applyFilters() {
        page.value = 1
        loadArticles()
      }

      const loadArticles = async () => {
        let url = `/api/posts/list?page=${page.value}&pagesize=${pageSize.value}`
        if (keyword.value) url += `&keyword=${encodeURIComponent(keyword.value)}`
        if (filterStatus.value !== -1) url += `&status=${filterStatus.value}`
        if (filterDate.value) url += `&date=${encodeURIComponent(filterDate.value)}` // 新增日期條件

        const res = await fetch(url)
        if (!res.ok) {
          const errorMsg = t('Posts_LoadError') || '讀取清單失敗'
          alert(errorMsg)
          return
        }
        const data = await res.json()
        let items = data.items || []
        if (filterStatus.value !== -1) {
          items = items.filter(a => Number(a.status) === Number(filterStatus.value))
        }
        articles.value = items
        totalPages.value = data.totalPages || 0
      }

      // 文章刪除功能
      const deleteArticle = async (id) => {
        const confirmMsg = t('Posts_ConfirmDelete') || '確定要刪除這篇文章嗎?'
        if (!confirm(confirmMsg)) return
        const res = await fetch(`/api/posts/delete/${id}`, { method: 'DELETE' })
        if (res.ok) {
          loadArticles()
        } else {
          const errorMsg = t('Posts_DeleteError') || '刪除失敗'
          alert(errorMsg)
        }
      }

      // 點 Edit：進入該列的狀態編輯
      const startEdit = (article) => {
        editingId.value = article.articleId
        editingStatus.value = article.status ?? 0 // 0: 正常, 1: 隱藏
      }

      // 儲存狀態
      const saveStatus = async (article) => {
        try {
          savingStatus.value = true

          const res = await fetch(`/api/posts/status/${article.articleId}`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: Number(editingStatus.value) })
          })
          if (!res.ok) throw new Error('更新失敗')

          // 前端就地更新
          article.status = Number(editingStatus.value)
          article.statusText = null // 讓 {{ item.statusText || statusText(item.status) }} 重新顯示
          article.modifyTime = new Date().toISOString()

          editingId.value = null
        } catch (e) {
          const errorMsg = t('Posts_UpdateError') || '狀態更新失敗'
          alert(errorMsg)
        } finally {
          savingStatus.value = false
        }
      }

      // 取消編輯
      const cancelEdit = () => {
        editingId.value = null
      }

      // 管理表格換頁功能
      const goPage = async (p) => {
        const newPage = Math.max(1, Math.min(totalPages.value, p))
        if (newPage === page.value) return
        page.value = newPage
        await loadArticles()
      }

      // 頁面簡化顯示功能
      const showPage = computed(() => {
        const result = []
        const total = totalPages.value
        const cur = page.value
        if (total <= 6) {
          for (let i = 1; i <= total; i++) result.push(i)
        } else {
          result.push(1)
          if (cur > 4) result.push('...')
          for (let i = Math.max(2, cur - 2); i <= Math.min(total - 1, cur + 2); i++) {
            result.push(i)
          }
          if (cur < total - 3) result.push('...')
          result.push(total)
        }
        return result
      })

      const search = async () => {
        page.value = 1
        await loadArticles()
      }

      // 多語系載入功能
      function applyI18n(dict) {
        if (!dict) return;

        document.querySelectorAll("[data-i18n]").forEach(el => {
          const key = el.getAttribute("data-i18n");
          if (dict[key]) el.textContent = dict[key];
        });

        document.querySelectorAll("[data-i18n-placeholder]").forEach(el => {
          const key = el.getAttribute("data-i18n-placeholder");
          if (dict[key]) el.setAttribute("placeholder", dict[key]);
        });

        document.querySelectorAll("[data-i18n-title]").forEach(el => {
          const key = el.getAttribute("data-i18n-title");
          if (dict[key]) el.setAttribute("title", dict[key]);
        });
      }

      async function loadTranslationsAndApply() {
        const c = getCulture();
        try {
          const res = await fetch(`/api/Translation/${c}`, {
            headers: { "Accept": "application/json" },
            cache: "no-store"
          });
          if (!res.ok) throw new Error(`i18n http ${res.status}`);
          const dict = await res.json();

          // 關鍵：更新 reactive 狀態
          dictRef.value = dict;
          culture.value = c;

          // 仍然幫頁面上 data-i18n 的靜態節點套字
          applyI18n(dict);
        } catch (e) {
          console.warn("Load translations failed:", e);
        }
      }

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫

          // 先載入翻譯再載入資料，確保初始畫面就有正確文字
          await loadTranslationsAndApply();
          await loadArticles();

          // 監看 cookie 的語系是否改變，有改就重載字典
          let last = culture.value;
          setInterval(() => {
            const cur = getCulture();
            if (cur !== last) {
              last = cur;
              loadTranslationsAndApply(); // 更新 dictRef / culture，表格就會重算
            }
          }, 500); // 0.5 秒偵測一次
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init());

      return {
        isLoading,

        // 列表 & 分頁
        articles,
        loadArticles,
        page,
        totalPages,
        goPage,
        showPage,
        keyword,
        search,
        filterDate,
        toggleDate,

        // 顯示/狀態文字
        statusText,
        filterStatus,
        applyFilters,
        formatDate,

        // 行內狀態編輯
        editingId,
        editingStatus,
        savingStatus,
        startEdit,
        saveStatus,
        cancelEdit,

        // 刪除
        deleteArticle,

        // 多語系
        t,
      }
    }
  })

  if (window.PostsApp && typeof window.PostsApp.unmount === 'function') {
    try { window.PostsApp.unmount() } catch (_) { }
  }
  const el = document.querySelector('#adminPosts')
  if (el) window.PostsApp = app.mount(el)
}

if (document.querySelector('#adminPosts')) {
  window.mountPostsPage()
}
