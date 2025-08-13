export const useUsers = () => {
  // setup() 內的內容全部移到這裡
  const { createApp, ref, computed, onMounted } = Vue

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
  //篩選 + 分頁
  const filteredUsers = computed(() => {
    let arr = users.value
    if (statusFilter.value !== null) {
      arr = arr.filter(u => u.status === statusFilter.value)
      return arr
    }
    if (bannedFilter.value !== null) {
      arr = bannedFilter.value
        ? arr.filter(u => u.status === 2)     // Yes → 被封禁
        : arr.filter(u => u.status !== 2)     // No  → 非封禁
    }
    return arr
  })

  const totalPages = computed(() => Math.max(1, Math.ceil(filteredUsers.value.length / pageSize.value)))
  const pagedUsers = computed(() => {
    const start = (pageIndex.value - 1) * pageSize.value
    return filteredUsers.value.slice(start, start + pageSize.value)
  })



  const statusText = (value) => ({
    0: "未啟用",
    1: "已啟用",
    2: "被封禁"
  }[value] ?? value)

  const formatDate = (datetime) => {
    const date = new Date(datetime)
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
  }
  //Ban
  const statusFilter = ref(null)  // 1=Enable, 0=Disable
  const bannedFilter = ref(null)  // true=yes, false=no

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
      const response = await fetch(`/api/ApiUser${qs}`)
      if (!response.ok) throw new Error('API 錯誤')
      const data = await response.json()
      users.value = data
      pageIndex.value = 1
    } catch (error) {
      console.error('載入失敗：', error)
    }
  }
  //月曆

  const onCalendarChange = (e) => {
    const raw = e?.detail?.value ?? e?.target?.value
    if (!raw) {
      selectedDate.value = null
      lastYmd.value = null
      fetchUsers(null)
      return
    }
    const ymd = formatYMD(new Date(raw))
    if (lastYmd.value === ymd) {
      //點第二次同一天,取消篩選
      selectedDate.value = null
      lastYmd.value = null
      fetchUsers(null)
    } else {
      //第一天或換一天,設定篩選
      selectedDate.value = new Date(raw)
      lastYmd.value = ymd
      fetchUsers(ymd)
    }

    // selectedDate.value = new Date(raw)
    // const ymd = formatYMD(selectedDate.value)
    // fetchUsers(ymd)
  }

  const deleteUser = (id) => {
    if (!confirm("你確定要刪除嗎?要不要再考慮一下")) return;

    fetch(`/api/ApiUser/${id}`, {
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
      const res = await fetch(`/api/ApiUser/${item.userId}`, {
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

  onMounted(() => fetchUsers(null))

  return {
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
    applyNameFilter

  }
}

export default useUsers