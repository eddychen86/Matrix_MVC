const { createApp, ref, onMounted, nextTick } = Vue

createApp({
    setup() {
        // 響應式數據
        const isForgot = ref(false)
        const showPassword = ref(false)
        const isSubmitting = ref(false)
        const loginForm = ref({
            UserName: '',
            Password: '',
            RememberMe: false
        })

        // 切換忘記密碼彈窗
        const toggleOpen = () => isForgot.value = true

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
                    throw new Error(`HTTP error! status: ${response.status}`)
                }

                const result = await response.json()

                if (result.success) {
                    if (result.data?.redirectUrl) {
                        // 成功時保持 loading 狀態直到頁面跳轉
                        // console.log(
                        //     'Redirecting to:', result.data.redirectUrl,
                        //     "window.location.href", window.location.origin
                        // )
                        setTimeout(() => {
                            // history.pushState(null, '', '/dashboard/overview/index')
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
            isForgot,
            showPassword,
            loginForm,
            toggleOpen,
            toggleClose,
            togglePasswordVisibility,
            submitForm,
            isSubmitting,
        }
    }
}).mount('#auth-body')