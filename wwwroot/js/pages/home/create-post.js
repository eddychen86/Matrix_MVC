const createPost = () => {
    const { ref, onMounted } = Vue

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

    function truncateFilename(name) {
        const hasChinese = /[^\x00-\x7F]/.test(name)
        return hasChinese
            ? (name.length > 5 ? name.slice(0, 5) + '…' : name)
            : (name.length > 10 ? name.slice(0, 10) + '…' : name)
    }

    function safeURL(file) {
        if (!file.__previewURL) {
            try { file.__previewURL = URL.createObjectURL(file) } catch { file.__previewURL = '' }
        }
        return file.__previewURL
    }

    function revokeAllPreviews() {
        [...selectedImages.value, ...selectedFiles.value].forEach(f => {
            if (f && f.__previewURL) {
                try { URL.revokeObjectURL(f.__previewURL) } catch { }
                f.__previewURL = null
            }
        })
    }

    function resetPostModal() {
        revokeAllPreviews()
        postContent.value = ''
        selectedFiles.value = []
        selectedImages.value = []
        tempSelectedIds.value = new Set()
        if (fileInput.value) fileInput.value.value = ''
    }

    const openModal = () => {
        showPostModal.value = true
    }
    const closeModal = () => {
        resetPostModal()
        showPostModal.value = false
    }

    async function fetchHashtags() {
        if (allHashtags.value.length > 0) return
        try {
            const res = await fetch('/Post/GetHashtags')
            allHashtags.value = await res.json()
        } catch (err) {
            console.error('標籤載入失敗', err)
        }
    }

    const openHashtagModal = async () => {
        await fetchHashtags()
        tempSelectedIds.value = new Set(selectedHashtags.value.map(t => String(t.tagId)))
        showHashtagModal.value = true
    }
    const cancelHashtagModal = () => {
        showHashtagModal.value = false
    }
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

    function handleFileChange(e) {
        const files = Array.from(e.target.files).filter(f => f instanceof File)
        const images = files.filter(f => f.type.startsWith('image/'))
        const nonImages = files.filter(f => !f.type.startsWith('image/'))

        if (fileInputMode.value === 'image') {
            if (nonImages.length) { alert('僅限選擇圖片檔案'); return }
            selectedImages.value = dedupe([...selectedImages.value, ...images])
        } else {
            if (images.length) { alert('請勿在檔案欄選擇圖片'); return }
            selectedFiles.value = dedupe([...selectedFiles.value, ...nonImages])
        }
    }

    async function submitPost() {
        if (!postContent.value.trim()) {
            alert('文章內容不得為空'); return
        }
        const formData = new FormData()
        formData.append('Content', postContent.value)
        formData.append('IsPublic', '0')

        selectedImages.value.forEach(f => formData.append('Attachments', f))
        selectedFiles.value.forEach(f => formData.append('Attachments', f))
        selectedHashtags.value.forEach(tag => formData.append('SelectedHashtags', tag.tagId))

        try {
            const res = await fetch('/Post/Create', { method: 'POST', body: formData })
            if (res.ok) {
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
        // state
        postContent, showPostModal, showHashtagModal,
        allHashtags, selectedHashtags, tempSelectedIds,
        selectedImages, selectedFiles, fileInput, fileInputMode,
        // actions
        openModal, closeModal,
        openHashtagModal, cancelHashtagModal, confirmHashtagModal,
        toggleTempTag,
        setFileInput, handleFileChange, submitPost,
        // utils
        truncateFilename, safeURL
    }
}
 