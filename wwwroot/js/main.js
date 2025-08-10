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
        const { ref, reactive, computed, } = Vue
        const { formatDate, timeAgo } = useFormatting()

        //#region Pop-Up Events


        //#region Reports (BE / Dashboard)

        // 篩選 / 分頁
        const reports = ref([])
        const keyword = ref('')
        const status = ref('')      // '', 0=Pending, 1=Processed, 2=Rejected
        const type = ref('')      // '', 0=User, 1=Article
        const from = ref('')      // yyyy-mm-dd
        const to = ref('')      // yyyy-mm-dd
        const page = ref(1)
        const pageSize = ref(10)
        const total = ref(0)

        const totalPages = computed(() => Math.max(0, Math.ceil(total.value / pageSize.value)))

        // 分頁顯示陣列（和你組員頁面一致的「…」風格）
        const showPage = computed(() => {
            const tp = totalPages.value
            const p = page.value
            if (tp <= 7) return Array.from({ length: tp }, (_, i) => i + 1)

            const arr = [1]
            if (p > 3) arr.push('...')
            for (let i = p - 1; i <= p + 1; i++) {
                if (i > 1 && i < tp) arr.push(i)
            }
            if (p < tp - 2) arr.push('...')
            arr.push(tp)
            return arr
        })

        const buildQuery = () => {
            const sp = new URLSearchParams({ page: page.value, pageSize: pageSize.value })
            if (status.value !== '') sp.append('status', status.value)
            if (type.value !== '') sp.append('type', type.value)
            if (from.value) sp.append('from', from.value)
            if (to.value) sp.append('to', to.value)
            if (keyword.value.trim()) sp.append('keyword', keyword.value.trim())
            return sp.toString()
        }

        async function loadReports() {
            try {
                const res = await fetch(`/api/dashboard/reports?${buildQuery()}`)
                if (!res.ok) return
                const data = await res.json()
                reports.value = data.items ?? []
                total.value = data.totalCount ?? 0
                // 讓 lucide 圖示重繪（若頁面上有 icon）
                if (window.lucide) setTimeout(() => window.lucide.createIcons(), 0)
            } catch (e) {
                console.error('loadReports error', e)
            }
        }

        function search() {
            page.value = 1
            loadReports()
        }

        function goPage(p) {
            if (typeof p !== 'number') return
            if (p < 1 || p > totalPages.value) return
            page.value = p
            loadReports()
        }

        async function processReport(id) {
            const res = await fetch(`/api/dashboard/reports/${id}/process`, { method: 'POST' })
            if (res.ok) loadReports()
        }

        async function rejectReport(id) {
            const res = await fetch(`/api/dashboard/reports/${id}/reject`, { method: 'POST' })
            if (res.ok) loadReports()
        }

        // 如果目前頁面存在 Reports 的容器，就自動載入（避免其它頁面多呼叫）
        if (document.getElementById('reports-app')) {
            loadReports()
        }

        //#endregion Reports








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

            reports, keyword, status, type, from, to,
            page, pageSize, totalPages, showPage,
            loadReports, search, goPage,
            processReport, rejectReport,

            // hooks
            formatDate,
            timeAgo,

            // menu functions (spread from useMenu)
            ...menuFunctions,
        }
    }
})