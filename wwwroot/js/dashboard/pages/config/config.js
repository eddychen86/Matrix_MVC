const { createApp, ref, reactive, onMounted } = Vue

window.mountConfigPage = function() {
  const app = createApp({
    setup() {
      //#region 變數
      
      const isLoading = ref(true)
      const showCreateForm = ref(false)
      const isSubmitting = ref(false)
      const userPermissions = ref(null)
      const formData = reactive({
        userName: '',
        email: '',
        password: '',
        passwordConfirm: '',
        role: 1,
        avatarFile: null,
        avatarPreview: null
      })
      const formErrors = reactive({
        userName: '',
        email: '',
        password: '',
        passwordConfirm: ''
      })
      const adminList_curPage = ref(1)
      const adminList_pageSize = ref(5)
      const adminList_totalPages = ref(1)
      const adminList_totalCount = ref(0)
      const adminList = reactive({
        header: [
          { title: 'Config_AdminList_UserName', class: '', fixed: false, },
          { title: 'Config_AdminList_DisplayName', class: '', fixed: false, },
          { title: 'Config_AdminList_SuperAdmin', class: '', fixed: false, },
          { title: 'Config_AdminList_Status', class: '', fixed: false, },
        ],
        data: []
      })
      const adminFilterVal = ref({
        Config_AdminList_Keyword: null,
        Config_AdminList_SuperAdmin: null,
        Config_AdminList_Status: null
      })
      const logFilterVal = ref({
        Config_LoginList_Keyword: null,
        Config_LoginList_Role: 0,
        Config_LoginList_Browser: null,
        Config_LoginList_ActionType: null,
        Config_LoginList_LoginTime: '',
        Config_LoginList_LogoutTime: '',
        Config_LoginList_StartTime: '',
      })
      const switchOpts = ref([
        { id: 0, title: 'All', value: 0 },
        { id: 1, title: 'Yes', value: 1 },
        { id: 2, title: 'No', value: 2 },
      ])
      const actionOpts = ref([
        { id: 0, title: 'Config_LoginList_ActionType_VIEW', value: 'VIEW' },
        { id: 1, title: 'Config_LoginList_ActionType_CREATE', value: 'CREATE' },
        { id: 2, title: 'Config_LoginList_ActionType_UPDATE', value: 'UPDATE' },
        { id: 3, title: 'Config_LoginList_ActionType_DELETE', value: 'DELETE' },
        { id: 4, title: 'Config_LoginList_ActionType_ERROR', value: 'ERROR' },
      ])
      const roleOpts = ref([
        { id: 0, title: 'All', value: 0 },
        { id: 1, title: 'Admin', value: 1 },
        { id: 2, title: 'SuperAdmin', value: 2 },
      ])
      const adminFilter = ref([
        { 
          id: 0, 
          title: 'Config_AdminList_Keyword', 
          type: 'text',
          placeholderKey: 'Config_AdminList_KeywordPlaceholder'
        },
        { id: 1, title: 'Config_AdminList_SuperAdmin', type: 'switch' },
        { id: 2, title: 'Config_AdminList_Status', type: 'switch' },
      ])
      const logFilter = ref([
        { 
          id: 0, 
          title: 'Config_LoginList_Keyword', 
          type: 'text',
          placeholderKey: 'Config_AdminList_KeywordPlaceholder'
        },
        { id: 1, title: 'Config_LoginList_Role', type: 'switch', options: roleOpts },
        // { 
        //   id: 2, 
        //   title: 'Config_LoginList_Browser', 
        //   type: 'Number',
        //   placeholderKey: 'Config_LoginList_Browser'
        // },
        { 
          id: 6, 
          title: 'Config_LoginList_ActionType', 
          type: 'select',
          options: actionOpts
        },
        { id: 3, title: 'Config_LoginList_LoginTime', type: 'DateTime' },
        { id: 4, title: 'Config_LoginList_LogoutTime', type: 'DateTime' },
        { id: 5, title: 'Config_LoginList_StartTime', type: 'DateTime' },
      ])
      //#endregion

      //#region data

      const getAvatarAsync = async path => {
        if (!path || path.trim() === '') { return null }

        try {
          const response = await fetch(path, { method: 'HEAD' })
          return response.ok ? path : null
        } catch (err) { result = null }
      }

      const getAdminsAsync = async (useFilter = false) => {
        try {
          const requestBody = {
            page: adminList_curPage.value,
            pageSize: adminList_pageSize.value
          }

          // 如果要使用篩選，加入篩選條件
          if (useFilter) {
            const filterConditions = {}

            if (adminFilterVal.value.Config_AdminList_Keyword) 
              filterConditions.keyword = adminFilterVal.value.Config_AdminList_Keyword
            if (adminFilterVal.value.Config_AdminList_SuperAdmin !== null && adminFilterVal.value.Config_AdminList_SuperAdmin !== 0)
              filterConditions.superAdmin = adminFilterVal.value.Config_AdminList_SuperAdmin === 1
            if (adminFilterVal.value.Config_AdminList_Status !== null && adminFilterVal.value.Config_AdminList_Status !== 0)
              filterConditions.status = adminFilterVal.value.Config_AdminList_Status === 1

            requestBody.filters = filterConditions
          }

          const response = await fetch('/api/Config', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(requestBody) })

          if(response.ok) {
            const result = await response.json()
            
            // 更新分頁資訊
            adminList_totalPages.value = result.totalPages || 1
            adminList_totalCount.value = result.totalCount || 0

            const processedData = await Promise.all(
              result.data.map(async m => ({
                id: m.userId,
                UserName: m.userName,
                DisplayName: m.displayName,
                email: m.email,
                Avatar: await getAvatarAsync(m.avatarPath), // 等待檢查結果
                SuperAdmin: m.role,
                status: m.status,
                isDelete: false,
                checked: false,
              }))
            )
            adminList.data = processedData
          } else {
            console.log('Error: data not found')

            // 重置資料
            adminList.data = []
            adminList_totalPages.value = 1
            adminList_totalCount.value = 0
          }
        } catch (err) { 
          console.log('Error: ', err)

          // 發生錯誤時重置資料
          adminList.data = []
          adminList_totalPages.value = 1
          adminList_totalCount.value = 0
        }
      }

      //#endregion

      //#region tools

      // 權限管理
      const PermissionService = {
        async getUserPermissions() {
          const response = await fetch('/api/Config/Permissions');
          const result = await response.json();
          return result.data;
        },

        async checkPermission(permissionKey) {
          const permissions = await this.getUserPermissions();
          return permissions.permissions[permissionKey];
        }
      };

      const startCreateAdmin = async () => {
        // 獲取當前用戶權限
        try {
          const permissions = await PermissionService.getUserPermissions()
          userPermissions.value = permissions
          // 重設表單數據
          resetFormData()
        } catch (error) {
          console.error('無法獲取用戶權限:', error)
        }
      }
      
      const cancelCreateAdmin = () => {
        showCreateForm.value = false
        resetFormData()
      }
      
      const resetFormData = () => {
        // reactive 不能直接賦值，需要更新各個屬性
        formData.userName = ''
        formData.email = ''
        formData.password = ''
        formData.passwordConfirm = ''
        formData.role = 1
        formData.avatarFile = null
        formData.avatarPreview = null
      }
      
      const triggerFileUpload = () => {
        // 觸發隱藏的檔案輸入
        const fileInput = document.querySelector('input[type="file"]')
        if (fileInput) fileInput.click()
      }
      
      const handleAvatarUpload = (event) => {
        const file = event.target.files[0]
        if (file) {
          // 檢查檔案大小 (5MB)
          if (file.size > 5 * 1024 * 1024) {
            alert('檔案大小不能超過 5MB')
            return
          }
          
          // 檢查檔案類型
          if (!file.type.match(/image\/(jpeg|jpg|png|gif)/)) {
            alert('只支援 JPG, PNG, GIF 格式的圖片')
            return
          }
          
          formData.avatarFile = file
          
          // 創建預覽圖片
          const reader = new FileReader()
          reader.onload = (e) => formData.avatarPreview = e.target.result
          reader.readAsDataURL(file)
        }
      }
      
      const submitCreateAdmin = async () => {
        // 提交前先清空舊的錯誤訊息
        for (const key in formErrors) {
          formErrors[key] = ''
        }

        isSubmitting.value = true
        
        try {
          // 準備請求數據
          const adminData = {
            userName: formData.userName,
            email: formData.email,
            password: formData.password,
            passwordConfirm: formData.passwordConfirm,
            role: parseInt(formData.role)
          }
          
          const response = await fetch('/api/Config/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(adminData)
          })
          
          const result = await response.json()
          
          if (response.ok && result.success) {
            alert(result.message || '管理員創建成功')
            
            // 關閉表單並重新獲取管理員列表
            cancelCreateAdmin()
            await getAdminsAsync()
          } else {
            // 處理後端返回的驗證錯誤
            if (result.errors) {
              for (const [field, messages] of Object.entries(result.errors)) {
                // 將後端欄位名稱（首字母小寫）對應到 formErrors
                const errorKey = field.charAt(0).toLowerCase() + field.slice(1)
                if (formErrors.hasOwnProperty(errorKey)) {
                  formErrors[errorKey] = messages.join(' ')
                }
              }
            } else {
              // 處理其他一般錯誤
              alert(result.message || '創建失敗')
            }
          }
        } catch (error) {
          console.error('創建管理員錯誤:', error)
          alert('網路錯誤，請稍後再試')
        } finally {
          isSubmitting.value = false
        }
      }

      const editAdmin = async id => {
        const admin = adminList.data.find(a => a.id === id)
        if (admin) {
          // 進入編輯模式
          admin.isEditing = true
          
          // 設置 toggle 開關的資料綁定
          admin.isSuperAdmin = admin.SuperAdmin === 2  // 轉換為 boolean
          admin.isActive = admin.status === 1          // 轉換為 boolean

          // 備份原始資料以便取消時還原
          admin.originalData = {
            SuperAdmin: admin.SuperAdmin,
            status: admin.status
          }
        }
      }
      
      const delAdmin = async id => {
        if (!confirm('確定要刪除此管理員嗎？')) return
        
        try {
          const response = await fetch(`/api/Config/Delete/${id}`, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' }
          })
          
          const result = await response.json()
          
          if (response.ok && result.success) {
            alert(result.message || '管理員刪除成功')
            // 重新載入管理員列表
            await getAdminsAsync()
          } else {
            alert(result.message || '刪除失敗')
          }
        } catch (error) {
          console.error('刪除管理員錯誤:', error)
          alert('網路錯誤，請稍後再試')
        }
      }
      
      const saveAdmin = async id => {
        const admin = adminList.data.find(a => a.id === id)
        if (!admin) return
        
        try {
          // 將 toggle 開關的 boolean 值轉換回數字格式
          const newRole = admin.isSuperAdmin ? 2 : 1
          const newStatus = admin.isActive ? 1 : 0
          
          const updateData = {
            userId: admin.id,
            displayName: admin.DisplayName,
            email: admin.email,
            role: newRole,
            status: newStatus
          }
          
          const response = await fetch(`/api/Config/Update/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updateData)
          })
          
          const result = await response.json()
          
          if (response.ok && result.success) {
            // 更新本地資料模型
            admin.SuperAdmin = newRole
            admin.status = newStatus
            
            alert(result.message || '管理員資料更新成功')
            
            // 退出編輯模式並清理暫時屬性
            admin.isEditing = false
            delete admin.originalData
            delete admin.isSuperAdmin
            delete admin.isActive
            
            // 重新載入管理員列表以確保資料一致性
            await getAdminsAsync()
          } else {
            alert(result.message || '更新失敗')
          }
        } catch (error) {
          console.error('更新管理員錯誤:', error)
          alert('網路錯誤，請稍後再試')
        }
      }
      
      const cancelEdit = id => {
        const admin = adminList.data.find(a => a.id === id)
        if (admin && admin.originalData) {
          // 還原原始資料
          admin.DisplayName = admin.originalData.DisplayName
          admin.email = admin.originalData.email
          admin.SuperAdmin = admin.originalData.SuperAdmin
          admin.status = admin.originalData.status
          
          // 退出編輯模式並清理所有暫時屬性
          admin.isEditing = false
          delete admin.originalData
          delete admin.isSuperAdmin  // 清理 toggle 屬性
          delete admin.isActive      // 清理 toggle 屬性
        }
      }

      // 篩選功能
      const applyFilter = async () => {
        adminList_curPage.value = 1 // 篩選時重置到第一頁
        await getAdminsAsync(true)
      }
      
      const clearFilter = async () => {
        // 重置篩選條件
        adminFilterVal.value.Config_AdminList_Keyword = null
        adminFilterVal.value.Config_AdminList_SuperAdmin = null
        adminFilterVal.value.Config_AdminList_Status = null
        adminList_curPage.value = 1
        await getAdminsAsync(false)
      }

      const toggleFirstPage = async () => {
        if (adminList_curPage.value !== 1) {
          adminList_curPage.value = 1
          await getAdminsAsync()
        }
      }
      
      const toggleLastPage = async () => {
        if (adminList_curPage.value !== adminList_totalPages.value) {
          adminList_curPage.value = adminList_totalPages.value
          await getAdminsAsync()
        }
      }
      
      const togglePrevPage = async () => {
        if (adminList_curPage.value > 1) {
          adminList_curPage.value -= 1
          await getAdminsAsync()
        }
      }
      
      const toggleNextPage = async () => {
        if (adminList_curPage.value < adminList_totalPages.value) {
          adminList_curPage.value += 1
          await getAdminsAsync()
        }
      }

      //#endregion

      //#region LifeCycle

      const init = async () => {
        try {
          // 預留：若未來有 /api/Db_ConfigApi 可在此呼叫
          await getAdminsAsync()
          await startCreateAdmin()
        } finally {
          isLoading.value = false
        }
      }

      onMounted(() => init())

      //#endregion

      return {
        isLoading,

        // Data
        adminList,
        adminList_curPage,
        adminList_pageSize,
        adminList_totalPages,
        adminList_totalCount,
        showCreateForm,
        isSubmitting,
        userPermissions,
        formData,
        formErrors,

        // tools
        startCreateAdmin,
        cancelCreateAdmin,
        triggerFileUpload,
        handleAvatarUpload,
        submitCreateAdmin,
        editAdmin,
        delAdmin,
        saveAdmin,
        cancelEdit,
        toggleFirstPage,
        toggleLastPage,
        togglePrevPage,
        toggleNextPage,
        applyFilter,
        clearFilter,

        // Filter
        switchOpts,
        actionOpts,
        adminFilterVal,
        logFilterVal,
        adminFilter,
        logFilter,
      }
    }
  })

  if (window.ConfigApp && typeof window.ConfigApp.unmount === 'function') {
    try { window.ConfigApp.unmount() } catch (_) {}
  }
  const el = document.querySelector('#adminConfig')
  if (el) window.ConfigApp = app.mount(el)
}

if (document.querySelector('#adminConfig')) {
  window.mountConfigPage()
}

