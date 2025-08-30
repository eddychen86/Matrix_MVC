export const createCKEditor = () => {
  const { ref, reactive } = Vue

  const editorHost = ref(null)           // 編輯器掛載的 DOM
  const editorInstance = ref(null)       // 保存 editor 實例
  const value = ref('')                  // 你的內容（HTML）
  const maxChars = 3000                  // 字數上限（純文字）
  const remaining = ref(maxChars)        // 剩餘可輸入字數

  // 從 create-post.js 移植的設定
  const ClassicEditor = window.ClassicEditor
  const editorConfig = reactive({
    placeholder: '請輸入文字（最多 3000 字）…',
    toolbar: {
      items: [
        'heading', '|',
        'bold', 'italic', 'link', '|',
        'bulletedList', 'numberedList', 'blockQuote', '|',
        'undo', 'redo'
      ],
      shouldNotGroupWhenFull: false
    },
    ui: {
      viewportOffset: {
        top: 0,
        right: 0,
        bottom: 0,
        left: 0
      }
    },
    // Lark Theme 相關設定
    language: 'zh-tw',
    removePlugins: [
      'CKFinder', 'CKFinderUploadAdapter',
      'CKBox', 'EasyImage',
      'AutoImage', 'ImageInsert',
      'MediaEmbed', 'MediaEmbedToolbar'
    ]
  })

  // HTML 轉純文字
  const stripHtml = (html) => {
    const tmp = document.createElement('div')
    tmp.innerHTML = html
    return (tmp.textContent || tmp.innerText || '').replace(/\s+/g, ' ').trim()
  }

  // 從 create-post.js 移植的 htmlToText 函數
  const htmlToText = (html) => {
    const el = document.createElement('div')
    el.innerHTML = html || ''
    const withBreaks = el.innerHTML
      .replace(/<\/p>\s*<p>/gi, '\n\n')
      .replace(/<br\s*\/?>/gi, '\n')
      .replace(/<\/?p[^>]*>/gi, '')
    el.innerHTML = withBreaks
    return el.textContent || ''
  }

  const updateRemaining = (html) => {
    remaining.value = maxChars - stripHtml(html).length
  }

  // 從 create-post.js 移植的 onEditorReady 函數
  const onEditorReady = (editor) => {
    // 外框 (含工具列)
    editor.ui.view.element.classList.add('my-editor-frame')

    // 內容區（真正輸入的地方）- 移除高度相關 class，讓 CSS 控制
    editor.ui.view.editable.element.classList.add(
      'rounded-[10px]',
      'bg-transparent'
    )
    
    // 移除任何內聯樣式，確保 CSS 可以控制高度
    const editable = editor.ui.getEditableElement()
    if (editable) {
      editable.style.minHeight = ''
      editable.style.maxHeight = ''
      editable.style.height = ''
    }

    editor.editing.view.document.on('clipboardInput', (evt, data) => {
      const dt = data.dataTransfer
      if (dt && (dt.files?.length || 0) > 0) {
        evt.stop()
      }
    })

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

  const initEditor = async (hostElement) => {
    if (!hostElement) return
    if (!ClassicEditor) {
      console.warn('ClassicEditor 未載入，跳過 CKEditor 初始化')
      return
    }

    editorHost.value = hostElement

    // 建一個可編輯元素，避免在 .cshtml 寫任何參數
    const mountPoint = document.createElement('div')
    editorHost.value.appendChild(mountPoint)

    try {
      // 使用整合後的設定初始化 CKEditor
      const editor = await ClassicEditor.create(mountPoint, editorConfig)
      editorInstance.value = editor

      // 執行 ready 設定
      onEditorReady(editor)

      // 同步初始值
      value.value = editor.getData()
      updateRemaining(value.value)

      // 監聽變更：超過上限就截斷
      editor.model.document.on('change:data', () => {
        const html = editor.getData()
        const plain = stripHtml(html)
        if (plain.length > maxChars) {
          // 以純文字安全截斷，避免切壞標籤
          const truncated = plain.slice(0, maxChars)
          editor.model.change(writer => {
            editor.setData(truncated)
            // 把游標移到文末，減少閃動感
            const endPos = writer.createPositionAt(editor.model.document.getRoot(), 'end')
            writer.setSelection(endPos)
          })
        }
        value.value = editor.getData()
        updateRemaining(value.value)
      })
    } catch (error) {
      console.error('CKEditor 初始化失敗:', error)
    }
  }

  const destroyEditor = () => {
    if (editorInstance.value) {
      editorInstance.value.destroy().catch(() => {})
      editorInstance.value = null
    }
  }

  return { 
    editorHost, 
    value, 
    remaining, 
    maxChars, 
    ClassicEditor, 
    editorConfig, 
    onEditorReady, 
    htmlToText,
    initEditor,
    destroyEditor
  }
}

// 預設導出
export default createCKEditor