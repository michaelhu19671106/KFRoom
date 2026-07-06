using Aspire.Hosting.Testing;
using KFRoom.Model.RequestModel;
using KFRoom.Web.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KFRoom.IntegrationTest.Web.Services;

public class AccountServiceIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    // AddMemberAsync測試一：傳入有效的註冊資料，應該返回成功的結果。
    [Fact]
    public async Task AddMemberAsync_WithValidData_ReturnsSuccess()
    {
        await ExecuteWithServiceAsync(async service =>
        {
            // Arrange
            var registration = CreateRegistrationModel();
            // Act
            var result = await service.AddMemberAsync(registration, memberAvatar: null);
            // Assert
            Assert.Equal("0", result.code);
            Assert.Equal("finished", result.message);
        });
    }
    // AddMemberAsync測試二：傳入有效的註冊資料和一個模擬的avatar檔案，應該返回成功的結果。
    [Fact]
    public async Task AddMemberAsync_WithAvatar_ReturnsSuccess()
    {
        await ExecuteWithServiceAsync(async service =>
        {
            // Arrange
            var registration = CreateRegistrationModel();
            var avatar = new FakeBrowserFile(
                name: "avatar.png",
                contentType: "image/png",
                data: System.Text.Encoding.UTF8.GetBytes("integration-test-avatar"));
            // Act
            var result = await service.AddMemberAsync(registration, avatar);
            // Assert
            Assert.Equal("0", result.code);
            Assert.Equal("finished", result.message);
        });
    }

    // LoginAsync測試一：使用已註冊的會員資料進行登入，應該返回成功的結果和有效的accessToken。
    [Fact]
    public async Task LoginAsync_WithRegisteredMember_ReturnsSuccessAndAccessToken()
    {
        await ExecuteWithServiceAsync(async service =>
        {
            // Arrange: 先註冊一個會員，然後使用該會員的Email和密碼進行登入。
            var registration = CreateRegistrationModel();
            // Act: 註冊會員
            var registerResult = await service.AddMemberAsync(registration, memberAvatar: null);
            // Assert: 註冊成功
            Assert.Equal("0", registerResult.code);

            // Arrange: 登入會員資料
            var loginModel = new LoginModel
            {
                MemberEmail = registration.MemberEmail,
                MemberPassword = registration.MemberPassword
            };
            // Act: 呼叫LoginAsync方法進行登入
            var loginResult = await service.LoginAsync(loginModel);
            // Assert: 登入成功，並且返回有效的accessToken
            Assert.Equal("0", loginResult.code);
            Assert.Equal("Finished", loginResult.message);
            Assert.False(string.IsNullOrWhiteSpace(loginResult.accessToken));
        });
    }
    // LoginAsync測試二：使用已註冊的會員資料，但提供錯誤的密碼進行登入，應該返回無效密碼的結果。
    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsInvalidPasswordCode()
    {
        await ExecuteWithServiceAsync(async service =>
        {
            // Arrange: 先註冊一個會員，然後使用該會員的Email和錯誤的密碼進行登入。
            var registration = CreateRegistrationModel();
            // Act: 註冊會員
            var registerResult = await service.AddMemberAsync(registration, memberAvatar: null);
            // Assert: 註冊成功
            Assert.Equal("0", registerResult.code);

            // Arrange: 登入會員資料，使用錯誤的密碼
            var loginModel = new LoginModel
            {
                MemberEmail = registration.MemberEmail,
                MemberPassword = "Wrong123"
            };
            // Act: 呼叫LoginAsync方法進行登入
            var loginResult = await service.LoginAsync(loginModel);
            // Assert: 登入失敗，返回無效密碼的結果
            Assert.Equal("2", loginResult.code);
            Assert.Equal("Invalid MemberPassword", loginResult.message);
            Assert.True(string.IsNullOrEmpty(loginResult.accessToken));
        });
    }
    // 執行測試的通用方法，負責建立測試環境、初始化AccountService，並執行傳入的測試動作。
    private static async Task ExecuteWithServiceAsync(Func<AccountService, Task> testAction)
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.KFRoom_AppHost>(
            cts.Token);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cts.Token);
        await app.StartAsync(cts.Token);

        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("apiservice", cts.Token)
            .WaitAsync(DefaultTimeout, cts.Token);

        var httpClient = app.CreateHttpClient("apiservice");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var service = new AccountService(httpClient, configuration);

        await testAction(service);
    }
    // 建立一個測試用的註冊模型，包含隨機生成的暱稱、電話號碼和Email，以便在測試中使用。
    private static RegistrationModel CreateRegistrationModel()
    {
        var id = Guid.NewGuid().ToString("N")[..8];

        return new RegistrationModel
        {
            MemberName = "測試會員",
            MemberNickName = $"暱稱{id}",
            MemberPhone = CreatePhoneNumber(),
            MemberLineId = $"line{id}",
            MemberEmail = $"member_{id}@example.com",
            MemberSex = "M",
            JobTypeId = 1,
            JobDescription = "Integration Test Job",
            InterestedInLiveYes = true,
            CityCode = 1,
            Address = "台北市測試地址1號",
            MemberPassword = "Pass1234",
            MemberPasswordAgain = "Pass1234",
            MemberBirthday = new DateTime(2000, 1, 1)
        };
    }
    // 建立一個隨機的台灣手機號碼，格式為09xxxxxxxx，其中x為隨機數字。
    private static string CreatePhoneNumber()
    {
        var random = Random.Shared.Next(10000000, 99999999);
        return $"09{random}";
    }
    // FakeBrowserFile類別實作了IBrowserFile介面，用於模擬上傳檔案的行為，方便在測試中使用。
    private sealed class FakeBrowserFile : IBrowserFile
    {
        private readonly byte[] _data;

        public FakeBrowserFile(string name, string contentType, byte[] data)
        {
            Name = name;
            ContentType = contentType;
            _data = data;
            LastModified = DateTimeOffset.UtcNow;
            Size = _data.Length;
        }

        public string Name { get; }

        public DateTimeOffset LastModified { get; }

        public long Size { get; }

        public string ContentType { get; }

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return new MemoryStream(_data, writable: false);
        }
    }
}