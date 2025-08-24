import { useFormatting } from '/js/hooks/useFormatting.js'
import { usePostActions } from '/js/hooks/usePostActions.js'
import postListService from '/js/components/PostListService.js'

export const useProfile = () => {
    const { ref, reactive, onMounted } = Vue
    const { timeAgo } = useFormatting()

    //#region Reactive State
    const rand = ref(1)
    const createTime = ref([])
    const content = ref([])
    const editMode = ref(false)
    const isPublic = ref(false)
    const profile = reactive({
        personId: null,
        userId: null,
        bannerPath: '',
        avatarPath: '',
        displayName: '',
        bio: '',
        email: '',
        password: '',
        website1: '',
        website2: '',
        website3: '',
        isPrivate: false
    })
    const updatProfile = reactive({
        personId: null,
        userId: null,
        bannerPath: '',
        avatarPath: '',
        displayName: '',
        bio: '',
        email: '',
        password: '',
        website1: '',
        website2: '',
        website3: '',
        isPrivate: false
    })
    const posts = ref([])
    const counts = ref(0)
    const currentPage = ref(1)
    const isLoading = ref(false)
    const hasMorePosts = ref(true)
    const userImages = ref([])

    // 密碼驗證相關狀態
    const passwordValidation = reactive({
        isValid: true,
        message: '',
        isValidating: false
    })

    // 密碼顯示狀態
    const showPassword = ref(false)
    //#endregion

    //#region Profile Management
    const toggleIcon = () => {
        isPublic.value = !isPublic.value
    }

    const cancel = () => {
        // 隱藏元件
        const editEl = document.querySelector('.profile-edit')
        if (editEl) editEl.style.display = 'none'
        
        editMode.value = false
        if (profile.backup) {
            // Restore from backup
            Object.assign(profile, profile.backup);
            delete profile.backup
        }
    }

    const startEdit = () => {
        editMode.value = true
        // Create a deep copy for backup
        Object.assign(updatProfile, JSON.parse(JSON.stringify(profile)))

        // 在下一個tick更新翻譯，確保DOM已經渲染並顯示元件
        Vue.nextTick(() => {
            const editEl = document.querySelector('.profile-edit')
            if (editEl) editEl.style.display = ''
            updateEditPopupTranslations()
        })
    }

    // 更新編輯彈窗的翻譯
    const updateEditPopupTranslations = (translations = null) => {
        try {
            // 如果沒有提供翻譯，嘗試從全域系統獲取
            if (!translations) {
                const currentLang = document.documentElement.lang || 'zh-TW'
                translations = window.translationCache?.get(currentLang)
            }

            if (translations) {
                // 只更新EditProfilePopup區域內的翻譯
                const popup = document.querySelector('.profile-edit')
                if (popup) {
                    // 找到彈窗內所有帶有 data-i18n 屬性的元素並更新
                    popup.querySelectorAll('[data-i18n]').forEach(element => {
                        const key = element.getAttribute('data-i18n')
                        if (translations[key]) {
                            // 檢查元素類型來決定更新方式
                            if (element.tagName === 'INPUT' && (element.type === 'submit' || element.type === 'button')) {
                                element.value = translations[key]
                            } else if (element.placeholder !== undefined && element.hasAttribute('data-i18n-placeholder')) {
                                element.placeholder = translations[key]
                            } else {
                                element.textContent = translations[key]
                            }
                        }
                    })
                }
            }
        } catch (error) {
            console.error('Failed to update popup translations:', error)
        }
    }

    const update = async () => {
        try {
            // 檢查密碼是否有效（如果有輸入密碼的話）
            if (updatProfile.password && updatProfile.password.trim() !== '') {
                const isPasswordValid = await validatePassword(updatProfile.password)
                if (!isPasswordValid) {
                    alert('密碼不符合規則，請檢查後重新提交')
                    return
                }
            }

            // Server端會自動從認證中獲取UserId，不需要傳遞
            const data = {
                bio: updatProfile.bio,
                displayName: updatProfile.displayName,
                email: updatProfile.email,
                // 只有當密碼不為空時才傳送密碼更新
                ...(updatProfile.password && updatProfile.password.trim() !== '' && { password: updatProfile.password }),
                isPrivate: updatProfile.isPrivate ? 1 : 0,
                website1: updatProfile.website1,
                website2: updatProfile.website2,
                website3: updatProfile.website3,
            }

            const response = await fetch('/api/Profile/personal', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            alert(result.message || "Profile updated successfully!");

            // 更新 profile 資料
            Object.assign(profile, updatProfile);

            // 隱藏元件
            const editEl = document.querySelector('.profile-edit')
            if (editEl) editEl.style.display = 'none'
            
            editMode.value = false
            rand.value = new Date().getTime() // Force re-render
        } catch (err) {
            alert("更新失敗")
            console.error("Failed to update profile:", err)
        }
    }

    // 密碼驗證函數
    const validatePassword = async (password) => {
        if (!password || password.trim() === '') {
            passwordValidation.isValid = true
            passwordValidation.message = ''
            passwordValidation.isValidating = false
            return true
        }

        passwordValidation.isValidating = true

        try {
            const response = await fetch('/api/Profile/validate-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ password: password })
            })

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const result = await response.json()
            passwordValidation.isValid = result.isValid
            passwordValidation.message = result.message || ''

            return result.isValid
        } catch (error) {
            console.error('密碼驗證失敗:', error)
            passwordValidation.isValid = false
            passwordValidation.message = '驗證過程中發生錯誤，請稍後再試'
            return false
        } finally {
            passwordValidation.isValidating = false
        }
    }

    // 防抖動密碼驗證
    let passwordValidationTimeout = null
    const validatePasswordWithDebounce = (password) => {
        if (passwordValidationTimeout) {
            clearTimeout(passwordValidationTimeout)
        }

        // 如果密碼為空，立即重設驗證狀態
        if (!password || password.trim() === '') {
            passwordValidation.isValid = true
            passwordValidation.message = ''
            passwordValidation.isValidating = false
            return
        }

        passwordValidationTimeout = setTimeout(() => {
            validatePassword(password)
        }, 500) // 500ms 後執行驗證
    }

    // 切換密碼顯示/隱藏
    const togglePasswordVisibility = () => {
        showPassword.value = !showPassword.value
    }
    //#endregion

    //#region NFT Management
    const uploadNFT = () => {
        // Create file input for NFT upload
        const input = document.createElement('input')
        input.type = 'file'
        input.accept = 'image/*'
        input.multiple = true
        input.style.display = 'none'

        input.addEventListener('change', async (event) => {
            const files = Array.from(event.target.files)
            if (files.length === 0) return

            for (const file of files) {
                await handleNFTUpload(file)
            }

            // Refresh user images after upload
            await GetNTFImages(profile?.personId, 30)
        })

        document.body.appendChild(input)
        input.click()
        document.body.removeChild(input)
    }

    const userNFTs = Vue.ref([])

    async function GetNTFImages(personId, count = 30) {
        const params = new URLSearchParams({ count: String(count) })
        if (personId) params.append('personId', personId)
        const response = await fetch(`/api/Nft/images?${params.toString()}`, {
            method: "GET",
            credentials: "include",
            cache: "no-store"
        })
        if (!response.ok) throw new Error(`Http${response.status}`)
        const nfts = await response.json();
        userNFTs.value = nfts
        return nfts
    }

    const handleNFTUpload = async (file) => {
        // Check file type
        if (!file.type.startsWith('image')) {
            alert('請選擇圖片')
            return
        }

        // Check file size (limit 10MB for NFTs)
        const maxSize = 10 * 1024 * 1024 // 10MB
        if (file.size > maxSize) {
            alert('檔案大小不可超過 10MB')
            return
        }

        try {
            // Create FormData
            const formData = new FormData()
            formData.append('file', file)
            formData.append('type', 'nft')
            formData.append('category', 'artwork')

            // Send upload request
            const response = await fetch('/api/Nft/upload', {
                method: 'POST',
                credentials: 'include',
                body: formData
            })

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const result = await response.json()

            if (result.success) {
                console.log('NFT uploaded successfully:', result.data.filePath)
                alert('NFT 上傳成功！')
            } else {
                throw new Error(result.message || '上傳失敗')
            }
        } catch (error) {
            console.error('NFT upload failed:', error)
            alert('上傳失敗，請稍後再試')
        }
    }

    const viewAllNFTs = () => {
        // Open NFT gallery modal or navigate to full NFT collection page
        showNFTGallery()
    }

    const viewNFTDetail = (image) => {
        // Show individual NFT detail modal
        showNFTDetailModal(image)
    }

    // ====== More 視窗 ======
    const showNFTGallery = () => {
        ensureGalleryStyles()

        const modal = document.createElement('div')
        modal.className = 'nft-backdrop'
        modal.innerHTML = `
    <div class="nft-grid-shell">
      <div id="nft-gallery-grid" class="nft-grid"></div>
    </div>
  `
        document.body.appendChild(modal)

        // 用下面改好的 loadAllNFTs 來塞縮圖
        loadAllNFTs(modal.querySelector('#nft-gallery-grid'))

        // 點背景關閉
        modal.addEventListener('click', (e) => {
            if (e.target === modal) modal.remove()
        })
    }
    const ensureGalleryStyles = () => {
        if (document.getElementById('nft-gallery-inline-css')) return
        const style = document.createElement('style')
        style.id = 'nft-gallery-inline-css'
        style.textContent = `
    /* Backdrop */
    .nft-backdrop{position:fixed;inset:0;background:rgba(0,0,0,.85);z-index:9999;display:flex;align-items:center;justify-content:center;padding:16px;}
    /* Grid shell */
    .nft-grid-shell{max-width:1200px;width:100%;max-height:90vh;overflow:auto;background:transparent;border-radius:12px;padding:8px;}
    /* Responsive grid */
    .nft-grid{display:grid;grid-template-columns:repeat(2,1fr);gap:12px;}
    @media (min-width:640px){.nft-grid{grid-template-columns:repeat(3,1fr);}}
    @media (min-width:900px){.nft-grid{grid-template-columns:repeat(4,1fr);}}
    @media (min-width:1200px){.nft-grid{grid-template-columns:repeat(5,1fr);}}
    /* Square cards */
    .nft-card{position:relative;width:100%;padding-bottom:100%;overflow:hidden;border-radius:10px;background:#111;cursor:pointer;}
    .nft-card>img{position:absolute;inset:0;width:100%;height:100%;object-fit:cover;}
    .nft-empty{color:#bbb;text-align:center;padding:24px;grid-column:1/-1;}
    /* NFT Gallery Grid (More) */
    .nft-actions { margin-top: 12px;text-align: center;}
    .nft-delete {background: #e53935;color: #fff;border: none;padding: 8px 16px;border-radius: 8px;cursor: pointer;}
    .nft-delete:hover {background: #c62828;}
    /* --- viewer for enlarged image --- */
    .nft-viewer{position:fixed;inset:0;background:rgba(0,0,0,.85);z-index:10000;display:flex;align-items:center;justify-content:center;padding:16px;}
    .nft-viewer-col{display:flex;flex-direction:column;align-items:center;gap:12px;max-width:90vw;max-height:90vh;}
    .nft-viewer-frame{position:relative;width:78vmin;height:78vmin;display:flex;align-items:center;justify-content:center;background:#000;border-radius:12px;box-shadow:0 10px 30px rgba(0,0,0,.5);}
    .nft-viewer-frame img{width:100%;height:100%;object-fit:contain;border-radius:12px;}
    .nft-close{position:absolute;top:8px;right:8px;background:rgba(0,0,0,0.6);color:#fff;border:none;border-radius:10px;padding:6px 10px;font-size:16px;cursor:pointer}
    .nft-actions{text-align:center}
    .nft-delete{background:#e53935;color:#fff;border:none;padding:8px 16px;border-radius:8px;cursor:pointer}
    .nft-delete:hover{background:#c62828}

    `
        document.head.appendChild(style)
    }
    //點開後的大圖
    function showNFTDetailModal(nft) {
        // 背景 overlay
        const overlay = document.createElement('div')
        overlay.style.position = 'fixed'
        overlay.style.inset = '0'
        overlay.style.background = 'rgba(0,0,0,0.85)'
        overlay.style.zIndex = '10000'
        overlay.style.display = 'flex'
        overlay.style.alignItems = 'center'
        overlay.style.justifyContent = 'center'
        overlay.style.padding = '16px'

        // 直向容器
        const col = document.createElement('div')
        col.style.display = 'flex'
        col.style.flexDirection = 'column'
        col.style.alignItems = 'center'
        col.style.gap = '12px'
        col.style.maxWidth = '90vw'
        col.style.maxHeight = '90vh'

        // 方形圖框
        const frame = document.createElement('div')
        frame.style.position = 'relative'
        frame.style.width = '78vmin'
        frame.style.height = '78vmin'
        frame.style.display = 'flex'
        frame.style.alignItems = 'center'
        frame.style.justifyContent = 'center'
        frame.style.background = '#000'
        frame.style.borderRadius = '12px'
        frame.style.boxShadow = '0 10px 30px rgba(0,0,0,.5)'

        // 圖片
        const img = new Image()
        img.src = nft.filePath
        img.alt = nft.fileName || ''
        img.style.width = '100%'
        img.style.height = '100%'
        img.style.objectFit = 'contain'
        img.style.borderRadius = '12px'

        // 關閉按鈕
        const closeBtn = document.createElement('button')
        closeBtn.textContent = '✕'
        closeBtn.style.position = 'absolute'
        closeBtn.style.top = '8px'
        closeBtn.style.right = '8px'
        closeBtn.style.background = 'rgba(0,0,0,0.6)'
        closeBtn.style.color = '#fff'
        closeBtn.style.border = 'none'
        closeBtn.style.borderRadius = '10px'
        closeBtn.style.padding = '6px 10px'
        closeBtn.style.cursor = 'pointer'

        // 動作區（Delete）
        const actions = document.createElement('div')
        actions.style.textAlign = 'center'

        const delBtn = document.createElement('button')
        delBtn.textContent = 'Delete'
        delBtn.style.background = '#e53935'
        delBtn.style.color = '#fff'
        delBtn.style.border = 'none'
        delBtn.style.padding = '8px 16px'
        delBtn.style.borderRadius = '8px'
        delBtn.style.cursor = 'pointer'

        // 判斷是否本人
        const isOwner = String(nft.userId) === String(window.currentUserId)

        if (!isOwner) {
            delBtn.disabled = true
            delBtn.style.opacity = '0.5'
            delBtn.style.cursor = 'not-allowed'
        }

        // 組裝
        frame.appendChild(img)
        frame.appendChild(closeBtn)
        actions.appendChild(delBtn)
        col.appendChild(frame)
        col.appendChild(actions)
        overlay.appendChild(col)
        document.body.appendChild(overlay)

        // 關閉事件
        closeBtn.addEventListener('click', (e) => { e.stopPropagation(); overlay.remove() })
        overlay.addEventListener('click', (e) => { if (e.target === overlay) overlay.remove() })

        // 刪除事件（只有本人時才綁定）
        if (isOwner) {
            delBtn.addEventListener('click', async () => {
                if (!confirm('確定要刪除這張 NFT 嗎？')) return

                const fileName = nft.fileName || (nft.filePath ? nft.filePath.split('/').pop() : '')
                if (!fileName) {
                    alert('缺少檔名，無法刪除')
                    return
                }

                try {
                    const response = await fetch(`/api/Nft/${encodeURIComponent(fileName)}`, {
                        method: 'DELETE',
                        credentials: 'include'
                    })
                    const data = await response.json().catch(() => ({}))

                    if (!response.ok || data?.success === false) {
                        throw new Error(data?.message || `HTTP ${response.status}`)
                    }

                    // 關閉視窗
                    overlay.remove()

                    // 取得被刪除的檔名（含副檔名）做一致比對
                    const deletedName = (fileName || '').toLowerCase()

                    // 1) 先「本地即時」移除，畫面立刻反應
                    try {
                        // More 的列表
                        if (Array.isArray(userNFTs?.value)) {
                            userNFTs.value = userNFTs.value.filter(x => {
                                const name = (x.fileName || (x.filePath ? x.filePath.split('/').pop() : '') || '').toLowerCase()
                                return name !== deletedName
                            })
                        }

                        // 九宮格的列表
                        if (Array.isArray(userImages?.value)) {
                            const filtered = userImages.value.filter(x => {
                                const name = (x.fileName || (x.filePath ? x.filePath.split('/').pop() : '') || '').toLowerCase()
                                return name !== deletedName
                            })
                            // 你有 padToNine → 補回 9 張
                            userImages.value = typeof padToNine === 'function' ? padToNine(filtered) : filtered
                        }

                        // 2) 重新渲染 More 視窗（如果正在 More）
                        const grid = document.querySelector('#nft-gallery-grid')
                        if (grid && typeof loadAllNFTs === 'function') {
                            await loadAllNFTs(grid)
                        }

                        // 3) 權威刷新（避免快取、確保跨裝置一致）
                        if (typeof GetNTFImages === 'function') {
                            await GetNTFImages(profile?.personId, 30)
                        }
                        if (typeof loadUserImages === 'function') {
                            await loadUserImages()
                        }
                    } catch (e) {
                        console.warn('Refresh after delete failed:', e)
                    }
                } catch (err) {
                    console.error('Delete failed:', err)
                    alert(`刪除失敗：${err.message || '請稍後再試'}`)
                }
            })
        }
    }





    // ====== 載入縮圖（純 CSS 正方形卡片；點了放大，X 在右上角） ======
    const loadAllNFTs = async (container) => {
        try {
            const response = await fetch('/api/Nft/images?count=100', {
                method: 'GET',
                credentials: 'include'
            })
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)

            const images = await response.json()

            if (!Array.isArray(images) || images.length === 0) {
                container.innerHTML = '<div class="nft-empty">尚無 NFT 作品</div>'
                return
            }

            // 先清空，再逐張建立卡片（不用 inline onclick，避免全域命名/快取問題）
            container.textContent = ''
            images.forEach((img) => {
                const card = document.createElement('button')
                card.type = 'button'
                card.className = 'nft-card'

                const im = document.createElement('img')
                im.src = img.filePath
                im.alt = img.fileName || ''
                card.appendChild(im)

                // 點縮圖 → （只有 X）
                card.addEventListener('click', (e) => {
                    e.stopPropagation()
                    showNFTDetailModal(img)
                })

                container.appendChild(card)
            })

            // 若其他地方需要用到清單
            window._nftGalleryImages = images
        } catch (err) {
            console.error('Failed to load all NFTs:', err)
            container.innerHTML = '<div class="nft-empty" style="color:#f66">載入失敗</div>'
        }
    }

    const formatFileSize = (bytes) => {
        if (bytes === 0) return '0 Bytes'
        const k = 1024
        const sizes = ['Bytes', 'KB', 'MB', 'GB']
        const i = Math.floor(Math.log(bytes) / Math.log(k))
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    }

    // Make viewNFTDetailFromGallery available globally for onclick events
    window.viewNFTDetailFromGallery = (fileId) => {
        const image = userImages.value.find(img => img.fileId === fileId)
        if (image) {
            showNFTDetailModal(image)
        }
    }


    //#endregion

    //#region File Handling

    const updateFile = type => {
        // console.log('updateFile called with type:', type)

        // 根據類型選擇對應的 input 元素
        let inputId
        if (type === 'avatar') {
            inputId = 'avatar-file-input'
        } else if (type === 'banner') {
            inputId = 'banner-file-input'
        } else {
            console.error('Invalid file type:', type)
            return
        }

        // 找到對應的 input 元素並觸發點擊
        const input = document.getElementById(inputId)
        if (input) {
            input.addEventListener('change', (event) => handleFileUpload(event, type), { once: true })
            input.click()
        } else {
            console.error('File input not found:', inputId)
        }
    }

    const handleFileUpload = async (event, type) => {
        const file = event.target.files[0]
        if (!file) return

        // 檢查是否為圖片
        if (!file.type.startsWith('image/')) {
            alert('請選擇圖片檔案')
            event.target.value = ''
            return
        }

        // 檢查檔案大小 (限制 5MB)
        const maxSize = 5 * 1024 * 1024 // 5MB
        if (file.size > maxSize) {
            alert('檔案大小不可超過 5MB')
            event.target.value = ''
            return
        }

        try {
            // 建立 FormData
            const formData = new FormData()
            formData.append('file', file)
            formData.append('type', type)

            // 發送上傳請求
            const response = await fetch('/api/Profile/upload-image', {
                method: 'POST',
                credentials: 'include',
                body: formData
            })

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const result = await response.json()

            if (result.success) {
                // 更新本地 profile 資料
                if (type === 'avatar') {
                    profile.avatarPath = result.data.filePath
                } else if (type === 'banner') {
                    profile.bannerPath = result.data.filePath
                }

                // 強制更新顯示
                rand.value = new Date().getTime()

                console.log(`${type} uploaded successfully:`, result.data.filePath)
            } else {
                throw new Error(result.message || '上傳失敗')
            }
        } catch (error) {
            console.error('File upload failed:', error)
            alert('上傳失敗，請稍後再試')
        } finally {
            // 清空 input 值，允許重複上傳同一檔案
            event.target.value = ''
        }
    }

    //#endregion

    //#region User & Profile Loading

    const GetPostsAsync = async (page = 1, append = false, pageSize = 10) => {
        // console.log(`載入第 ${page} 頁文章，每頁 ${pageSize} 篇`)

        if (isLoading.value || (!hasMorePosts.value && append)) return null

        // 服務可用性檢查
        if (!postListService) return null

        try {
            isLoading.value = true

            // 使用統一的 PostListService，傳遞 PersonId 作為 uid
            const result = await postListService.getPosts(
                page, // 現在 PostListService 使用 1-based 頁碼
                pageSize,
                profile.personId, // 使用 PersonId 作為 uid 參數
                true // isProfilePage = true
            )

            if (!result.success) {
                throw new Error(result.error || 'Failed to load posts')
            }

            const formattedArticles = postListService.formatArticles(result.articles).map(m => ({
                ...m,
                createTime: timeAgo(m.createTime),
                authorName: m.authorName,
                authorAvator: m.authorAvator,
            }))

            if (append) {
                // 追加新文章到現有列表
                posts.value = [...posts.value, ...formattedArticles]
            } else {
                // 替換整個列表（初次載入）
                posts.value = formattedArticles
            }

            counts.value = result.totalCount

            // 檢查是否還有更多文章
            const totalLoaded = posts.value.length
            hasMorePosts.value = totalLoaded < result.totalCount

            if (append) {
                currentPage.value = page
            }

            return result
        } catch (err) {
            console.error('載入文章失敗:', err)
            return null
        } finally {
            isLoading.value = false
        }
    }

    const loadProfile = async () => {
        try {
            // 設置載入狀態
            isLoading.value = true

            // 從網址判斷是否帶入 username：/profile/{username}
            const pathParts = window.location.pathname.split('/').filter(Boolean)
            const isProfilePath = pathParts[0]?.toLowerCase() === 'profile'
            const maybeUsername = isProfilePath && pathParts.length >= 2 ? pathParts[1] : null
            // 過濾掉可能的 Index 舊路由片段
            const username = (maybeUsername && maybeUsername.toLowerCase() !== 'index') ? maybeUsername : null

            const url = username ? `/api/Profile/${encodeURIComponent(username)}` : '/api/Profile'

            const response = await fetch(url, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (!response.ok) {
                // API 失敗處理
                if (response.status === 404) {
                    // 使用者不存在，顯示 alert 並跳轉到首頁
                    alert('查無此使用者')
                    window.location.href = '/'
                    return
                } else if (response.status === 401 || response.status === 403) {
                    // 權限問題，建議重新登入
                    alert('您的登入狀態已過期或無權限查看此頁面，請重新登入')
                    window.location.href = '/Auth/Login'
                    return
                } else if (response.status >= 500) {
                    // 伺服器錯誤，提供重試選項
                    const retry = confirm('伺服器暫時無法回應，是否要重新嘗試載入？')
                    if (retry) {
                        // 遞迴呼叫重新載入
                        setTimeout(() => loadProfile(), 1000)
                        return
                    } else {
                        alert('載入失敗，將返回首頁')
                        window.location.href = '/'
                        return
                    }
                } else if (response.status === 429) {
                    // 請求過於頻繁
                    alert('請求過於頻繁，請稍後再試')
                    setTimeout(() => {
                        window.location.href = '/'
                    }, 2000)
                    return
                } else {
                    // 其他未知錯誤
                    alert(`載入失敗 (錯誤代碼: ${response.status})，將返回首頁`)
                    window.location.href = '/'
                    return
                }
            }

            // 重置分頁狀態
            currentPage.value = 1
            hasMorePosts.value = true
            posts.value = []

            // 載入個人資料
            const data = await response.json()
            Object.assign(profile, data)
            isPublic.value = !data.isPrivate
        } catch (err) {
            console.error('載入 Profile 失敗:', err)
            // API 錯誤處理，顯示 alert 並跳轉到首頁
            alert('載入個人資料失敗，請稍後再試')
            window.location.href = '/'
        } finally {
            // 無論成功或失敗都要關閉載入狀態
            isLoading.value = false
        }
    }

    const loadUserImages = async () => {
        try {
            // 若已載入 profile，優先以該使用者的 UserId 查詢圖片
            const query = new URLSearchParams({ count: '10' })
            if (profile?.userId) {
                query.append('userId', profile.userId)
            }

            const response = await fetch(`/api/Profile/images?${query.toString()}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const images = await response.json()
            userImages.value = images
            // console.log('載入用戶圖片成功:', images.length, '張圖片')
        } catch (err) {
            console.error('載入用戶圖片失敗:', err)
            userImages.value = []
        }
    }

    // 載入更多文章
    const loadMorePosts = async () => {
        if (!hasMorePosts.value || isLoading.value) return

        const nextPage = currentPage.value + 1
        await GetPostsAsync(nextPage, true)
    }

    // 手動載入更多按鈕邏輯
    const setupManualLoad = () => {
        // 提供手動載入更多文章的按鈕功能
        const showLoadMoreButton = Vue.computed(() => {
            return hasMorePosts.value && !isLoading.value && posts.value.length > 0
        })

        return {
            showLoadMoreButton,
            loadMore: loadMorePosts
        }
    }
    //#endregion

    //#region Article Interaction
    // 使用統一的文章操作 hook
    const postActions = usePostActions()
    
    const stateFunc = async (type, aid) => {
        return await postActions.stateFunc(type, aid)
    }
    //#endregion

    //#region Lifecycle
    let manualLoadFunctions = null

    onMounted(async () => {
        await loadProfile()
        // 載入第一頁文章（限制 10 篇）
        await GetPostsAsync(1, false, 10)
        // 載入用戶圖片
        await loadUserImages()
        manualLoadFunctions = setupManualLoad()

        await GetNTFImages(profile?.personId, 30)

        // 設置無限滾動（Profile 頁面）
        Vue.nextTick(() => {
            if (window.globalApp && typeof window.globalApp.setupInfiniteScroll === 'function') {
                window.globalApp.setupInfiniteScroll(profile.personId, true)
            }
        })

        // 註冊為全域語言切換的回調函數
        if (!window.profileTranslationCallbacks) {
            window.profileTranslationCallbacks = []
        }
        window.profileTranslationCallbacks.push(updateEditPopupTranslations)
    })

    // 清理函數（如果需要）
    const cleanup = () => {
        // 清理無限滾動
        if (window.globalApp && typeof window.globalApp.cleanupInfiniteScroll === 'function') {
            window.globalApp.cleanupInfiniteScroll()
        }
    }
    //#endregion

    //#region Exposed API
    return {
        // Reactive State
        rand,
        createTime,
        content,
        editMode,
        isPublic,
        profile,
        updatProfile,
        posts,
        counts,
        isLoading,
        hasMorePosts,
        passwordValidation,
        showPassword,
        userImages,

        // Methods
        toggleIcon,
        cancel,
        startEdit,
        update,
        updateFile,
        loadProfile,
        loadUserImages,
        stateFunc,
        loadMorePosts,
        cleanup,
        validatePassword,
        validatePasswordWithDebounce,
        togglePasswordVisibility,

        // NFT Methods
        uploadNFT,
        viewAllNFTs,
        viewNFTDetail,
        GetNTFImages,
        userNFTs,

        // Manual load functions
        showLoadMoreButton: manualLoadFunctions?.showLoadMoreButton || Vue.ref(false),
        loadMore: manualLoadFunctions?.loadMore || (() => { })
    }
    //#endregion
}

// 為相容既有全域用法，仍掛到 window（ESM 匯入則使用 import）
window.useProfile = useProfile

export default useProfile
