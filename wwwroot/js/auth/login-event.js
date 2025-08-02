const { createApp, ref, onMounted, nextTick } = Vue

createApp({
    setup() {
        // 響應式數據
        const isForgot = ref(false)
        const showPassword = ref(false)
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
            document.querySelectorAll('.input-item p').forEach(p => p.textContent = '')

            Object.keys(errors).forEach(field => {
                const errMsg = errors[field]
                if (errMsg && errMsg.length > 0) {
                    const el = document.querySelector(`p[asp-validation-for="${field}"]`)
                    if (el && field) {
                        el.textContent = errMsg[0]
                    } else if (!field) {
                        console.log('General error:', errMsg[0])
                    } else console.log(`Could not find validation element for ${field}`)
                }
            })
        }

        // 表單提交
        const submitForm = async (event) => {
            event.preventDefault()

            try {
                // 獲取表單數據
                const formData = new FormData(event.target)
                const token = formData.get('__RequestVerificationToken')

                const response = await fetch('/api/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({
                        UserName: loginForm.value.UserName,
                        Password: loginForm.value.Password,
                        RememberMe: loginForm.value.RememberMe
                    })
                })

                const result = await response.json()

                if (result.success && result.data?.redirectUrl) {
                    window.location.href = result.data.redirectUrl
                } else if (result.errors) updateErrorMsg(result.errors)
            } catch (error) {
                console.error('Login error:', error)
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
            submitForm
        }
    }
}).mount('#auth-body')