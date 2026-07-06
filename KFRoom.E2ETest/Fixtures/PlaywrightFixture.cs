using System;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace KFRoom.E2ETest.Fixtures;
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; }
    public IBrowser Browser { get; private set; }

    // 初始化 Playwright 和瀏覽器
    public async Task InitializeAsync()
    {
        // 建立 Playwright 實例
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        // 建立 Firefox 瀏覽器實例
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }
    // 解構函式，釋放資源
    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}