const { createApp, ref, onMounted, nextTick } = Vue

createApp({
    setup() {
        // 響應式數據
        const isForgot = ref(false)
        const showPassword = ref(false)
        const isSubmitting = ref(false)
        const forgotEmail = ref('')
        const isForgotSubmitting = ref(false)
        const forgotMessage = ref('')
        const forgotMessageType = ref('success')
        const loginForm = ref({
            UserName: '',
            Password: '',
            RememberMe: false
        })

        // 切換忘記密碼彈窗
        const toggleOpen = () => isForgot.value = true

        const submitForgotPassword = async (event) => {
            console.log('submitForgotPassword called', event)
            event.preventDefault()
            
            console.log('isForgot.value:', isForgot.value)
            console.log('forgotEmail.value:', forgotEmail.value)
            
            if (!isForgot.value) {
                console.log('Forgot password modal not open, returning')
                return
            }
            
            if (!forgotEmail.value) {
                console.log('No email provided')
                forgotMessage.value = '請輸入電子郵件'
                forgotMessageType.value = 'error'
                return
            }
            
            console.log('Starting forgot password request...')
            isForgotSubmitting.value = true
            forgotMessage.value = ''
            
            try {
                console.log('Sending request to API...')
                const response = await fetch('/api/ForgotPassword/reset', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        Email: forgotEmail.value
                    })
                })
                
                console.log('Response received:', response.status, response.statusText)
                const result = await response.json()
                console.log('Result:', result)
                
                if (result.success) {
                    forgotMessage.value = result.message
                    forgotMessageType.value = 'success'
                    forgotEmail.value = ''
                    console.log('Success message set')
                } else {
                    forgotMessage.value = result.message || '重置失敗'
                    forgotMessageType.value = 'error'
                    console.log('Error message set:', result.message)
                }
            } catch (error) {
                console.error('Forgot password error:', error)
                forgotMessage.value = '發送失敗，請稍後再試'
                forgotMessageType.value = 'error'
            } finally {
                isForgotSubmitting.value = false
                console.log('Request completed')
            }
        }

        const toggleClose = (event) => {
            if (event.target === event.currentTarget) isForgot.value = false
        }

        // 切換密碼顯示/隱藏
        const togglePasswordVisibility = () => {
            showPassword.value = !showPassword.value
            // 等待 Vue 完成 DOM 更新後再重新創建 Lucide 圖標
            nextTick(() => {
                setTimeout(() => {
                    lucide.createIcons()
                }, 0)
            })
        }

        // 更新錯誤訊息
        const updateErrorMsg = (errors) => {
            // 清除之前的錯誤訊息
            document.querySelectorAll('p[data-valmsg-for]').forEach(p => p.textContent = '')

            // console.log('Received errors:', errors) // Debug log

            Object.keys(errors).forEach(field => {
                const errMsg = errors[field]
                if (errMsg && errMsg.length > 0) {
                    // 直接使用 data-valmsg-for 選擇器
                    const el = document.querySelector(`p[data-valmsg-for="${field}"]`)

                    if (el) {
                        el.textContent = errMsg[0]
                        // console.log(`Set error for ${field}:`, errMsg[0]) // Debug log
                    } else {
                        console.log(`Could not find validation element for ${field}`)
                        console.log('Available validation elements:', document.querySelectorAll('p[data-valmsg-for]'))
                    }
                }
            })
        }

        // 表單提交
        const submitForm = async (event) => {
            event.preventDefault()

            // 設定提交狀態
            isSubmitting.value = true

            try {
                // 獲取表單數據
                const formData = new FormData(event.target)
                const token = formData.get('__RequestVerificationToken')
                const userName = formData.get('UserName') || loginForm.value.UserName
                const password = formData.get('Password') || loginForm.value.Password
                const rememberMe = formData.get('RememberMe') === 'true' || loginForm.value.RememberMe

                const response = await fetch('/api/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({
                        UserName: userName,
                        Password: password,
                        RememberMe: rememberMe
                    })
                })

                if (!response.ok) {
                    // 讀取錯誤響應內容
                    const errorText = await response.text()
                    console.error('Login API error response:', errorText)
                    throw new Error(`HTTP error! status: ${response.status}`)
                }

                const result = await response.json()

                if (result.success) {
                    // 檢查是否需要強制修改密碼
                    if (result.data?.forcePasswordChange) {
                        // 停止載入狀態並顯示修改密碼提醒
                        isSubmitting.value = false
                        alert('您的密碼已重置，請立即修改密碼以確保帳號安全！')
                        // 可以在這裡添加跳轉到修改密碼頁面的邏輯
                        setTimeout(() => {
                            window.location.href = result.data.redirectUrl
                        }, 1000)
                    } else if (result.data?.redirectUrl) {
                        // 正常登入流程
                        setTimeout(() => {
                            window.location.href = result.data.redirectUrl
                        }, 5000)
                    } else {
                        console.error('No redirectUrl provided in response')
                        isSubmitting.value = false
                    }
                } else {
                    console.log('Login failed:', result.message)
                    // 失敗時重置提交狀態
                    isSubmitting.value = false
                    if (result.errors) updateErrorMsg(result.errors)
                }
            } catch (error) {
                console.error('Login error:', error)
                // 發生錯誤時重置提交狀態
                isSubmitting.value = false
            }
        }

        // 組件掛載後初始化
        onMounted(() => {
            // 綁定表單數據到 DOM 元素
            const userName = document.querySelector('input[name="UserName"]')
            const pwd = document.querySelector('input[name="Password"]')
            const rememberMe = document.querySelector('input[name="RememberMe"]')

            if (userName) userName.addEventListener('input', e => loginForm.value.UserName = e.target.value)
            if (pwd) pwd.addEventListener('input', e => loginForm.value.Password = e.target.value)
            if (rememberMe) rememberMe.addEventListener('change', e => loginForm.value.RememberMe = e.target.checked)

            // 初始化 Lucide 圖標
            lucide.createIcons()
        })

        return {
            submitForgotPassword,
            isForgot,
            showPassword,
            loginForm,
            toggleOpen,
            toggleClose,
            togglePasswordVisibility,
            submitForm,
            isSubmitting,
            forgotEmail,
            isForgotSubmitting,
            forgotMessage,
            forgotMessageType
        }
    }
}).mount('#auth-body')
