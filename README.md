# KFRoom

KFRoom 是一個以 **.NET 10** 建立的教學範例，採用 **Blazor Web App** 作為前端、**ASP.NET Core Minimal API** 作為後端，並以 **.NET Aspire** 統整本機開發時的服務啟動與相依管理。

目前方案聚焦於會員相關流程，包含：
- 會員註冊
- 會員登入
- 會員頭像上傳至 Blob Storage
- JWT 存取權杖產生

## 方案架構

```text
KFRoom.AppHost          Aspire 啟動入口，負責協調 Web、API 與 Storage
KFRoom.Web              Blazor 前端網站
KFRoom.ApiService       後端 API 與商業邏輯
KFRoom.Model            前後端共用的 DTO / Request / Response Model
KFRoom.ServiceDefaults  Aspire 共用服務設定
KFRoom.Tests            測試專案
KFRoom.UnitTest         測試專案
```

## 主要技術

- .NET 10
- Blazor Web App
- ASP.NET Core Minimal API
- .NET Aspire
- Azure Blob Storage
- Dapper
- SQL Server
- JWT
- Blazor Bootstrap

## 目前功能

### 前端頁面
- `/` 首頁
- `/Registration` 會員註冊
- `/Login` 會員登入

### 後端 API
- `POST /Account/AddMemberAsync`：新增會員，支援表單與頭像上傳
- `POST /Account/LoginAsync`：驗證帳密並回傳 JWT

## 專案說明

### KFRoom.AppHost
Aspire 啟動專案，會：
- 啟動 Azure Storage Emulator
- 建立 Blob 資源 `blobs`
- 啟動 `KFRoom.ApiService`
- 啟動 `KFRoom.Web`
- 管理服務間相依順序與健康檢查

### KFRoom.Web
Blazor 前端專案，負責：
- 顯示首頁、註冊、登入頁面
- 呼叫後端 API
- 將登入成功後的 JWT 暫存於瀏覽器 sessionStorage

### KFRoom.ApiService
後端 API 專案，負責：
- 會員註冊流程
- 密碼雜湊與 Salt 處理
- 會員登入驗證
- JWT 產生
- Blob Storage 頭像上傳
- 透過 Dapper 存取 SQL Server

### KFRoom.Model
放置共用資料模型，例如：
- `RegistrationModel`
- `LoginModel`
- `MemberDTO`
- `StandardResponse`
- `LoginResponse`

### KFRoom.ServiceDefaults
集中 Aspire 共用設定，例如：
- Service Discovery
- Resilience
- OpenTelemetry
- Health Check 預設整合

## 執行需求

建議準備以下環境：
- Visual Studio 2026 或相容版本
- .NET 10 SDK
- SQL Server
- 可執行 Azurite 的本機環境

## 本機執行方式

### 方式一：使用 Visual Studio
1. 開啟 `KFRoom.slnx`
2. 將 `KFRoom.AppHost` 設為啟動專案
3. 執行方案

### 方式二：使用命令列
在方案根目錄執行：

`dotnet run --project KFRoom.AppHost`

## 設定說明

### SQL Server
`KFRoom.ApiService/appsettings.Development.json` 內目前使用 `ConnectionStrings:sqldb` 作為資料庫連線字串。

請依本機環境調整，例如：
- Server
- Database
- User ID
- Password
- TrustServerCertificate

此外，API 目前透過 Dapper 呼叫以下預存程序：
- `sp_AddMember`
- `sp_GetMemberSalt`
- `sp_GetMemberByEmailPassword`

因此資料庫需先建立對應資料表與預存程序。

### Blob Storage
會員頭像預設使用 Blob Storage，容器與預設檔名設定於：
- `KFRoom.ApiService/appsettings.json`

目前設定：
- Container：`kfroom-member-avatar`
- Default file：`default_avatar.png`

### JWT
JWT 設定位於：
- `KFRoom.ApiService/appsettings.Development.json`

包含：
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Expires`

## 開發重點

此方案適合作為以下主題的教學範例：
- Blazor 表單輸入與驗證
- 檔案上傳
- Blazor 呼叫 Web API
- 密碼雜湊與 Salt
- JWT 驗證流程
- Dapper 存取 SQL Server
- Aspire 管理分散式應用的本機開發環境

## 注意事項

- 若未完成 SQL Server 與預存程序準備，註冊與登入流程將無法正常運作

## 授權

此專案目前未提供授權條款。
