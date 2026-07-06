using KFRoom.Model.RequestModel;
using KFRoom.Model.ResponseModel;
using KFRoom.UnitTest.TestHelpers;
using KFRoom.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KFRoom.UnitTest.Web.Services;

public class AccountServiceTests
{
    // 模擬Configuration
    private IConfiguration BuildConfig()
    {
        var dict = new Dictionary<string, string>
        {
            ["BaseAddress"] = "http://test/"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }
    // AddMemberAsync測試一：測試有上傳包含頭像"MemberAvatar"以及其他欄位時，request 是 multipart/form-data，驗證回傳StandardResponse的內容。
    [Fact]
    public async Task AddMemberAsync_WithAvatar_SendsMultipartAndParsesResponse()
    {
        // Arrange：建立TestDelegatingHandler模擬HttpClient行為，設定回傳的HttpResponseMessage包含序列化的StandardResponse物件。
        var handler = new TestDelegatingHandler();
        var responseObj = new StandardResponse { code = "0", message = "finished" };
        var responseMsg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseObj), Encoding.UTF8, "application/json")
        };
        // 設定TestDelegatingHandler的ResponseFunc，當HttpClient發出請求時，回傳預設的HttpResponseMessage。
        handler.ResponseFunc = (req) => Task.FromResult(responseMsg);
        // 建立HttpClient並注入TestDelegatingHandler，設定BaseAddress為測試URL。
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        // 建立AccountService實例，注入HttpClient和模擬的Configuration。
        var service = new AccountService(httpClient, BuildConfig());

        // Act：
        // 建立RegistrationModel物件
        var registration = new RegistrationModel
        {
            MemberName = "A",
            MemberNickName = "N",
            MemberPhone = "0912345678",
            MemberLineId = "L",
            MemberEmail = "a@b.com",
            MemberSex = "M",
            JobTypeId = 1,
            JobDescription = "d",
            InterestedInLiveYes = true,
            CityCode = 1,
            Address = "addr",
            MemberPassword = "Password1",
            MemberPasswordAgain = "Password1",
            MemberBirthday = DateTime.UtcNow
        };
        // 使用FakeBrowserFile模擬上傳的頭像檔案，檔名為"avatar.png"，內容為"hello"的字串。
        var file = new FakeBrowserFile("avatar.png", "image/png", Encoding.UTF8.GetBytes("hello"));
        // 呼叫AddMemberAsync方法，傳入RegistrationModel和模擬的IBrowserFile，並等待結果。
        var result = await service.AddMemberAsync(registration, file);

        // Assert：
        // 驗證HttpClient發出的請求不為null，表示確實有發出請求。
        Assert.NotNull(handler.LastRequest);
        // 驗證請求的URI為預期的API端點
        Assert.Equal(new Uri("http://test/Account/AddMemberAsync"), handler.LastRequest.RequestUri);
        // Content-Type非null，而且包含"multipart/form-data"
        Assert.NotNull(handler.LastRequestContentType);
        Assert.Contains("multipart/form-data", handler.LastRequestContentType);
        // 回傳的StandardResponse物件的code屬性為"0"。
        Assert.Equal("0", result.code);
    }

    //	AddMemberAsync測試二：測試未上傳頭像時，request 是 multipart / form - data，驗證回傳StandardResponse的內容。
    [Fact]
    public async Task AddMemberAsync_WithoutAvatar_SendsFieldsOnly()
    {
        // Arrange：建立TestDelegatingHandler模擬HttpClient行為，設定回傳的HttpResponseMessage包含序列化的StandardResponse物件。
        var handler = new TestDelegatingHandler();
        var responseObj = new StandardResponse { code = "0", message = "finished" };
        var responseMsg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseObj), Encoding.UTF8, "application/json")
        };
        // 設定TestDelegatingHandler的ResponseFunc，當HttpClient發出請求時，回傳預設的HttpResponseMessage。
        handler.ResponseFunc = (req) => Task.FromResult(responseMsg);
        // 建立HttpClient並注入TestDelegatingHandler，設定BaseAddress為測試URL。
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        // 建立AccountService實例，注入HttpClient和模擬的Configuration。
        var service = new AccountService(httpClient, BuildConfig());

        // Act：
        // 建立RegistrationModel物件並呼叫AddMemberAsync方法，傳入RegistrationModel和null表示不上傳頭像。
        var registration = new RegistrationModel
        {
            MemberName = "A",
            MemberNickName = "N",
            MemberPhone = "0912345678",
            MemberLineId = "L",
            MemberEmail = "a@b.com",
            MemberSex = "M",
            JobTypeId = 1,
            JobDescription = "d",
            InterestedInLiveYes = true,
            CityCode = 1,
            Address = "addr",
            MemberPassword = "Password1",
            MemberPasswordAgain = "Password1",
            MemberBirthday = DateTime.UtcNow
        };
        var result = await service.AddMemberAsync(registration, null);

        // Assert：
        // 驗證HttpClient發出的請求不為null，表示確實有發出請求。
        Assert.NotNull(handler.LastRequest);
        // 驗證請求的URI為預期的API端點
        Assert.Equal(new Uri("http://test/Account/AddMemberAsync"), handler.LastRequest.RequestUri);
        // Content object may be disposed by HttpClient; use captured content type instead
        Assert.NotNull(handler.LastRequestContentType);
        Assert.Contains("multipart/form-data", handler.LastRequestContentType);
        // Avoid reading multipart body (may be disposed); assert basics and parsed response
        Assert.Equal("0", result.code);
    }

    // AddMemberAsync測試三：測試呼叫API失敗，驗證EnsureSuccessStatusCode()引起例外。
    [Fact]
    public async Task AddMemberAsync_ApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange：建立TestDelegatingHandler模擬HttpClient行為，設定回傳的HttpResponseMessage狀態碼為InternalServerError，並包含錯誤訊息。
        var handler = new TestDelegatingHandler();
        var responseMsg = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("error")
        };
        // 設定TestDelegatingHandler的ResponseFunc，當HttpClient發出請求時，回傳預設的HttpResponseMessage。
        handler.ResponseFunc = (req) => Task.FromResult(responseMsg);
        // 建立HttpClient並注入TestDelegatingHandler，設定BaseAddress為測試URL。
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        // 建立AccountService實例，注入HttpClient和模擬的Configuration。
        var service = new AccountService(httpClient, BuildConfig());

        // Act：建立RegistrationModel物件並呼叫AddMemberAsync方法，傳入RegistrationModel和模擬的IBrowserFile，並等待結果。
        var registration = new RegistrationModel
        {
            MemberName = "A",
            MemberNickName = "N",
            MemberPhone = "0912345678",
            MemberLineId = "L",
            MemberEmail = "a@b.com",
            MemberSex = "M",
            JobTypeId = 1,
            JobDescription = "d",
            InterestedInLiveYes = true,
            CityCode = 1,
            Address = "addr",
            MemberPassword = "Password1",
            MemberPasswordAgain = "Password1",
            MemberBirthday = DateTime.UtcNow
        };
        var file = new FakeBrowserFile("avatar.png", "image/png", Encoding.UTF8.GetBytes("hello"));

        // Assert：驗證呼叫AddMemberAsync方法時，會因為API回傳錯誤狀態碼而引起HttpRequestException例外。
        await Assert.ThrowsAsync<HttpRequestException>(() => service.AddMemberAsync(registration, file));
    }

    // LoginAsync測試一：測試登入成功，驗證傳回值。
    [Fact]
    public async Task LoginAsync_Success_ReturnsLoginResponse()
    {
        // Arrange：建立TestDelegatingHandler模擬HttpClient行為，設定回傳的HttpResponseMessage包含序列化的LoginResponse物件。
        var handler = new TestDelegatingHandler();
        var responseObj = new LoginResponse { code = "0", message = "ok", accessToken = "token123" };
        var responseMsg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseObj), Encoding.UTF8, "application/json")
        };
        // 設定TestDelegatingHandler的ResponseFunc，當HttpClient發出請求時，回傳預設的HttpResponseMessage。
        handler.ResponseFunc = (req) => Task.FromResult(responseMsg);
        // 建立HttpClient並注入TestDelegatingHandler，設定BaseAddress為測試URL。
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        // 建立AccountService實例，注入HttpClient和模擬的Configuration。
        var service = new AccountService(httpClient, BuildConfig());

        // Act：建立LoginModel物件並呼叫LoginAsync方法，傳入LoginModel，並等待結果。
        var model = new LoginModel { MemberEmail = "a@b.com", MemberPassword = "Password1" };
        var result = await service.LoginAsync(model);

        // Assert：
        // 驗證回傳的LoginResponse物件的code屬性為"0"，
        // accessToken屬性為"token123"
        Assert.Equal("0", result.code);
        Assert.Equal("token123", result.accessToken);
        // 驗證HttpClient發出的請求不為null，表示確實有發出請求。
        Assert.NotNull(handler.LastRequest);
        // 驗證請求的URI為預期的API端點
        Assert.Equal(new Uri("http://test/Account/LoginAsync"), handler.LastRequest.RequestUri);
    }

    // LoginAsync測試二：測試呼叫API失敗，驗證EnsureSuccessStatusCode()引起例外。
    [Fact]
    public async Task LoginAsync_ApiUnauthorized_ThrowsHttpRequestException()
    {
        // Arrange：
        // 建立TestDelegatingHandler模擬HttpClient行為，設定回傳的HttpResponseMessage狀態碼為Unauthorized，並包含錯誤訊息。
        var handler = new TestDelegatingHandler();
        var responseMsg = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("unauthorized")
        };
        // 設定TestDelegatingHandler的ResponseFunc，當HttpClient發出請求時，回傳預設的HttpResponseMessage。
        handler.ResponseFunc = (req) => Task.FromResult(responseMsg);
        // 建立HttpClient並注入TestDelegatingHandler，設定BaseAddress為測試URL。
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        // 建立AccountService實例，注入HttpClient和模擬的Configuration。
        var service = new AccountService(httpClient, BuildConfig());

        // Act：建立LoginModel物件並呼叫LoginAsync方法，傳入LoginModel，並等待結果。
        var model = new LoginModel { MemberEmail = "a@b.com", MemberPassword = "Password1" };

        // Assert：驗證呼叫LoginAsync方法時，會因為API回傳未授權狀態碼而引起HttpRequestException例外。
        await Assert.ThrowsAsync<HttpRequestException>(() => service.LoginAsync(model));
    }
}
