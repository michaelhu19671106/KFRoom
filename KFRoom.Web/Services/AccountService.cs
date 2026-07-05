using KFRoom.Model.RequestModel;
using KFRoom.Model.ResponseModel;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace KFRoom.Web.Services;
public class AccountService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public AccountService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    // 呼叫API AddMember進行會員註冊
    public async Task<StandardResponse> AddMemberAsync(RegistrationModel registrationModel, IBrowserFile? memberAvatar)
    {
        var apiUrl = $"{_httpClient.BaseAddress}Account/AddMemberAsync";
        // 先加入圖片檔案參數
        StreamContent? imageContent = null;
        if (memberAvatar != null)
        {
            imageContent = new StreamContent(memberAvatar.OpenReadStream());
        }
        // 上傳檔案與其他參數需要使用 MultipartFormDataContent
        using MultipartFormDataContent multipartContent = new();
        if (memberAvatar != null && imageContent != null)
        {
            multipartContent.Add(imageContent, "MemberAvatar", memberAvatar.Name);
        }
        // 再加入其他參數
        multipartContent.Add(new StringContent(registrationModel.MemberName, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberName");
        multipartContent.Add(new StringContent(registrationModel.MemberNickName, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberNickName");
        multipartContent.Add(new StringContent(registrationModel.MemberPhone, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberPhone");
        multipartContent.Add(new StringContent(registrationModel.MemberLineId, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberLineId");
        multipartContent.Add(new StringContent(registrationModel.MemberEmail, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberEmail");
        multipartContent.Add(new StringContent(registrationModel.MemberSex, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberSex");
        multipartContent.Add(new StringContent(registrationModel.JobTypeId.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "JobTypeId");
        multipartContent.Add(new StringContent(registrationModel.JobDescription, Encoding.UTF8, MediaTypeNames.Text.Plain), "JobDescription");
        multipartContent.Add(new StringContent(registrationModel.InterestedInLiveYes.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "InterestedInLiveYes");
        multipartContent.Add(new StringContent(registrationModel.CityCode.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "CityCode");
        multipartContent.Add(new StringContent(registrationModel.Address, Encoding.UTF8, MediaTypeNames.Text.Plain), "Address");
        multipartContent.Add(new StringContent(registrationModel.MemberPassword, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberPassword");
        multipartContent.Add(new StringContent(registrationModel.MemberPasswordAgain, Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberPasswordAgain");
        multipartContent.Add(new StringContent(registrationModel.MemberBirthday.ToString("yyyy-MM-dd"), Encoding.UTF8, MediaTypeNames.Text.Plain), "MemberBirthday");
        // 呼叫API
        var response = await _httpClient.PostAsync(apiUrl, multipartContent);
        response.EnsureSuccessStatusCode();  // 檢查 HTTP 回應是否為 成功狀態碼 (2xx)，若不是則拋出例外
        string data = await response.Content.ReadAsStringAsync();
        StandardResponse jsonData = JsonSerializer.Deserialize<StandardResponse>(data);
        return jsonData;
    }
    // LoginAsync方法：呼叫Login API進行登入驗證，取得JWT Token並回傳給頁面。
    public async Task<LoginResponse> LoginAsync(LoginModel model)
    {
        var apiUrl = $"{_httpClient.BaseAddress}Account/LoginAsync";
        // 將 model 轉為 JSON 並包裝成 StringContent以便傳送
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        // 呼叫API
        var response = await _httpClient.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();  
        string data = await response.Content.ReadAsStringAsync();
        LoginResponse jsonData = JsonSerializer.Deserialize<LoginResponse>(data);
        return jsonData;
    }
}
