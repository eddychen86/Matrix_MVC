import authService from '/js/services/AuthService.js'

export const useCreatePost = () => {
  const { ref } = Vue

  // 狀態
  const content = ref('')
  const isSubmitting = ref(false)
  const lastError = ref('')
  const lastSuccess = ref('')

  // 內部：確認已登入
  const ensureAuthenticated = async () => {
    try {
      const auth = await authService.getAuthStatus()
      return !!(auth?.success && auth?.data?.authenticated)
    } catch {
      return false
    }
  }

  // 送出貼文（僅文字）
  const submitPost = async () => {
    lastError.value = ''
    lastSuccess.value = ''

    const ok = await ensureAuthenticated()
    if (!ok) {
      alert('請先登入以新增貼文')
      return { success: false, message: 'unauthenticated' }
    }

    const text = (content.value || '').trim()
    if (!text) {
      lastError.value = '內容不得為空'
      return { success: false, message: 'empty_content' }
    }

    isSubmitting.value = true
    try {
      const formData = new FormData()
      formData.append('Content', text)

      const res = await fetch('/Post/Create', {
        method: 'POST',
        credentials: 'include',
        body: formData
      })

      if (!res.ok) {
        const msg = await res.text().catch(() => '')
        lastError.value = msg || `送出失敗 (${res.status})`
        return { success: false, message: lastError.value }
      }

      // 成功
      lastSuccess.value = '貼文已送出'
      content.value = ''
      return { success: true }
    } catch (err) {
      lastError.value = err?.message || '發生未知錯誤'
      return { success: false, message: lastError.value }
    } finally {
      isSubmitting.value = false
    }
  }

  return {
    // state
    content,
    isSubmitting,
    lastError,
    lastSuccess,
    // actions
    submitPost,
  }
}

export default useCreatePost

