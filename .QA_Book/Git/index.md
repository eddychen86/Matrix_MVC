# Git 版本控制問題目錄

本目錄包含 Git 版本控制相關的問題解決方案。

## 📋 問題清單

### 問題 4: 搜尋所有分支中包含特定關鍵字的內容
**檔案**: [`git-srch.md`](./git-srch.md)  
**描述**: 在所有 git 分支中搜尋特定關鍵字的各種方法  
**關鍵字**: git, 分支搜尋, 關鍵字, git log, git grep  
**適用範圍**: 適用於任何 git 專案

### 問題 5: 從舊 commit 選擇性合併特定檔案到當前分支
**檔案**: [`git-file.md`](./git-file.md)  
**描述**: 從歷史 commit 中恢復或合併特定檔案的方法  
**關鍵字**: git, checkout, cherry-pick, restore, 檔案恢復  
**適用範圍**: 適用於任何 git 專案

---

## 🔍 快速搜尋

### 操作類型
- **搜尋操作**: 問題 4
  - `git log --all --grep`
  - `git log --all -S`
  - `git grep`
- **檔案操作**: 問題 5
  - `git checkout <commit> -- <file>`
  - `git restore --source=<commit>`
  - `git cherry-pick -n`

### 使用場景
- **代碼考古**: 問題 4 (尋找特定功能的歷史)
- **版本回滾**: 問題 5 (恢復特定檔案)
- **分支管理**: 問題 4, 5 (跨分支操作)

### 命令分類
- **查詢命令**: `git log`, `git grep`, `git show`
- **操作命令**: `git checkout`, `git restore`, `git cherry-pick`
- **分支命令**: `git branch`, `git log --all`

## 🛠️ 常用命令速查

### 搜尋相關
```bash
# 搜尋 commit 訊息
git log --all --grep="關鍵字" -i --oneline

# 搜尋檔案內容變更
git log --all -S"關鍵字" -i --oneline

# 搜尋當前分支檔案內容
git grep -i "關鍵字"
```

### 檔案恢復相關
```bash
# 從特定 commit 恢復檔案
git checkout <commit-hash> -- <檔案路徑>

# 使用 restore (Git 2.23+)
git restore --source=<commit-hash> <檔案路徑>

# 查看檔案歷史版本
git show <commit-hash>:<檔案路徑>
```

## 📊 統計

- **總問題數**: 2
- **已解決**: 2
- **搜尋相關**: 1
- **檔案操作**: 1
- **適用版本**: Git 2.0+
- **最後更新**: 2025-08-25