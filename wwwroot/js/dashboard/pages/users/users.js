const { createApp, ref, computed, onMounted } = Vue

// 供動態載入後初始化掛載
window.mountUsersPage = function() {
  const app = createApp({
    compilerOptions: {
        isCustomElement: (tag) => tag.includes('calendar-')
    },
    setup() {
        const isLoading = ref(true)
        const createDateInput = ref("")
        
        //名字篩選用
        const nameFilterInput = ref("")  //輸入框暫存
        const nameFilter = ref("")   //真正篩選用

        //搜尋方法
        const applyNameFilter = () =>{
            nameFilter.value = nameFilterInput.value.trim()
            pageIndex.value = 1
        }

        const users = ref([])
        const pageIndex = ref(1)
        const pageSize = ref(10)

        const editingId = ref(null)
        const tempStatus = ref(0)
        const updating = ref(false)

        //Ban
        const statusFilter = ref(null)  // 1=Enable, 0=Disable
        const bannedFilter = ref(null)  // true=yes, false=no
        // 篩選 + 分頁：把所有條件串在一起，最後一次性 return
        const filteredUsers = computed(() => {
            let arr = users.value.slice()

            // 1) 名稱
            if (nameFilter.value) {
                const q = nameFilter.value.toLowerCase()
                arr = arr.filter(u => (u.userName ?? '').toLowerCase().includes(q))
            }

            // 2) Status (0/1)
            if (statusFilter.value !== null) {
                arr = arr.filter(u => u.status === statusFilter.value)
            }

            // 3) Ban (status===2)
            if (bannedFilter.value !== null) {
                arr = bannedFilter.value
                    ? arr.filter(u => u.status === 2)   // Yes → 被封禁
                    : arr.filter(u => u.status !== 2)   // No  → 非封禁
            }
            return arr
        })

        const totalPages = computed(() => Math.max(1, Math.ceil(filteredUsers.value.length / pageSize.value)))
        const pagedUsers = computed(() => {
            const start = (pageIndex.value - 1) * pageSize.value
            return filteredUsers.value.slice(start, start + pageSize.value)
        })

        const statusText = (value) =>({
            0: "未啟用",
            1: "已啟用",
            2: "被封禁"
        }[value] ?? value)

        const formatDate = (datetime) => {
            const date = new Date(datetime)
            return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
        }
        

        //Status change
        function toggleStatus(v){
            statusFilter.value = (statusFilter.value === v) ? null : v
            bannedFilter.value = null
            pageIndex.value = 1
        }
        //Ban change
        function toggleBanned(v){
            bannedFilter.value = (bannedFilter.value === v) ? null : v
            statusFilter.value = null
            pageIndex.value = 1
        }

        const selectedDate = ref(null)
        const lastYmd = ref(null)

        //轉YYYY-MM-DD字串給後端
        function formatYMD(dt)
        {
            const d = new Date(dt)
            const y = d.getFullYear()
            const m = String(d.getMonth()+1).padStart(2, '0')
            const day = String(d.getDate()).padStart(2, '0')
            return `${y}-${m}-${day}`
        }

        const fetchUsers = async (dateYmd = null) => {
            try {
                isLoading.value = true
                const qs = dateYmd ? `?createDate=${dateYmd}` : ''
                // 如果欲中止支援，傳入可被導航中止的 signal（可選）
                const init = {}
                if (window && window.__navAbortController) {
                    init.signal = window.__navAbortController.signal
                }
                const response = await fetch(`/api/Db_UsersApi${qs}`, init)
                if (!response.ok) throw new Error('API 錯誤')
                const data = await response.json()
                users.value = data
                pageIndex.value = 1
            }catch (error) {
                if (error && (error.name === 'AbortError' || String(error).includes('AbortError'))) {
                    console.warn('使用者清單請求已中止')
                } else {
                    console.error('載入失敗：', error)
                }
            } finally {
                isLoading.value = false
            }
        }
        //月曆 (跟輸入框同步)

        const onCalendarChange = (e) =>{
            const raw = e?.detail?.value ?? e?.target?.value
            if (!raw){
                createDateInput.value = ""
                selectedDate.value = null
                lastYmd.value = null
                fetchUsers(null)
                return
            }
            const ymd = formatYMD(new Date(raw))
            if(lastYmd.value === ymd){
                //點第二次同一天,取消篩選
                createDateInput.value = null
                selectedDate.value = null
                lastYmd.value = null
                fetchUsers(null)
            }else{
                //第一天或換一天,設定篩選
                selectedDate.value = new Date(raw)
                lastYmd.value = ymd
                createDateInput.value = ymd //更新輸入框
                fetchUsers(ymd)
            }
        }

        const deleteUser = (id) => {
            if(!confirm("你確定要刪除嗎?要不要再考慮一下"))return;

            fetch(`/api/Db_UsersApi/${id}`,{
                method: 'DELETE'
            })
            .then(response => {
                if(!response.ok)throw new Error("刪除失敗囉")
                users.value = users.value.filter(u => u.userId !== id)
            })
            .catch(err =>{
                console.log("刪除錯誤!!");
            });

        }

        const startEdit = (item) => {
            editingId.value = item.userId
            tempStatus.value = item.status
        }

        const cancelEdit = () => {
            editingId.value = null
            tempStatus.value = 0
        }

        const confirmEdit = async (item) => {
            if(updating.value) return
            updating.value = true
            try {
                // API：PUT 更改status狀態
                const res = await fetch(`/api/Db_UsersApi/${item.userId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ status: tempStatus.value })
                })
                if(!res.ok){
                    const text = await res.text()
                    throw Error("更新失敗")
                }

                const index = users.value.findIndex(u => u.userId === item.userId)
                if(index > -1)users.value[index].status = tempStatus.value

                cancelEdit()
            }
            catch(err)
            {
                console.error("更新錯誤",err)
                alert("更新失敗:" + err.message)
            }
            finally{
                updating.value = false
            }

        }

        //日期篩選 (手動or按月曆)
        const applyCreateDateFilter = () =>{
            let val = createDateInput.value.trim()
            if(!val){
                //清除篩選
                selectedDate.value = null
                lastYmd.value = null
                fetchUsers(null)
                return
            }
        
            //驗證日期
            const dateObj = new Date(val)
            if(isNaN(dateObj)){
                alert("日期格式錯誤!")
                return
            }
            //更新月曆與篩選
                selectedDate.value = dateObj
                lastYmd.value = formatYMD(dateObj)
                fetchUsers(lastYmd.value)
        }

        onMounted(() => fetchUsers(null))

        return {
            isLoading,
            users,
            pageIndex,
            pageSize,
            totalPages,
            pagedUsers,
            formatDate,
            deleteUser,
            editingId,
            tempStatus,
            updating,
            statusText,
            startEdit,
            cancelEdit,
            confirmEdit,
            onCalendarChange,
            selectedDate,
            formatYMD,
            statusFilter,
            bannedFilter,
            toggleStatus,
            toggleBanned,
            nameFilterInput,
            nameFilter,
            applyNameFilter,
            createDateInput,
            applyCreateDateFilter
        }
    }
  })

  // 若已存在舊的 App，先解除掛載
  if (window.DashboardUsersApp && typeof window.DashboardUsersApp.unmount === 'function') {
    try { window.DashboardUsersApp.unmount() } catch (_) {}
  }

  if (document.querySelector('#adminUser')) {
    window.DashboardUsersApp = app.mount('#adminUser')
  }
}

// 首次進入完整頁面時（非 AJAX）也能自動掛載
if (document.querySelector('#adminUser') && !window.__PARTIAL_LOADING) {
  window.mountUsersPage()
}
