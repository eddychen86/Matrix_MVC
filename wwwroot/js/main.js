const globalApp = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue not ready, retrying...')
        return
    } else {
        lucide.createIcons()
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => window.globalApp = Vue.createApp(content).mount('#app'))
        } else {
            // DOM 已經載入完成
            window.globalApp = Vue.createApp(content).mount('#app')
        }
    }
}

globalApp({
    setup() {
        const { ref, reactive, computed, watch } = Vue
        const { formatDate, timeAgo } = useFormatting()

        //#region Pop-Up Events

        // Popup State
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        // Popup Data Storage
        const popupData = reactive({
            Search: [],
            Notify: [],
            Follows: [],
            Collects: []
        })

        // popup helper
        const getPopupTitle = type => {
            const titles = {
                'Search': '搜尋',
                'Notify': '通知',
                'Follows': '追蹤',
                'Collects': '收藏'
            }

            return titles[type] || '視窗'
        }

        // Update popup data
        const updatePopupData = (type, data) => {
            if (popupData[type] !== undefined) popupData[type] = data
        }

        // Popup click
        const openPopup = async type => {
            popupState.type = type
            popupState.title = getPopupTitle(type)
            popupState.isVisible = true

            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()

                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            }
        }

        const closePopup = () => {
            popupState.isVisible = false
            popupState.type = ''
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

        //#endregion

        //#region Menu Integration

        // Import menu functionality
        const menuFunctions = useMenu()

        //#endregion

        // posts後臺顯示資料
        const page = ref(1)
        const pageSize = ref(10)
        const totalPages = ref(0)
        const articles = ref([])
        const keyword = ref('')

        const expandedId = ref(null)
        const editingId = ref(null)
        const editContent = ref('')

        // 目前選取的文章
        const selectedId = ref(null)
        const statusEditor = ref(null)

        const findArticle = (id) => articles.value.find(a => a.articleId === id) || null

        const loadArticles = async () => {
            let url = `/api/posts/list?page=${page.value}&pagesize=${pageSize.value}`
            if (keyword.value) url += `&keyword=${encodeURIComponent(keyword.value)}`

            const res = await fetch(url)
            const data = await res.json()
            articles.value = data.items
            totalPages.value = data.totalPages

            // 載入後若仍有選取，讓左側開關跟新資料同步
            if (selectedId.value) {
                const a = findArticle(selectedId.value)
                statusEditor.value = a ? (a.status === 1 ? 1 : 0) : null
            }
        }

        // 狀態中文對照
        const statusText = (s) => (s === 0 ? '正常' : s === 1 ? '隱藏' : s === 2 ? '已刪除' : '未知')

        // 展開以選取列完整內文
        const toggleExpand = (id) => {
            if (expandedId.value === id) {
                expandedId.value = null
                editingId.value = null
                selectedId.value = null
                statusEditor.value = null
                return
            }
            editingId.value = null
            expandedId.value = id

            // 同步左側開關
            selectedId.value = id
            const a = findArticle(id)
            statusEditor.value = a ? (a.status === 1 ? 1 : 0) : null
        }

        // 開始編輯文章
        const startEdit = (article) => {
            expandedId.value = article.articleId
            editingId.value = article.articleId
            editContent.value = article.content || ''

            selectedId.value = article.articleId
            statusEditor.value = article.status === 1 ? 1 : 0
        }

        // 取消編輯
        const cancelEdit = () => {
            editingId.value = null
            expandedId.value = null
        }

        // 儲存文章內容
        const saveEdit = async (id) => {
            const res = await fetch(`/api/posts/update/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ content: editContent.value })
            })
            if (res.ok) {
                await loadArticles()
                editingId.value = null
                expandedId.value = null
            } else {
                alert('更新失敗')
            }
        }

        // 左側開關
        const onChangeStatus = async () => {
            const id = selectedId.value
            if (!id || statusEditor.value == null) return

            const a = findArticle(id)
            if (!a) return

            const newStatus = statusEditor.value
            const prevStatus = a.status
            if (newStatus === prevStatus) return

            try {
                // 用你現有的更新 API；若你的後端要別的路由，改這裡即可
                const res = await fetch(`/api/posts/status/${selectedId.value}`, {
                    method: 'PATCH',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ status: statusEditor.value })
                })
                if (!res.ok) throw new Error()

                a.status = newStatus
                a.statusText = newStatus === 0 ? '正常' : '隱藏'
            } catch {
                alert('狀態更新失敗')
                statusEditor.value = prevStatus
            }
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

        return {
            // pop-up
            popupState,
            popupData,
            getPopupTitle,
            openPopup,
            closePopup,
            // 為新版 popup 提供向後兼容
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            // hooks
            formatDate,
            timeAgo,

            // menu functions (spread from useMenu)
            ...menuFunctions,

            // 列表 & 分頁
            articles,
            loadArticles,
            page,
            totalPages,
            goPage,
            showPage,
            keyword,
            search,

            // 顯示/狀態文字
            statusText,

            // 選取 + 左側開關
            selectedId,
            statusEditor,
            onChangeStatus,

            // 展開/編輯/刪除
            expandedId,
            editingId,
            editContent,
            toggleExpand,
            startEdit,
            cancelEdit,
            saveEdit,
            deleteArticle,
        }


    }
})