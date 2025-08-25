# 問題 1: PermissionService 不是函數錯誤

**症狀**: 在 `config.js:274` 行出現 "PermissionService is not a function" 錯誤

**原因**: `PermissionService` 是定義為物件，包含多個方法，不是可直接呼叫的函數

**錯誤寫法**:
```javascript
await PermissionService()  // ❌ 試圖呼叫物件當作函數
```

**正確寫法**:
```javascript
// 移除錯誤的呼叫，或呼叫特定方法：
await PermissionService.getUserPermissions()  // ✅ 呼叫物件的方法
```

**解決方案**: 
- 如果不需要初始化，直接移除 `await PermissionService()` 這行
- 如果需要初始化，呼叫具體的方法如 `getUserPermissions()`

---

## JavaScript 物件 vs 函數
- **物件**: 包含多個方法的容器，如 `PermissionService = { method1(), method2() }`
- **函數**: 可直接執行的程式碼，如 `function doSomething() {}`
- **使用**: 物件需要透過 `.` 語法呼叫其方法

## 服務物件模式
- 將相關功能分組到單一物件中
- 按需呼叫特定方法，避免不必要的初始化
- 保持方法的單一職責原則