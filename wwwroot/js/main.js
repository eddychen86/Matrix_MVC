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
        const DatePicker = window.VueDatePicker
        const { ref, reactive, computed, watch, onMounted, onUnmounted } = Vue
        const { formatDate, timeAgo } = useFormatting()
        const isLoading = ref(false)
        const isAppReady = ref(false)


        //---------------------------Report----------------------------


        function formatDateValue(date) {
            const d = new Date(date)
            return d.toISOString().split('T')[0]  // yyyy-MM-dd 格式
        }

        function applyFilters() {
            page.value = 1
            loadReports()
        }

        // ✅ 點按狀態按鈕：再點一次同一顆=清除
        function setStatus(v) {
            const newVal = String(v);                 // 後端用 '0','1','2'
            status.value = (status.value === newVal) ? '' : newVal;
            page.value = 1;
            loadReports();
        }

        // ✅ 點按類型按鈕：再點一次同一顆=清除
        function setType(v) {
            const newVal = String(v);                 // 後端用 '0','1'
            type.value = (type.value === newVal) ? '' : newVal;
            page.value = 1;
            loadReports();
        }

        // ✅ 幫按鈕決定是否「被選中」
        const isStatusActive = v => status.value === String(v);
        const isTypeActive = v => type.value === String(v);

        

        //---------------------------Report----------------------------

        onMounted(() => {
            isAppReady.value = true

            const wrapper = document.getElementById('popup-wrapper')
            if (wrapper) wrapper.style.display = ''

            //REPORT----------------------------------------------

            const clearBtn = document.getElementById('btn-clear-date');
            if (clearBtn) {
                clearBtn.addEventListener('click', () => {
                    from.value = '';
                    to.value = '';
                    // 如果有 cally 的話，同步清空 UI
                    const callyEl = document.querySelector('calendar-date');
                    if (callyEl) {
                        callyEl.start = null;
                        callyEl.end = null;
                    }
                    loadReports(); // 重新載入資料
                })
            }

            // === Cally 單日：以 ModifyTime(ProcessTime) 篩選 ===
            window.setReportDate = (val) => {
                const v = String(val || '').trim()       // "YYYY-MM-DD"
                from.value = v                            // 單日 → from = to
                to.value = v
                page.value = 1
                loadReports()
                document.getElementById('popover-date')?.hidePopover?.()
            }
            window.clearReportDate = () => {
                from.value = null
                to.value = null
                page.value = 1
                loadReports()
                document.getElementById('popover-date')?.hidePopover?.()
            }
        })
        onUnmounted(() => {
            delete window.setReportDate
            delete window.clearReportDate
        })
        //REPORT----------------------------------------------


        //#region Pop-Up Events


        //#region Reports (BE / Dashboard)

        // 篩選 / 分頁
        const reports = ref([])
        const keyword = ref('')
        const status = ref('')      // '', 0=Pending, 1=Processed, 2=Rejected
        const type = ref('')      // '', 0=User, 1=Article
        const from = ref(null)      // yyyy-mm-dd
        const to = ref(null)      // yyyy-mm-dd
        const page = ref(1)
        const pageSize = ref(10)
        const total = ref(0)

        const totalPages = computed(() => Math.max(0, Math.ceil(total.value / pageSize.value)))


        // 🔽 新增：當 keyword 清空時，自動回復到未搜尋狀態
        watch(keyword, (val, oldVal) => {
            // 只有在「從有字 → 變成空」時才 reload，避免初始化時觸發
            if (oldVal !== undefined && oldVal.trim() !== '' && val.trim() === '') {
                page.value = 1
                loadReports()
            }
        })

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
            // from/to 可能是 null 或已是 yyyy-MM-dd 字串
            if (from.value) sp.append('from', typeof from.value === 'string' ? from.value : formatDateValue(from.value))
            if (to.value) sp.append('to', typeof to.value === 'string' ? to.value : formatDateValue(to.value))
            if (keyword.value.trim()) sp.append('keyword', keyword.value.trim())
            return sp.toString()
        }

        async function loadReports() {
            try {
                const url = `/api/dashboard/reports?${buildQuery()}`
                const res = await fetch(url)
                console.log('GET', url, '→', res.status)   // 👈 看看是 200/401/404
                if (!res.ok) return
                const data = await res.json()
                reports.value = data.items ?? []
                total.value = data.totalCount ?? 0
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
        } else {
            // 監聽 DOM，等 #reports-app 出現再載一次
            const mo = new MutationObserver((mutations, obs) => {
                if (document.getElementById('reports-app')) {
                    loadReports()
                    obs.disconnect()
                }
            })
            mo.observe(document.body, { childList: true, subtree: true })
        }
        

        //#endregion Reports








        // Popup State
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        const searchQuery = ref('')


        // Popup Data Storage
        const popupData = reactive({
            Search: {
                Users: [],
                Hashtags: []
            },
            Notify: [],
            Follows: [],
            Collects: []
        })


        watch(searchQuery, (newVal) => {
            console.log('👀 searchQuery 改變：', newVal)
        })

        // 當 openPopup 的類型是 Search 的時候，清空 searchQuery
        //watch(() => popupState.type, (newType) => {
        //    if (newType === 'Search') {
        //        searchQuery.value = ''
        //        popupData.Search = []
        //    }
        //})

        const manualSearch = async () => {
            console.log('🔍 手動搜尋按鈕觸發！', searchQuery)

            const keyword = searchQuery.value

            if (!keyword || keyword.trim().length < 1) {
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            isLoading.value = true
            try {
                const [userRes, tagRes] = await Promise.all([
                    fetch(`/api/search/users?keyword=${encodeURIComponent(keyword)}`),
                    fetch(`/api/search/hashtags?keyword=${encodeURIComponent(keyword)}`)
                ])

                const users = await userRes.json()
                const tags = await tagRes.json()


                popupData.Search.Users = users.data.map(user => ({
                    displayName: user.displayName,
                    avatarUrl: user.avatarPath,
                    bio: user.bio || '這位使用者尚未填寫個人簡介。'
                }))

                popupData.Search.Hashtags = tags.data
                console.log('🎯 搜尋結果資料：', popupData.Search)
            } catch (err) {
                console.error('Search API Error:', err)
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
            } finally {
                isLoading.value = false
            }
        }


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

            console.log('🧠 開啟 popup：', popupState.type)

            if (type === 'Search') {
                searchQuery.value = ''
                popupData.Search.Users = []
                popupData.Search.Hashtags = []
                return
            }

            isLoading.value = true   // 👈 加上這行：開始 loading

            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()

                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            } finally {
                isLoading.value = false
            }  // 👈 加上這行：結束 loading
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

        console.log('✅ setup() 成功初始化，searchQuery =', searchQuery.value)
        return {
            // pop-up
            popupState,
            popupData,
            isLoading, //加入 loading 狀態
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

            isAppReady,


            setStatus, setType, isStatusActive, isTypeActive,
            applyFilters,

            // hooks
            formatDate,
            timeAgo,
            searchQuery,
            manualSearch,
            // menu functions (spread from useMenu)
            ...menuFunctions,
        }
    }
})