# 問題 21: macOS 上的 NuGet 包路徑解析錯誤

**症狀**: 在 macOS 上執行 `dotnet build` 時出現錯誤：
```
error MSB4018: The "ResolvePackageAssets" task failed unexpectedly.
NuGet.Packaging.Core.PackagingException: Unable to find fallback package folder 'C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages'.
```

**原因**: NuGet 配置檔案中的 `activePackageSource` 設置了錯誤的包源（Visual Studio Marketplace），導致 .NET 嘗試在 macOS 上尋找 Windows 特定的路徑。

**錯誤配置**:
```xml
<activePackageSource>
  <add key="Visual Studio Marketplace" value="https://marketplace.visualstudio.com/vs" />
</activePackageSource>
```

**正確配置**:
```xml
<activePackageSource>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
</activePackageSource>
```

**解決方案**: 
1. 清除所有 NuGet 緩存：
   ```bash
   dotnet nuget locals all --clear
   ```

2. 移除專案編譯輸出：
   ```bash
   rm -rf bin obj
   ```

3. 修復 NuGet 配置檔案：
   - 編輯 `~/.nuget/NuGet/NuGet.Config`
   - 將 `activePackageSource` 從 Visual Studio Marketplace 改為 nuget.org

4. 驗證修復：
   ```bash
   dotnet restore
   dotnet build
   ```

**相關檔案**: `~/.nuget/NuGet/NuGet.Config`

**注意事項**:
- 此問題通常出現在跨平台開發或從 Windows 環境遷移的專案
- Visual Studio Marketplace 包源不適用於一般的 .NET 包管理
- 確保 `packageSources` 和 `activePackageSource` 指向同一個有效的包源