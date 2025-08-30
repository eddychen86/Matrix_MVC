import { postListService } from './PostListService.js'

export function useCreatePost({ onCreated } = {}) {
    const { ref, reactive, computed } = Vue

    const URL_API = (typeof window !== 'undefined')
        ? (window.URL || window.webkitURL || null)
        : null

    const postContent = ref('')
    const showPostModal = ref(false)
    const showHashtagModal = ref(false)

    const allHashtags = ref([])
    const selectedHashtags = ref([])
    const tempSelectedIds = ref(new Set())

    const selectedImages = ref([])
    const selectedFiles = ref([])
    const fileInput = ref(null)
    const fileInputMode = ref('file')
    const maxSize = 5 * 1024 * 1024
    const maxTags = 6

    // 備用的 htmlToText 函數，當 ckEditorManager 未初始化時使用
    function htmlToText(html) {
        const el = document.createElement('div')
        el.innerHTML = html || ''
        const withBreaks = el.innerHTML
            .replace(/<\/p>\s*<p>/gi, '\n\n')
            .replace(/<br\s*\/?>/gi, '\n')
            .replace(/<\/?p[^>]*>/gi, '')
        el.innerHTML = withBreaks
        return el.textContent || ''
    }

    function truncateFilename(name) {
        const hasChinese = /[^\x00-\x7F]/.test(name)
        return hasChinese
            ? (name.length > 5 ? name.slice(0, 5) + '…' : name)
            : (name.length > 10 ? name.slice(0, 10) + '…' : name)
    }

    function safeURL(file) {
        if (!file || !(file instanceof File)) return ''
        if (file.__previewURL) return file.__previewURL
        if (!URL_API || typeof URL_API.createObjectURL !== 'function') return ''
        try {
            file.__previewURL = URL_API.createObjectURL(file)
            return file.__previewURL
        } catch {
            return ''
        }
    }

    function revokeAllPreviews() {
        ;[...selectedImages.value, ...selectedFiles.value].forEach(f => {
            if (f && f.__previewURL && URL_API?.revokeObjectURL) {
                try { URL_API.revokeObjectURL(f.__previewURL) } catch { }
                f.__previewURL = null
            }
        })
    }

    function resetPostModal() {
        revokeAllPreviews()
        postContent.value = ''
        selectedFiles.value = []
        selectedImages.value = []
        selectedHashtags.value = []
        tempSelectedIds.value = new Set()
        if (fileInput.value) fileInput.value.value = ''
    }

    const openModal = async () => { 
        showPostModal.value = true
        
        // 確保 ckEditorManager 已初始化
        if (!window.ckEditorManager) {
            try {
                const { createCKEditor } = await import('/js/components/ckeditor5.js')
                if (typeof createCKEditor === 'function') {
                    window.ckEditorManager = createCKEditor()
                    console.log('CKEditor 管理器已在開啟彈窗時初始化')
                }
            } catch (error) {
                console.error('CKEditor 管理器初始化失敗:', error)
            }
        }
        
        // Vue 的 v-if 會自動控制顯示狀態，不需要手動移除 display: none
    }
    const closeModal = () => { 
        resetPostModal(); 
        showPostModal.value = false 
    }

    async function fetchHashtags() {
        if (allHashtags.value.length > 0) return
        try {
            const res = await fetch('/CreatePost/GetHashtags')
            const raw = await res.json()
            allHashtags.value = (raw || []).map(x => ({
                tagId: String(x.tagId ?? x.TagId ?? x.id ?? x.ID),
                content: String(x.content ?? x.Content ?? x.name ?? x.Name ?? '')
            }))
        } catch (err) {
            console.error('標籤載入失敗', err)
        }
    }

    const openHashtagModal = async () => {
        await fetchHashtags()
        tempSelectedIds.value = new Set(selectedHashtags.value.map(t => String(t.tagId)))
        showHashtagModal.value = true
    }
    const cancelHashtagModal = () => { showHashtagModal.value = false }
    const confirmHashtagModal = () => {
        const set = tempSelectedIds.value
        if (set.size > maxTags) {
            alert(`最多只能選擇${maxTags}個標籤`)
            return
        }
        selectedHashtags.value = allHashtags.value.filter(t => set.has(String(t.tagId)))
        showHashtagModal.value = false
    }
    function toggleTempTag(tag, evt) {
        const id = String(tag.tagId)
        const set = tempSelectedIds.value

        if (set.has(id)) {
            set.delete(id)
            if (evt?.target) evt.target.checked = false
            return
        }
        if (set.size >= maxTags) {
            alert(`最多只能選擇${maxTags}個標籤`)
            if (evt?.target) evt.target.checked = false
            return
        }
        set.add(id)
        if (evt?.target) evt.target.checked = true
    }

    function setFileInput(type) {
        fileInputMode.value = type
        if (!fileInput.value) return
        fileInput.value.value = ''
        fileInput.value.accept = (type === 'image')
            ? 'image/*'
            : '.pdf,.docx,.doc,.ppt,.pptx,.xls,.xlsx,.txt,.zip,.rar'
        fileInput.value.click()
        return true
    }

    function dedupe(files) {
        return files.filter((file, idx, arr) =>
            arr.findIndex(f => f.name === file.name && f.size === file.size) === idx
        )
    }
    function looksLikeImage(file) {
        const ct = (file.type || '').toLowerCase()
        const byCT = ct.startsWith('image/')

        const ext = (file.name.split('.').pop() || '').toLowerCase()
        const byExt = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'heic', 'heif'].includes(ext)

        return byCT || byExt
    }

    function handleFileChange(e) {
        const files = Array.from(e.target?.files || []).filter(f => f instanceof File)
        if (!files.length) return

        for (const f of files) {
            if (f.size > maxSize) {
                alert(`檔案 ${f.name} 超過 5MB，請重新選擇`)
                if (fileInput.value) fileInput.value.value = ''
                return
            }
        }

        const images = files.filter(looksLikeImage)
        const nonImages = files.filter(f => !looksLikeImage(f))

        if (fileInputMode.value === 'image') {
            if (nonImages.length) { alert('僅限選擇圖片'); return }
            if (selectedImages.value.length + images.length > 6) {
                alert('圖片最多只能上傳 6 張')
                if (fileInput.value) fileInput.value.value = ''
                return
            }
            selectedImages.value = dedupe([...selectedImages.value, ...images])
        } else {
            if (images.length) { alert('僅限選擇檔案'); return }
            if (selectedFiles.value.length + nonImages.length > 6) {
                alert('檔案最多只能上傳 6 個')
                if (fileInput.value) fileInput.value.value = ''
                return
            }
            selectedFiles.value = dedupe([...selectedFiles.value, ...nonImages])
        }

        if (fileInput.value) fileInput.value.value = ''
    }

    function afterCreated(article) {
        try {
            if (typeof onCreated === 'function') {
                onCreated(article);
            } else {
                window.dispatchEvent(new CustomEvent('post:created', { detail: article }));
            }
        }
        catch (err) {
            console.error('afterCreated error:', err);
        }
    }

    function removeImage(index) {
        selectedImages.value.splice(index, 1);
    }

    function removeFile(index) {
        selectedFiles.value.splice(index, 1);
    }

    async function submitPost() {
        if (!postContent.value.trim()) { alert('文章內容不得為空'); return }

        const formData = new FormData()
        // 優先使用 ckEditorManager 中的 htmlToText，否則使用備用函數
        const textConverter = window.ckEditorManager?.htmlToText || htmlToText
        formData.append('Content', textConverter(postContent.value))
        formData.append('IsPublic', '0')
        selectedImages.value.forEach(f => formData.append('Attachments', f))
        selectedFiles.value.forEach(f => formData.append('Attachments', f))
        selectedHashtags.value.forEach(tag => formData.append('SelectedHashtags', tag.tagId))

        // 👇 這段把 FormData 內容印出來
        const dump = []
        for (const [k, v] of formData.entries()) {
            dump.push([k, v instanceof File ? `(File:${v.name}, ${v.size}B)` : v])
        }
        // console.log('[submit] POST /CreatePost/Create payload =', dump)

        try {
            const res = await fetch('/CreatePost/Create', { method: 'POST', body: formData })
            // console.log('[submit] response status =', res.status)

            if (!res.ok) {
                const txt = await res.text()
                console.error('[submit] error body =', txt)
                alert('送出失敗: ' + (txt?.slice(0, 200) || res.status))
                return
            }

            const article = await res.json()
            // console.log('[submit] success article =', article)

            // 觸發貼文列表局部刷新
            try {
                // 格式化新貼文數據以符合前端顯示格式
                const formattedArticle = postListService.formatArticles([article])[0];

                // console.log('準備觸發 post:listRefresh 事件', { formattedArticle });

                // 方法1: 本地事件 - 立即更新發文者自己的列表
                window.dispatchEvent(new CustomEvent('post:listRefresh', {
                    detail: {
                        action: 'prepend',
                        newArticle: formattedArticle,
                        rawArticle: article,
                        source: 'local' // 標記為本地事件
                    }
                }));

                // 方法2: SignalR - 通知其他用戶
                if (window.matrixSignalR && window.matrixSignalR.isConnected) {
                    const signalRSuccess = await window.matrixSignalR.notifyNewPost({
                        articleId: article.articleId,
                        authorId: article.authorId || article.userId,
                        authorName: article.authorName || article.userName,
                        content: article.content,
                        createTime: article.createTime,
                        formattedArticle: formattedArticle
                    });

                    if (signalRSuccess) {
                        // console.log('SignalR 新貼文通知已發送');
                    } else {
                        console.warn('SignalR 新貼文通知發送失敗');
                    }
                } else {
                    console.warn('SignalR 連接未建立，無法通知其他用戶');
                }

                // console.log('post:listRefresh 事件已觸發');
            } catch (refreshError) {
                console.warn('刷新貼文列表失敗:', refreshError);
            }

            afterCreated?.(article)
            alert('送出成功！')
            closeModal()
        } catch (err) {
            console.error('[submit] network error =', err)
            alert('網路錯誤：' + err.message)
        }
    }


    return {
        postContent, showPostModal, showHashtagModal,
        allHashtags, selectedHashtags, tempSelectedIds,
        selectedImages, selectedFiles, fileInput, fileInputMode,
        openModal, closeModal,
        openHashtagModal, cancelHashtagModal, confirmHashtagModal,
        toggleTempTag,
        setFileInput, handleFileChange, submitPost,
        truncateFilename, safeURL,
        
        removeImage, removeFile, afterCreated,
        
        // TODO(human): CKEditor 相關屬性需要暴露給模板使用
        // 從 window.ckEditorManager 獲取，如果不存在則提供預設值
        ClassicEditor: computed(() => window.ckEditorManager?.ClassicEditor || window.ClassicEditor || null),
        editorConfig: computed(() => window.ckEditorManager?.editorConfig || null),
        onEditorReady: computed(() => window.ckEditorManager?.onEditorReady || (() => {}))
    }
}
