import { useFormatting } from '/js/hooks/useFormatting.js'

const { createApp, ref, reactive, computed, watch, onMounted, onUnmounted } = Vue

window.mountReportsPage = function() {
  const app = createApp({
    setup() {
      const isLoading = ref(true)
      
      // 引入格式化功能
      const { formatDate, timeAgo } = useFormatting()

      //#region Reports 資料管理
      const adminNameCache = reactive({})  // { [id]: 'Eason' }
      const reports = ref([])
      const keyword = ref('')
      const status = ref('')      // '', 0=Pending, 1=Processed, 2=Rejected
      const type = ref('')        // '', 0=User, 1=Article
      const from = ref(null)      // yyyy-mm-dd
      const to = ref(null)        // yyyy-mm-dd
      const page = ref(1)
      const pageSize = ref(10)
      const total = ref(0)
      const rowBusy = reactive({})   // key = reportId, value = true/false
      //#endregion

      //#region Computed 計算屬性
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
      //#endregion

      //#region Watch 監聽器
      // 🔽 新增：當 keyword 清空時，自動回復到未搜尋狀態
      watch(keyword, (val, oldVal) => {
        // 只有在「從有字 → 變成空」時才 reload，避免初始化時觸發
        if (oldVal !== undefined && oldVal.trim() !== '' && val.trim() === '') {
          page.value = 1
          loadReports()
        }
      })
      //#endregion

      //#region 工具函式
      async function resolveAdminName(personId) {
        if (!personId) return null
        if (adminNameCache[personId]) return adminNameCache[personId]
        try {
          const res = await fetch(`/api/Db_Reports/persons/${personId}`)   // ← 這裡改成 persons + personId
          if (!res.ok) return null
          const data = await res.json()
          const name = data.displayName || data.name || data.username || null
          if (name) adminNameCache[personId] = name
          return name
        } catch {
          return null
        }
      }

      function formatDateValue(date) {
        const d = new Date(date)
        return d.toISOString().split('T')[0]  // yyyy-MM-dd 格式
      }

      //Report狀態判斷
      const isNotYet = s => s === 0 || s === '0' || s === undefined || s === null || s === '';

      // ✅ 幫按鈕決定是否「被選中」
      const isStatusActive = v => status.value === String(v);
      const isTypeActive = v => type.value === String(v);
      //#endregion

      //#region 過濾和搜尋功能
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

      function search() {
        page.value = 1
        loadReports()
      }

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
      //#endregion

      //#region 分頁功能
      function goPage(p) {
        if (typeof p !== 'number') return
        if (p < 1 || p > totalPages.value) return
        page.value = p
        loadReports()
      }
      //#endregion

      //#region 資料載入功能
      async function loadReports() {              //處理檢舉資料以及撈處理人
        try {
          const url = `/api/Db_Reports?${buildQuery()}`
          const res = await fetch(url)
          // console.log('GET', url, '→', res.status)
          if (!res.ok) {
            console.error('API Error:', res.status, res.statusText)
            return
          }
          const data = await res.json()

          // ⬇⬇ 正規化
          reports.value = (data.items ?? []).map(r => {
            // 🔧 修改：把狀態改成三態判斷（0=Pending, 1=Processed, 2=Rejected）
            const raw = (r.status ?? r.statusCode ?? r.Status ?? r.StatusCode)
            const s = String(raw ?? '').toLowerCase()
            const n = typeof raw === 'string' ? parseInt(raw, 10) : raw

            let statusCode = 0
            let statusText = 'Not yet'
            if (n === 1 || s === 'processed' || s === 'done' || s === 'success') {
              statusCode = 1
              statusText = 'Processed'
            } else if (n === 2 || s === 'rejected' || s === 'reject' || s === 'denied') { // ✅ 新增：Rejected 分支
              statusCode = 2
              statusText = 'Processed'
            }

            return {
              ...r,
              // 🔧 修改：改用上面算出的三態
              statusCode,                       // 0=Pending, 1=Processed, 2=Rejected
              statusText,

              // 後端欄位可能叫法不同 → 統一命名
              resolverName: r.resolverName || r.resolver || r.admin || null,
              resolverId: r.resolverId || r.adminId || r.managerId || null,

              // 🔧 修改：處理時間欄位統一
              processTime: r.processTime || r.modifyTime || null,
            }
          })

          // 補 resolverName（非同步補齊，不擋畫面）
          for (const it of reports.value) {
            if (!it.resolverName && it.resolverId) {
              resolveAdminName(it.resolverId).then(name => {
                if (name) it.resolverName = name
              })
            }
          }

          total.value = data.totalCount ?? 0
          if (window.lucide) setTimeout(() => window.lucide.createIcons(), 0)
        } catch (e) {
          console.error('loadReports error', e)
        }
      }
      //#endregion

      //#region 報告處理功能
      async function takeReportAction(item, action) {
        const id = item.reportId
        if (!id) return

        if (action !== 'process' && action !== 'reject') {
          console.error('Invalid action:', action)
          return
        }

        if (rowBusy[id]) return
        rowBusy[id] = true

        try {
          const url = `/api/Db_Reports/${id}/${action}`

          const res = await fetch(url, {
            method: 'POST',
            // 2) 一律帶 Cookie（.NET 會用 Cookie 驗身分）
            credentials: 'include', 
            headers: {
              'Content-Type': 'application/json'
            },
            // 4) 若後端需要額外資料（常見：備註、原因、resolverId），可放在 body
            //    沒需要就送 {} 即可
            body: JSON.stringify({})
          })

          if (!res.ok) {
            const text = await res.text().catch(() => '')
            console.warn('Action failed:', { url, id, action, status: res.status, text })
            return
          }

          const result = await res.json().catch(() => ({}))

          // ✅ 前端立即更新畫面
          item.statusCode = 1
          item.statusText = 'Processed'

          // ✅ 管理員名字：優先用後端回傳；其次用 resolverId 去查；最後用預設字樣
          if (result.resolverName) {
            item.resolverName = result.resolverName
          } else if (result.resolverId) {
            item.resolverId = result.resolverId
            const name = await resolveAdminName(result.resolverId)
            if (name) item.resolverName = name
          } else if (item.resolverId) {
            const name = await resolveAdminName(item.resolverId)
            if (name) item.resolverName = name
          } else {
            item.resolverName = item.resolverName || 'Admin'
          }

          if (result.processTime) item.processTime = result.processTime

          // （可選）再拉一次列表，確保與後端一致
          // await loadReports()

          if (window.lucide) setTimeout(() => window.lucide.createIcons(), 0)
        } catch (err) {
          console.error('takeReportAction error', err)
        } finally {
          rowBusy[id] = false
        }
      }

      async function processReport(id) {
        const res = await fetch(`/api/Db_Reports/${id}/process`, { method: 'POST' })
        if (res.ok) loadReports()
      }

      async function rejectReport(id) {
        const res = await fetch(`/api/Db_Reports/${id}/reject`, { method: 'POST' })
        if (res.ok) loadReports()
      }
      //#endregion

      const init = async () => {
        try {
          // 初始化 Reports 相關功能
          
          // 設置清除日期按鈕事件
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

          // 如果目前頁面存在 Reports 的容器，就自動載入
          if (document.getElementById('reports-app')) {
            // console.log('Reports app container found, loading reports...')
            loadReports()
          } else {
            console.log('Reports app container not found, setting up observer...')
            // 監聽 DOM，等 #reports-app 出現再載一次
            const mo = new MutationObserver((mutations, obs) => {
              if (document.getElementById('reports-app')) {
                console.log('Reports app container appeared, loading reports...')
                loadReports()
                obs.disconnect()
              }
            })
            mo.observe(document.body, { childList: true, subtree: true })
          }
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      onUnmounted(() => {
        delete window.setReportDate
        delete window.clearReportDate
      })

      return { 
        isLoading,
        // Reports 資料
        reports, keyword, status, type, from, to,
        page, pageSize, totalPages, showPage, total,
        rowBusy,
        // Reports 功能函式
        loadReports, search, goPage,
        
        processReport, rejectReport, takeReportAction,
        setStatus, setType, isStatusActive, isTypeActive,
        applyFilters, isNotYet,
        // 格式化函式
        formatDate, timeAgo,
        // 工具函式
        formatDateValue, resolveAdminName
      }
    }
  })

  if (window.ReportsApp && typeof window.ReportsApp.unmount === 'function') {
    try { window.ReportsApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#reports-app')
  if (el) window.ReportsApp = app.mount(el)
}

// 確保 DOM 載入完成後再掛載
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('#reports-app')) {
      console.log('DOM loaded, mounting reports page...')
      window.mountReportsPage()
    }
  })
} else {
  if (document.querySelector('#reports-app')) {
    console.log('DOM already loaded, mounting reports page...')
    window.mountReportsPage()
  }
}