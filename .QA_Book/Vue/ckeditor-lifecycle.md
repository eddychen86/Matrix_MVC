# 問題 23: CKEditor Vue 3 生命週期函數調用錯誤

**症狀**: Console 出現錯誤訊息：
```
[Vue warn]: onMounted is called when there is no active component instance to be associated with. Lifecycle injection APIs can only be used during execution of setup(). If you are using async setup(), make sure to register lifecycle hooks before the first await statement.
[Vue warn]: onBeforeUnmount is called when there is no active component instance to be associated with...
```

**原因**: CKEditor 的 `createCKEditor()` 函數中直接調用 `onMounted` 和 `onBeforeUnmount`，但這些生命週期函數必須在 Vue 3 的 `setup()` 函數或其他組合式函數內調用，不能在獨立的普通函數中使用。

**錯誤寫法**:
```javascript
// ckeditor5.js
export const createCKEditor = () => {
  const { ref, reactive, onMounted, onBeforeUnmount } = Vue
  
  // ❌ 在普通函數中直接調用生命週期函數
  onMounted(async () => {
    // 初始化邏輯
  })

  onBeforeUnmount(() => {
    // 清理邏輯  
  })
}
```

**正確寫法**:
```javascript
// ckeditor5.js
export const createCKEditor = () => {
  const { ref, reactive } = Vue
  
  // ✅ 返回初始化和清理函數，讓調用方在 setup() 中處理生命週期
  const initEditor = async (hostElement) => {
    // 初始化邏輯
  }
  
  const destroyEditor = () => {
    // 清理邏輯
  }
  
  return { 
    initEditor,
    destroyEditor,
    // 其他屬性...
  }
}

// create-post.js 中的正確調用方式
export function useCreatePost({ onCreated } = {}) {
  const { ref, onMounted, onBeforeUnmount } = Vue
  
  let ckEditorManager = null
  
  // ✅ 在 setup() 函數內調用生命週期函數
  onMounted(async () => {
    if (!window.ckEditorManager) {
      const { createCKEditor } = await import('/js/components/ckeditor5.js')
      ckEditorManager = createCKEditor()
      await ckEditorManager.initEditor()
      window.ckEditorManager = ckEditorManager
    }
  })
  
  onBeforeUnmount(() => {
    if (ckEditorManager) {
      ckEditorManager.destroyEditor()
    }
  })
}
```

**解決方案**: 
1. 將 `ckeditor5.js` 中的生命週期邏輯移除
2. 改為返回初始化和清理函數
3. 在使用該組合式函數的地方（如 `useCreatePost`）的 `setup()` 中處理生命週期

**相關檔案**: 
- `wwwroot/js/components/ckeditor5.js:2,105,150`
- `wwwroot/js/components/create-post.js:3`
- `Views/Shared/Components/CreatePostPopup/Default.cshtml:26-27`