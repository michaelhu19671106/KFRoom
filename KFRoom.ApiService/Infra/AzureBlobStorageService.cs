using Azure.Storage.Blobs;
namespace KFRoom.ApiService.Infra;
public class AzureBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    // 注入組態設定物件，以建立BlobServiceClient物件
    public AzureBlobStorageService(BlobServiceClient blobServiceClient) //IConfiguration config)
    {
        //var connectionString = config["ConnectionStrings:blobs"];
        //if (string.IsNullOrWhiteSpace(connectionString))
        //{
        //    throw new InvalidOperationException("找不到 Blob Storage 連線字串 'blobs'。");
        //}

        //_blobServiceClient = new BlobServiceClient(connectionString);
        ArgumentNullException.ThrowIfNull(blobServiceClient);
        _blobServiceClient = blobServiceClient;
    }
    // 上傳檔案至Azure Blob Storage
    public async Task UploadFileAsync(Stream file, string containerName, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(file, overwrite: true);
    }
}
