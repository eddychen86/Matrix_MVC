# 問題 6: Vue 3 響應式物件重置錯誤

**症狀**: 在 Vue 3 中使用 `reactive()` 創建的物件，執行重置函數時報錯或失去響應式

**原因**: 直接對 `reactive()` 物件進行賦值會丟失響應式代理參照

**錯誤寫法**:
```javascript
const formData = reactive({ userName: '', email: '' })

const resetFormData = () => {
  formData = {  // ❌ 直接賦值會失去響應式
    userName: '',
    email: ''
  }
}
```

**正確寫法**:
```javascript
const formData = reactive({ userName: '', email: '' })

// 方法1: 個別更新屬性
const resetFormData = () => {
  formData.userName = ''
  formData.email = ''
}

// 方法2: 使用 Object.assign
const resetFormData = () => {
  Object.assign(formData, {
    userName: '',
    email: ''
  })
}
```

**ref() 的差異**:
```javascript
// ref 可以重新賦值 .value
const formData = ref({ userName: '', email: '' })

const resetFormData = () => {
  formData.value = {  // ✅ ref 可以這樣重置
    userName: '',
    email: ''
  }
}
```

**解決方案**: 
- `reactive()` 物件：必須個別更新屬性或使用 `Object.assign()`
- `ref()` 物件：可以直接重新賦值 `.value` 或更新個別屬性
- 選擇 `reactive()` 適合複雜物件，`ref()` 適合簡單值或整個物件替換的場景

**相關檔案**: `/wwwroot/js/dashboard/pages/config/config.js:135-145`