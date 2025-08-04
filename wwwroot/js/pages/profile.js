const useProfile = () => {
    const { ref, reactive, onMounted } = Vue
    const { timeAgo } = useFormatting()

    // 響應式數據
    const rand = ref(1)
    const createTime = ref([])
    const content = ref([])
    const editMode = ref(false)
    const isPublic = ref(false)
    
    const profile = reactive({
        isPrivate: false,
        articles: [],
        bio: '',
        displayName: '',
        email: '',
        password: '',
        website1: '',
        website2: '',
        website3: '',
        avatarPath: '',
        bannerPath: '',
        userId: ''
    })

    // 方法
    const toggleIcon = () => {
        isPublic.value = !isPublic.value
    }

    const cancel = () => {
        editMode.value = false
        if (profile.backup) {
            profile.bio = profile.backup.bio
            profile.displayName = profile.backup.displayName
            profile.email = profile.backup.email
            profile.password = profile.backup.password
            profile.website1 = profile.backup.website1
            profile.website2 = profile.backup.website2
            profile.website3 = profile.backup.website3
            delete profile.backup
        }
    }

    const startEdit = () => {
        editMode.value = true
        profile.backup = JSON.parse(JSON.stringify(profile))
    }

    const update = async () => {
        try {
            const userId = await getCurrentUserId()
            if (!userId) return
            
            const data = {
                bio: profile.bio,
                displayName: profile.displayName,
                email: profile.email,
                password: profile.password,
                website1: profile.website1,
                website2: profile.website2,
                website3: profile.website3,
                userId: userId
            }

            const response = await axios.put(`/api/ProfileApi/${userId}`, data, {
                headers: {
                    "Content-Type": "application/json"
                }
            })

            alert(response.data.message || response.data)
            editMode.value = false
            rand.value = new Date().getTime()
        } catch (err) {
            alert("更新失敗")
            console.error(err)
        }
    }

    const editFileChange = (inputTypeFile) => {
        readURL(inputTypeFile, inputTypeFile.parentElement.previousSibling, document.getElementById("update"))
    }

    const readURL = (inputTypeFile, img, btn) => {
        if (inputTypeFile.files[0] != null) {
            const file = inputTypeFile.files[0]
            const allowTypes = "image.*"
            
            if (file.type.match(allowTypes)) {
                if (btn) btn.disabled = false
                const reader = new FileReader()
                reader.onload = (e) => {
                    if (img) {
                        img.src = e.target.result
                        img.title = file.name
                    }
                }
                reader.readAsDataURL(file)
            } else {
                alert("不允許的檔案上傳類型！")
                if (btn) btn.disabled = true
                inputTypeFile.value = ""
            }
        }
    }

    // 獲取當前用戶 ID 的工具函數
    const getCurrentUserId = async () => {
        // 首先嘗試從全局狀態獲取
        if (window.currentUser?.userId) {
            return window.currentUser.userId
        }
        
        // 等待全局狀態初始化
        for (let i = 0; i < 20; i++) {
            if (window.currentUser?.userId) {
                return window.currentUser.userId
            }
            await new Promise(resolve => setTimeout(resolve, 50))
        }
        
        // 備用方案：直接調用 API
        try {
            const response = await fetch('/api/auth/status')
            const data = await response.json()
            if (data.success && data.data.authenticated && data.data.user.id) {
                return data.data.user.id
            }
        } catch (error) {
            console.error('無法從 API 獲取用戶 ID:', error)
        }
        
        // 最後的備用方案
        console.warn('無法獲取當前用戶 ID，使用預設 ID')
        return null // '8615C454-4C27-4C41-9256-E609DC8465C5'
    }

    // 載入 Profile 資料
    const loadProfile = async () => {
        try {
            const userId = await getCurrentUserId()
            if (!userId) return

            console.log('使用的 userId:', userId)

            const response = await fetch('/api/ProfileApi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: userId })
            })

            const data = await response.json()

            Object.assign(profile, {
                ...data,
                articles: data.articles
                    // .filter(f => f.articleId === userId)
                    .map(m => ({
                        ...m,
                        authorAvatar: m.authorAvatar || m.authorName.slice(0, 1).toUpperCase(),
                        createTime: timeAgo(m.createTime, 'en-US'),
                    }))
            })

            // console.log(profile)

            isPublic.value = !data.isPrivate
        } catch (err) {
            console.error('載入 Profile 失敗:', err)
        }
    }

    // 組件掛載時載入資料
    onMounted(() => {
        loadProfile()
    })

    return {
        // 響應式數據
        rand,
        createTime,
        content,
        editMode,
        isPublic,
        profile,
        
        // 方法
        toggleIcon,
        cancel,
        startEdit,
        update,
        editFileChange,
        readURL,
        loadProfile
    }
}

// 導出給全局使用
window.useProfile = useProfile