// API層/Domain層需要使用下列物件
using Azure.Storage.Blobs;
using KFRoom.ApiService.Domains;
using KFRoom.ApiService.Infra;
using KFRoom.ApiService.Repository;
using KFRoom.Model.RequestModel;
using KFRoom.Model.ResponseModel;
// [FromForm]特性需要引用Microsoft.AspNetCore.Mvc命名空間
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

// 注冊BlobServiceClient物件實例，供AzureBlobStorageService物件注入使用
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["ConnectionStrings:blobs"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("找不到 Blob Storage 連線字串 'blobs'。");
    }

    return new BlobServiceClient(connectionString);
});
// 注冊相關服務類別實體：AccountDomain會使用AzureBlobStorageService/SecurityService/MSSQLAccountRepository
// AzureBlobStorageService/SecurityService物件整個應用程式只需要一個實例，因為多個API呼叫使用同一個物件並不會發生資料運作衝突
// MSSQLAccountRepository會操作DB，為了確保每個API呼叫使不會互相干擾，採用Scoped生命週期，確保每個API呼叫使用不同的MSSQLAccountRepository物件實例
// AccountDomain物件會呼叫MSSQLAccountRepository物件存取DB，因此採用Scoped生命週期
builder.Services.AddSingleton<AzureBlobStorageService>();
builder.Services.AddSingleton<SecurityService>();
builder.Services.AddScoped<IAccountRepository, MSSQLAccountRepository>();
// API AddMemberAsync會使用AccountDomain物件
builder.Services.AddScoped<AccountDomain>();

var app = builder.Build();

// 只保留一組 Aspire 預設端點（包含 health checks）
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//var summaries = new []
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();

//    return forecast;
//})
//.WithName("GetWeatherForecast");

// AddMemberAsync API：呼叫Domain層將會員頭像儲存到會員圖像儲存區，並新增一筆會員記錄。
// Account用來區分Domain，跟AccountController的意思一樣，不加也可以
// 由於呼叫端會上傳檔案，因此以[FromForm]接收參數
app.MapPost("/Account/AddMemberAsync", async Task<StandardResponse> ([FromForm] RegistrationModel model, AccountDomain accountDomain) =>
{
    StandardResponse response = new StandardResponse();
    try
    {
        // 8.系統在AddMember API呼叫Domain層AddMember方法。
        var ret = await accountDomain.AddMemberAsync(model);
    }
    catch (Exception ex)
    {
        // 14a.系統在AddMember API判斷8~13執行失敗。
        // 14a-1.系統依API規格回傳執行失敗資料。
        response.code = "1";
        response.message = "Fail to add member";
        return response;
    }
    // 14.系統在AddMember API判斷8~13執行成功。
    // 15.系統在AddMember API回傳執行成功資料。
    response.code = "0";
    response.message = "finished";
    return response;
}).DisableAntiforgery();  // 本API使用了[FromForm]會被視為需要 anti-forgery 保護而出錯，但目前並不需要！
// LoginAsync API：
// 呼叫AccountDomain.ValidateMemberAsync方法驗證會員帳/密，
// 再呼叫SecurityService.GenerateJWTAsync方法產生JWT並回傳。
app.MapPost("/Account/LoginAsync", async Task<LoginResponse> (LoginModel model, AccountDomain accountDomain, SecurityService securityService, IConfiguration _configuration) =>
{
    LoginResponse response = new LoginResponse();
    try
    {
        // 8.系統在Login API呼叫Domain層ValidateMember方法。
        var (ret, member) = await accountDomain.ValidateMemberAsync(model.MemberEmail, model.MemberPassword);
        if (ret == 1)
        {
            //14a.系統在Login API判斷8r回傳ret == 1。
            //14a - 1.系統在Login API依API規格回傳資料。
            //  {
            //        "AccessToken":"",
            //    "code":"1",
            //    "message”:” Invalid MemberEmail"
            //  }
            response.accessToken = "";
            response.code = "1";
            response.message = "Invalid MemberEmail";
            return response;
        }
        if (ret == 2)
        {
            //14b.系統在Login API判斷8r回傳ret == 2。
            //14b - 1.系統在Login API依API規格回傳資料。
            //  {
            //        "AccessToken":"",
            //    "code":"2",
            //    "message":" Invalidate MemberPassword"
            //  }
            response.accessToken = "";
            response.code = "2";
            response.message = "Invalid MemberPassword";
            return response;
        }
        // 14.系統在Login API判斷8r回傳ret ==0。
        // 15.系統在Login API呼叫Infra層產生JWT。
        var token = securityService.GenerateJWTAsync(
            JwtKey: _configuration["Jwt:Key"]!,
            JwtIssuer: _configuration["Jwt:Issuer"]!,
            JwtAudience: _configuration["Jwt:Audience"]!,
            MemberEmail: member!.MemberEmail,
            StatusId: member.StatusId,
            MemberId: member.MemberId,
            Expires: int.Parse(_configuration["Jwt:Expires"]!)
            );
        // 16-0.系統在Login API判斷8~15執行成功。
        // 16.系統在Login API依API規格回傳資料。
        //{
        //    "AccessToken":"15產生JWT",
        //    "code":"0",
        //    "message":"Finished"
        //}
        response.accessToken = token;
        response.code = "0";
        response.message = "Success.";
        return response;
    }
    catch(Exception ex)
    {
        // 16-0a.系統在Login API判斷8~15執行失敗。
        //16 - 0a - 1.系統回傳：
        //  {
        //    "AccessToken":"",
        //    "code":"3",
        //    "message":"System Error"
        //  }
        response.accessToken = "";
        response.code = "3";
        response.message = "System Error";
        return response;
    }
});

app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}