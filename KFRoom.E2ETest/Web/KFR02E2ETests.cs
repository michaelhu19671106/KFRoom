using KFRoom.E2ETest.Fixtures;
using KFRoom.Model.RequestModel;
using KFRoom.Model.TestData;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
// 使用 xUnit
using Xunit;

namespace KFRoom.E2ETest.Web;

// 加入類別架構，才能使用 xUnit 進行測試
// 要實作IClassFixture<PlaywrightFixture>
public class KFR02E2ETests : IClassFixture<PlaywrightFixture>
{
    // 使用 PlaywrightFixture 來管理 Playwright 實例
    private readonly PlaywrightFixture _fixture;

    // 建構函式，注入 PlaywrightFixture
    public KFR02E2ETests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }
    // 測試一：KFR02-主要
    [Fact]
    public async Task Main_Registration_WithAvatar_Success()  
    {
        // 在測試中使用 PlaywrightFixture 的 Browser 實例
        // 建立新的瀏覽器上下文
        var context = await _fixture.Browser.NewContextAsync();
        // 建立新分頁
        var page = await context.NewPageAsync();
        // 前往KFRoom首頁
        await page.GotoAsync("https://localhost:7158/");
        // 前往註冊頁面
        await page.GetByRole(AriaRole.Link, new() { Name = "註冊" }).ClickAsync();
        // 填寫註冊表單
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).FillAsync("Michael Hu");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).PressAsync("Tab");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "暱稱*(2~15個中英文字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "暱稱*(2~15個中英文字元)" }).FillAsync("Michael");
        await page.Locator("#memberBirthday").FillAsync("");
        await page.Locator("#memberBirthday").PressAsync("ArrowRight");
        await page.Locator("#memberBirthday").FillAsync("1967-11-01");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "手機*(9~10數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "手機*(9~10數字)" }).FillAsync("0921345678");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).FillAsync("Michael11");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).FillAsync("MichaelHu1106@gmail.com");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByText("註冊會員 姓名*(2~30個中英文字元) 暱稱*(2~15").ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).FillAsync("michaelhu1106@gmail.com");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).PressAsync("Tab");
        await page.GetByLabel("性別*").SelectOptionAsync(new[] { "男" });
        await page.GetByLabel("職業類別*").SelectOptionAsync(new[] { "2" });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).FillAsync("IT Leader");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).FillAsync("IT Student");
        await page.GetByLabel("居住城市*").SelectOptionAsync(new[] { "2" });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "居住地址*(60個字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "居住地址*(60個字元以內)" }).FillAsync("工學路2號");
        await page.GetByText("姓名*(2~30個中英文字元) 暱稱*(2~15").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "C:\\images\\business_message\\radio.png" });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "密碼*(8~12個字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "密碼*(8~12個字元)" }).FillAsync("12345678");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "再次輸入密碼*(同密碼)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "再次輸入密碼*(同密碼)" }).FillAsync("12345678");
        // await page.Locator("div").Nth(2).ClickAsync();
        // 提交註冊表單
        await page.GetByRole(AriaRole.Button, new() { Name = "註冊" }).ClickAsync();
        // 等待註冊成功訊息
        await page.CloseAsync();
    }
    // 測試一-1：KFR02-主要-加入輸入參數化，方便測試不同的註冊資料
    [Theory]
    [MemberData(nameof(RegistrationModelTestData.ValidModels), MemberType = typeof(RegistrationModelTestData))]
    public async Task Main_Registration_WithAvatar_Success_MemberData(RegistrationModel model)
    {
        // 在測試中使用 PlaywrightFixture 的 Browser 實例
        // 建立新的瀏覽器上下文
        var context = await _fixture.Browser.NewContextAsync();
        // 建立新分頁
        var page = await context.NewPageAsync();
        // 前往KFRoom首頁
        await page.GotoAsync("https://localhost:7158/");
        // 前往註冊頁面
        await page.GetByRole(AriaRole.Link, new() { Name = "註冊" }).ClickAsync();
        // 填寫註冊表單
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).ClickAsync();
        // 以參數model取代固定值
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).FillAsync(model.MemberName);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "姓名*(2~30個中英文字元)" }).PressAsync("Tab");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "暱稱*(2~15個中英文字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "暱稱*(2~15個中英文字元)" }).FillAsync(model.MemberNickName);
        await page.Locator("#memberBirthday").FillAsync("");
        await page.Locator("#memberBirthday").PressAsync("ArrowRight");
        await page.Locator("#memberBirthday").FillAsync(model.MemberBirthday.ToString("yyyy-MM-dd"));
        await page.GetByRole(AriaRole.Textbox, new() { Name = "手機*(9~10數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "手機*(9~10數字)" }).FillAsync(model.MemberPhone);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Line Id*(2~30個中英文字元及數字)" }).FillAsync(model.MemberLineId);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).FillAsync(model.MemberEmail);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "email*" }).PressAsync("Tab");
        await page.GetByLabel("性別*").SelectOptionAsync(new[] { model.MemberSex });
        await page.GetByLabel("職業類別*").SelectOptionAsync(new[] { model.JobTypeId.ToString() });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "職業說明*(60個中英文字元以內)" }).FillAsync(model.JobDescription);
        await page.GetByLabel("居住城市*").SelectOptionAsync(new[] { model.CityCode.ToString() });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "居住地址*(60個字元以內)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "居住地址*(60個字元以內)" }).FillAsync(model.Address);
        await page.GetByText("姓名*(2~30個中英文字元) 暱稱*(2~15").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "C:\\images\\business_message\\radio.png" });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "密碼*(8~12個字元)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "密碼*(8~12個字元)" }).FillAsync(model.MemberPassword);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "再次輸入密碼*(同密碼)" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "再次輸入密碼*(同密碼)" }).FillAsync(model.MemberPasswordAgain);
        // await page.Locator("div").Nth(2).ClickAsync();
        // 提交註冊表單
        await page.GetByRole(AriaRole.Button, new() { Name = "註冊" }).ClickAsync();
        // 等待註冊成功訊息
        await page.CloseAsync();
    }
}