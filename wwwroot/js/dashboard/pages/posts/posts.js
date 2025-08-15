const { ref, reactive, computed, watch } = Vue

window.mountPostsPage = function () {
  const app = createApp({
    setup() {
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

      // 預留：若未來有 /api/Db_PostsApi 可在此呼叫
      const init = async () => {
        try {
          // await fetch('/api/Db_PostsApi')
        } finally {
          isLoading.value = false
        }
      }

      // 狀態中文對照
      const statusText = (s) => (s === 0 ? '正常' : s === 1 ? '隱藏' : s === 2 ? '已刪除' : '未知')

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
          alert('讀取清單失敗')
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
        if (!confirm('確定要刪除這篇文章嗎?')) return
        const res = await fetch(`/api/posts/delete/${id}`, { method: 'DELETE' })
        if (res.ok) {
          loadArticles()
        } else {
          alert('刪除失敗')
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
          alert('狀態更新失敗')
        } finally {
          savingStatus.value = false
        }
      }

      // 取消編輯
      const cancelEdit = () => {
        editingId.value = null
      }

      Vue.onMounted(loadArticles)

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

      onMounted(() => init())

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

        // 行內狀態編輯
        editingId,
        editingStatus,
        savingStatus,
        startEdit,
        saveStatus,
        cancelEdit,

        // 刪除
        deleteArticle,
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
