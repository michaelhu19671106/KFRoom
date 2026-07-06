namespace KFRoom.UnitTest.TestHelpers;

// 客製化的 HttpMessageHandler 用於模擬 HttpClient 行為，捕獲最後的請求(request)並允許返回自定義響應。
public class TestDelegatingHandler : HttpMessageHandler
{
    // 在測試中捕獲最後的請求以供斷言使用（如果請求有內容，則捕獲內容類型）
    public HttpRequestMessage? LastRequest { get; private set; }
    // 在測試中捕獲最後的請求內容類型以供斷言使用（如果請求有內容）
    public string? LastRequestContentType { get; private set; }

    // 一個委派，可以檢查請求並返回回應
    public Func<HttpRequestMessage, Task<HttpResponseMessage>>? ResponseFunc { get; set; }

    // 覆寫 SendAsync 方法以捕獲請求並返回自定義響應
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content != null)
        {
            // 如果請求有內容，捕獲內容類型
            LastRequestContentType = request.Content.Headers.ContentType?.MediaType;
        }
        if (ResponseFunc != null)
        {
            // 如果設置了回應委派，使用它來生成回應
            return await ResponseFunc(request);
        }
        // 否則，返回一個默認的 OK 回應，內容為空的 JSON
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };
    }
}
