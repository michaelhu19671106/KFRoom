using Microsoft.AspNetCore.Components.Forms;

namespace KFRoom.UnitTest.TestHelpers;

// 模擬的 IBrowserFile 實現，用於測試中模擬文件上傳
public class FakeBrowserFile : IBrowserFile
{
    // 模擬的檔案資料，使用 MemoryStream 來提供檔案內容
    private readonly byte[] _data;
    private readonly MemoryStream _stream;

    // 建構函數，初始化檔案名稱、內容類型和檔案資料
    public FakeBrowserFile(string name, string contentType, byte[] data)
    {
        Name = name;
        ContentType = contentType;
        _data = data ?? Array.Empty<byte>();
        _stream = new MemoryStream(_data);
        Size = _data.Length;
        LastModified = DateTimeOffset.UtcNow;
    }

    // IBrowserFile 相關屬性

    public string Name { get; }

    public DateTimeOffset LastModified { get; }

    public long Size { get; }

    public string? ContentType { get; }

    // IBrowserFile 方法，返回檔案內容的讀取流
    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        // Return a copy so multiple reads don't interfere
        return new MemoryStream(_data, writable: false);
    }
}
