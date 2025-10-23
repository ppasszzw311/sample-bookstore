# 簡單書店

以 ASP.NET Core Razor Pages 打造的示範書店後台與前台。專案完整實作 README 中列出的公開瀏覽與管理功能，並提供預先填充的資料方便立即體驗。

## 專案架構

- `BookStore/`：Razor Pages 網站專案
  - `Data/`：以 JSON 檔案為基礎的資料儲存與存取層
  - `Models/`：書籍、分類、訂閱等核心資料模型
  - `Pages/`：前台 (Index、Books、NewReleases) 與後台 (Admin) Razor Pages
  - `Services/`：相關書籍推薦服務
  - `App_Data/store.json`：預先建立的範例資料

## 開發環境

- .NET 8 SDK
- Razor Pages (ASP.NET Core)

若環境尚未安裝 .NET SDK，可參考 [官方安裝說明](https://learn.microsoft.com/dotnet/core/install/)。

## 執行方式

```bash
cd BookStore
dotnet restore
dotnet run
```

啟動後預設會使用 <http://localhost:5000>，瀏覽器將顯示前台首頁。

## 功能總覽

### 公開頁面

- **找書介面**：依分類篩選、關鍵字搜尋 (支援標籤)、多種排序選項。
- **查詢結果**：卡片式列表呈現書籍資訊與售價。
- **書目詳細**：顯示詳細說明、標籤、瀏覽次數並推薦相關書籍。
- **新書專區**：以時間區間篩選最新上架的書籍。
- **新書訂閱**：提交 Email 進行訂閱 (避免重複訂閱)，首頁同時顯示點閱排行。

### 後台管理

- **分類管理**：新增分類並列出現有分類清單。
- **書目管理**：新增單一本書、支援貼上 CSV 內容批次匯入、檢視書籍總表。
- **書目統計**：依分類統計書本數量、平均價格與價格區間，並提供整體摘要。

## CSV 批次匯入格式

匯入欄位順序：

```
Title, Author, Category, PublishedOn, Price, Description, CoverImageUrl, Tags
```

- `Category` 需對應既有分類名稱。
- `PublishedOn` 請使用可被 `DateTime.Parse` 辨識的格式 (例如 `2024/06/01`、`2024-06-01`)。
- `Tags` 可使用逗號分隔多個標籤。