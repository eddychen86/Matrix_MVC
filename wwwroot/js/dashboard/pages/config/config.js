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
      const toolStateList = reactive([
        { id: 0, name: 'Friends', state: false },
        { id: 0, name: 'ProfileNFTs', state: false },
        { id: 0, name: 'Config_WebLog', state: false },
      ])
      const adminList_curPage = ref(1)
      const adminList_pageSize = ref(5)
      const adminList = reactive({
        header: [
          { title: 'Config_AdminList_UserName', class: '', fixed: false, },
          { title: 'Config_AdminList_DisplayName', class: '', fixed: false, },
          { title: 'Config_AdminList_SuderAdmin', class: '', fixed: false, },
          { title: 'Config_AdminList_Status', class: '', fixed: false, },
        ],
        data: []
      })
      //#endregion

      //#region data

      const getAvatarAsync = async path => {
        if (!path || path.trim() === '') { return null }

        try {
          const response = await fetch(path, { method: 'HEAD' })
          return response.ok ? path : null
        } catch (err) { result = null }
      }

      const getAdminsAsync = async () => {
        try {
          const response = await fetch('/api/Config', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              pages: adminList_curPage.value,
              pageSize: adminList_pageSize.value
            })
          })

          if(response.ok) {
            const result = await response.json()
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
          } else console.log('Error: data not found')
        } catch (err) { console.log('Error: ', err) }
      }

      //#endregion

      //#region tools

      // 驗證函數
      const isValidUserName = (userName) => {
        if (!userName) return true // 空值時不顯示錯誤
        const pattern = /^[A-Za-z][A-Za-z0-9\-]*$/
        return userName.length >= 3 && userName.length <= 20 && pattern.test(userName)
      }

      const isValidEmail = (email) => {
        if (!email) return true // 空值時不顯示錯誤
        const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        return pattern.test(email) && email.length <= 100
      }

      const isValidPassword = (password) => {
        if (!password) return true // 空值時不顯示錯誤
        const pattern = /(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[#@$!%*?&]).{8,}/
        return password.length >= 8 && password.length <= 20 && pattern.test(password)
      }

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
        formData = {
          userName: '',
          email: '',
          password: '',
          passwordConfirm: '',
          role: 1,
          avatarFile: null,
          avatarPreview: null ,
        }
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
        // 驗證密碼一致
        if (formData.password !== formData.passwordConfirm) {
          alert('密碼不一致')
          return
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
          
          // TODO: 如果有頭像檔案，需要先上傳頭像再創建管理員
          // 目前先不處理頭像上傳
          
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
            // 處理錯誤
            if (result.errors) {
              let errorMessages = []
              for (const [field, messages] of Object.entries(result.errors)) {
                errorMessages.push(...messages)
              }
              alert('創建失敗：\n' + errorMessages.join('\n'))
            } else {
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
      const editAdmin = id => {
        // adminList.data
        console.log(id)
      }
      const delAdmin = id => {
        // adminList.data
        console.log(id)
      }
      const saveAdmin = id => {
        // adminList.data
        console.log(id)
      }

      const toggleFirstPage = async () => {
        adminList_curPage.value = 1
        await getAdminsAsync()
      }
      const toggleLastPage = async () => {
        adminList_curPage.value = adminList?.pages || 1
        await getAdminsAsync()
      }
      const togglePrevPage = async () => {
        adminList_curPage.value > 1 ? adminList_curPage.value -= 1 : 1
        await getAdminsAsync()
      }
      const toggleNextPage = async () => {
        adminList_curPage.value < adminList?.pages ? adminList_curPage.value += 1 : adminList_curPage.value
        await getAdminsAsync()
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
        toolStateList,
        adminList,
        adminList_curPage,
        adminList_pageSize,
        showCreateForm,
        isSubmitting,
        userPermissions,
        formData,

        // tools
        isValidUserName,
        isValidEmail,
        isValidPassword,
        startCreateAdmin,
        cancelCreateAdmin,
        triggerFileUpload,
        handleAvatarUpload,
        submitCreateAdmin,
        editAdmin,
        delAdmin,
        saveAdmin,
        toggleFirstPage,
        toggleLastPage,
        togglePrevPage,
        toggleNextPage,
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

