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
        const forgotPwdDisplay = ref(null)
        const forgotMessageType = ref('success')
        const loginForm = ref({
            UserName: '',
            Password: '',
            RememberMe: false
        })

        // 切換忘記密碼彈窗
        const toggleOpen = () => {
            // 開啟狀態
            isForgot.value = true

            // 保障：移除可能殘留的 display:none 與 hidden 類別
            try {
                const targets = document.querySelectorAll('#auth-layout_forgot, [data-vue-init]')
                targets.forEach(el => {
                    if (!el) return
                    // 清除行內 display:none
                    if (el.style && el.style.display === 'none') {
                        el.style.display = ''
                    }
                })
            } catch (err) {
                console.error('toggleOpen: failed to clear display:none/hidden', err)
            }
        }

        const submitForgotPassword = async (event) => {
            event.preventDefault()
            
            
            if (!isForgot.value) {
                return
            }
            
            if (!forgotEmail.value) {
                forgotMessage.value = '請輸入電子郵件'
                forgotMessageType.value = 'error'
                return
            }
            
            isForgotSubmitting.value = true
            forgotMessage.value = ''
            
            try {
                const response = await fetch('/api/ForgotPassword/reset', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        Email: forgotEmail.value
                    })
                })
                
                const result = await response.json()
                
                if (result.success) {
                    forgotMessage.value = result.message
                    forgotMessageType.value = 'success'
                    forgotEmail.value = ''
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
            }
        }

        // 點擊背景遮罩關閉彈窗
        const toggleClose = (event) => {
            // 只有點擊背景遮罩時才關閉
            if (event && event.target === event.currentTarget) {
                isForgot.value = false
            }
        }

        // 直接關閉彈窗（用於關閉按鈕）
        const closeModal = (event) => {
            if (event) {
                event.preventDefault()
                event.stopPropagation()
            }
            isForgot.value = false
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

            Object.keys(errors).forEach(field => {
                const errMsg = errors[field]
                if (errMsg && errMsg.length > 0) {
                    // 直接使用 data-valmsg-for 選擇器
                    const el = document.querySelector(`p[data-valmsg-for="${field}"]`)

                    if (el) {
                        el.textContent = errMsg[0]
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
            // 移除 Vue 初始化前的隱藏樣式
            const elementsToShow = document.querySelectorAll('[data-vue-init]')
            elementsToShow.forEach(el => {
                el.style.display = ''
            })

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
            closeModal,
            togglePasswordVisibility,
            submitForm,
            isSubmitting,
            forgotEmail,
            isForgotSubmitting,
            forgotMessage,
            forgotMessageType,
            forgotPwdDisplay
        }
    }
}).mount('#auth-body')
