export function useCreatePost({ onCreated } = {}) {
    const { ref, onMounted } = Vue

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
    const maxSize = 5 * 1024 * 1024;

    const ClassicEditor = window.ClassicEditor

    const editorConfig = {
        placeholder: 'Write your post here...',
        toolbar: {
            items: [
                'heading', '|',
                'bold', 'italic', 'underline', 'link', '|',
                'bulletedList', 'numberedList', 'blockQuote', '|',
                'undo', 'redo'
            ]
        },

        removePlugins: [
            'ImageUpload',
            'CKFinder', 'CKFinderUploadAdapter',
            'CKBox', 'EasyImage',
            'AutoImage', 'ImageInsert',
            'MediaEmbed', 'MediaEmbedToolbar'
        ]
    }

    function htmlToText(html) {
        const el = document.createElement('div');
        el.innerHTML = html || '';
        const withBreaks = el.innerHTML
            .replace(/<\/p>\s*<p>/gi, '\n\n')
            .replace(/<br\s*\/?>/gi, '\n')
            .replace(/<\/?p[^>]*>/gi, '');
        el.innerHTML = withBreaks;
        return el.textContent || '';
    }

    function onEditorReady(editor) {

        editor.editing.view.document.on('clipboardInput', (evt, data) => {
            const dt = data.dataTransfer
            if (dt && (dt.files?.length || 0) > 0) {
                evt.stop()
            }
        })

        const editable = editor.ui.getEditableElement()
        if (!editable) return

        editable.addEventListener('dragover', (e) => {
            const hasFile = Array.from(e.dataTransfer?.items || []).some(i => i.kind === 'file')
            if (hasFile) e.preventDefault()
        })

        editable.addEventListener('drop', (e) => {
            if ((e.dataTransfer?.files?.length || 0) > 0) {
                e.preventDefault()
            }
        })
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

    const openModal = () => { showPostModal.value = true }
    const closeModal = () => { resetPostModal(); showPostModal.value = false }

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
        selectedHashtags.value = allHashtags.value.filter(t => set.has(String(t.tagId)))
        showHashtagModal.value = false
    }

    function toggleTempTag(tag) {
        const id = String(tag.tagId)
        const set = tempSelectedIds.value
        if (set.has(id)) set.delete(id)
        else set.add(id)
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
        formData.append('Content', htmlToText(postContent.value))
        formData.append('IsPublic', '0')
        selectedImages.value.forEach(f => formData.append('Attachments', f))
        selectedFiles.value.forEach(f => formData.append('Attachments', f))
        selectedHashtags.value.forEach(tag => formData.append('SelectedHashtags', tag.tagId))

        try {
            const res = await fetch('/CreatePost/Create', { method: 'POST', body: formData })
            if (res.ok) {
                const article = await res.json()
                afterCreated?.(article)
                alert('送出成功！')
                closeModal()
            } else {
                const error = await res.text()
                alert('送出失敗: ' + error)
            }
        } catch (err) {
            alert('網路錯誤：' + err.message)
        }
    }

    onMounted(() => {
        const btn = document.querySelector('#openPostBtn')
        if (btn) btn.addEventListener('click', openModal, { once: false })
    })

    return {
        postContent, showPostModal, showHashtagModal,
        allHashtags, selectedHashtags, tempSelectedIds,
        selectedImages, selectedFiles, fileInput, fileInputMode,
        openModal, closeModal,
        openHashtagModal, cancelHashtagModal, confirmHashtagModal,
        toggleTempTag,
        setFileInput, handleFileChange, submitPost,
        truncateFilename, safeURL,
        ClassicEditor, editorConfig, onEditorReady,
        htmlToText, removeImage, removeFile, afterCreated
    }
}
