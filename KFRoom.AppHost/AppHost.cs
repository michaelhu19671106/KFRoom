using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
//// 建立SQL Database 資源
//var sql = builder.AddSqlServer("sql");
//var sqldb = sql.AddDatabase("sqldb");
// 建立模擬的 Azure Storage 資源與 Blob 子資源
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var blobs = storage.AddBlobs("blobs");
// 在 apiService 專案中加入對 Blob Storage 的依賴，並在啟動前確保相依資源已就緒
var apiService = builder.AddProject<Projects.KFRoom_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WaitFor(blobs)
    .WithReference(blobs);
    //.WaitFor(sqldb)
    //.WithReference(sqldb);
// 這是樣版專案內建
builder.AddProject<Projects.KFRoom_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
