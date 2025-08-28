const { createApp, ref, computed, onMounted } = Vue

// 供動態載入後初始化掛載
window.mountUsersPage = function () {
    const app = createApp({
        compilerOptions: {
            isCustomElement: (tag) => tag.includes('calendar-')
        },
        setup() {
            const isLoading = ref(true)


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
            window.__i18nState = {
                culture,   // Vue ref，可在 console 用 __i18nState.culture.value 看到目前語系
                dictRef,   // Vue ref，可在 console 看字典鍵值
                t,         // 你的 t()，可以直接試 __i18nState.t('Status_Enabled')
                reload: () => loadTranslationsAndApply() // 需要時可強制重載字典
            };



            // 模板/函式通用的 t()：讀 culture.value 來建立依賴
            function t(key) {
                const _ = culture.value;                  // 這行讓使用 t() 的地方會跟著語系變動
                return (dictRef.value?.[key] ?? key);
            }
            //以上是多語系
            const createDateInput = ref("")

            //名字篩選用
            const nameFilterInput = ref("")  //輸入框暫存
            const nameFilter = ref("")   //真正篩選用

            //搜尋方法
            const applyNameFilter = () => {
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
            //多語系
            const statusText = computed(() => {
                // const isEn = (culture.value || '').toLowerCase().startsWith('en')
                // const d = dictRef.value || {}
                // const fb = isEn
                //     ? { 0: 'Disabled', 1: 'Enabled', 2: 'Banned' }
                //     : { 0: '未啟用', 1: '已啟用', 2: '被封禁' }

                return (value) => ({
                    0: 'Users_Status_Disabled',
                    1: 'Users_Status_Enabled',
                    2: 'Users_Status_Banned',
                }[value] ?? value)
            })
            //以上多語系
            const formatDate = (datetime) => {
                const date = new Date(datetime)
                return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
            }


            //Status change
            function toggleStatus(v) {
                statusFilter.value = (statusFilter.value === v) ? null : v
                bannedFilter.value = null
                pageIndex.value = 1
            }
            //Ban change
            function toggleBanned(v) {
                bannedFilter.value = (bannedFilter.value === v) ? null : v
                statusFilter.value = null
                pageIndex.value = 1
            }

            const selectedDate = ref(null)
            const lastYmd = ref(null)

            //轉YYYY-MM-DD字串給後端
            function formatYMD(dt) {
                const d = new Date(dt)
                const y = d.getFullYear()
                const m = String(d.getMonth() + 1).padStart(2, '0')
                const day = String(d.getDate()).padStart(2, '0')
                return `${y}-${m}-${day}`
            }

            const fetchUsers = async (dateYmd = null) => {
                try {
                    const qs = dateYmd ? `?createDate=${dateYmd}` : ''
                    const response = await fetch(`/api/DB_Users${qs}`)
                    if (!response.ok) throw new Error('API 錯誤')
                    const data = await response.json()
                    users.value = data
                    pageIndex.value = 1
                } catch (error) {
                    console.error('載入失敗：', error)
                }
            }
            //月曆 (跟輸入框同步)

            const onCalendarChange = (e) => {
                const raw = e?.detail?.value ?? e?.target?.value
                if (!raw) {
                    createDateInput.value = ""
                    selectedDate.value = null
                    lastYmd.value = null
                    fetchUsers(null)
                    return
                }
                const ymd = formatYMD(new Date(raw))
                if (lastYmd.value === ymd) {
                    //點第二次同一天,取消篩選
                    createDateInput.value = null
                    selectedDate.value = null
                    lastYmd.value = null
                    fetchUsers(null)
                }
                else {
                    //第一天或換一天,設定篩選
                    selectedDate.value = new Date(raw)
                    lastYmd.value = ymd
                    createDateInput.value = ymd //更新輸入框
                    fetchUsers(ymd)
                }
            }

            const deleteUser = (id) => {
                if (!confirm("你確定要刪除嗎?要不要再考慮一下")) return;

                fetch(`/api/DB_Users/${id}`, {
                    method: 'DELETE'
                })
                    .then(response => {
                        if (!response.ok) throw new Error("刪除失敗囉")
                        users.value = users.value.filter(u => u.userId !== id)
                    })
                    .catch(err => {
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
                if (updating.value) return
                updating.value = true
                try {
                    // API：PUT 更改status狀態
                    const res = await fetch(`/api/DB_Users/${item.userId}`, {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ status: tempStatus.value })
                    })
                    if (!res.ok) {
                        const text = await res.text()
                        throw Error("更新失敗")
                    }

                    const index = users.value.findIndex(u => u.userId === item.userId)
                    if (index > -1) users.value[index].status = tempStatus.value

                    cancelEdit()
                }
                catch (err) {
                    console.error("更新錯誤", err)
                    alert("更新失敗:" + err.message)
                }
                finally {
                    updating.value = false
                }

            }

            //日期篩選 (手動or按月曆)
            const applyCreateDateFilter = () => {
                let val = createDateInput.value.trim()
                if (!val) {
                    //清除篩選
                    selectedDate.value = null
                    lastYmd.value = null
                    fetchUsers(null)
                    return
                }

                //驗證日期
                const dateObj = new Date(val)
                if (isNaN(dateObj)) {
                    alert("日期格式錯誤!")
                    return
                }
                //更新月曆與篩選
                selectedDate.value = dateObj
                lastYmd.value = formatYMD(dateObj)
                fetchUsers(lastYmd.value)
            }
            //多語系
            const ROLE_KEY_BY_RAW = {
                '管理員': 'Role_Admin',
                '一般使用者': 'Role_User',
                'Admin': 'Role_Admin',
                'User': 'Role_User',
            }

            const roleTextI18n = computed(() => {
                const isEn = (culture.value || '').toLowerCase().startsWith('en')
                const d = dictRef.value || {}

                // 語意化 Key → 保底文字（字典缺鍵時用）
                return (item) => {
                    const raw = (item.roleText || '').trim()
                    const key = ROLE_KEY_BY_RAW[raw]
                    if (!key) return raw // 沒對到就顯示原字

                    // 先用後端字典，沒有就用保底
                    return d[key]
                }
            })




            // 3) 把翻譯套進 DOM（支援 text、placeholder、title）
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

            // 4) 從後端載入翻譯（不改後端：呼叫 /api/Translation/{culture}）
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
            // window.reloadTranslations = loadTranslationsAndApply; 外部切語言時呼叫

            // 5) Vue 內用的 t()：動態字串（例如 statusText）用這個
            // function t(key) {
            //     const _ = culture.value;    建立依賴
            //     return (window.__i18n?.dict?.[key] ?? key);
            // }


            // 6) 如果別處（例如 menu 的 toggleLang）改了語言 cookie，呼叫這個就會重套翻譯
            // window.reloadTranslations = loadTranslationsAndApply;

            const init = async () => {
                try {
                    // 先載入翻譯再載入資料，確保初始畫面就有正確文字
                    await loadTranslationsAndApply();
                    await fetchUsers(null);
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

            onMounted(() => init())

            //以上多語系

            return {
                isLoading,

                users, pageIndex, pageSize, totalPages, pagedUsers,
                formatDate, deleteUser, editingId, tempStatus,
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
                applyCreateDateFilter,
                roleTextI18n,
                t
            }
        }
    })


    // 若已存在舊的 App，先解除掛載
    if (window.DashboardUsersApp && typeof window.DashboardUsersApp.unmount === 'function') {
        try { window.DashboardUsersApp.unmount() } catch (_) { }
    }

    if (document.querySelector('#adminUser')) {
        window.DashboardUsersApp = app.mount('#adminUser')
    }
}

// 首次進入完整頁面時（非 AJAX）也能自動掛載
if (document.querySelector('#adminUser') && !window.__PARTIAL_LOADING) {
    window.mountUsersPage()
}
