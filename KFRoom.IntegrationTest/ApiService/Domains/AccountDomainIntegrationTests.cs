using Azure.Storage.Blobs;
using KFRoom.ApiService.Domains;
using KFRoom.ApiService.Infra;
using KFRoom.ApiService.Repository;
using KFRoom.Model.RequestModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KFRoom.IntegrationTest.ApiService.Domains;

public class AccountServiceIntegrationTests
{
    // 測試1.帳號不存在：
    // •	GetMemberSalt 回空陣列
    // •	ValidateMemberAsync 應回 (1, null)
    [Fact]
    public async Task ValidateMemberAsync_ReturnsOneAndNull_WhenMemberEmailDoesNotExist()
    {
        // Arrange
        var domain = CreateDomain();
        var email = CreateUniqueEmail();

        // Act
        var (ret, member) = await domain.ValidateMemberAsync(email, "Password123");

        // Assert
        Assert.Equal(1, ret);
        Assert.Null(member);
    }
    // 測試2.帳號存在但密碼錯誤：
    // •	可取得 salt
    // •	雜湊後查不到會員
    // •	應回(2, null)
    [Fact]
    public async Task ValidateMemberAsync_ReturnsTwoAndNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var domain = CreateDomain();
        var repository = CreateRepository();
        var securityService = new SecurityService();

        var email = CreateUniqueEmail();
        var correctPassword = "Password123";
        var wrongPassword = "WrongPassword123";

        await DeleteMemberAsync(email);

        try
        {
            await SeedMemberAsync(repository, securityService, email, correctPassword);

            // Act
            var (ret, member) = await domain.ValidateMemberAsync(email, wrongPassword);

            // Assert
            Assert.Equal(2, ret);
            Assert.Null(member);
        }
        finally
        {
            await DeleteMemberAsync(email);
        }
    }
    // 測試3.帳密正確：
    // •	可取得 salt
    // •	雜湊後能查到會員
    // •	應回(0, member)
    [Fact]
    public async Task ValidateMemberAsync_ReturnsZeroAndMember_WhenCredentialsAreCorrect()
    {
        // Arrange
        var domain = CreateDomain();
        var repository = CreateRepository();
        var securityService = new SecurityService();

        var email = CreateUniqueEmail();
        var password = "Password123";

        await DeleteMemberAsync(email);

        try
        {
            await SeedMemberAsync(repository, securityService, email, password);

            // Act
            var (ret, member) = await domain.ValidateMemberAsync(email, password);

            // Assert
            Assert.Equal(0, ret);
            Assert.NotNull(member);
            Assert.Equal(email, member!.MemberEmail);
            Assert.Equal("整合測試會員", member.MemberName);
            Assert.Equal("整合測試暱稱", member.MemberNickName);
            Assert.Equal(1, member.StatusId);
        }
        finally
        {
            await DeleteMemberAsync(email);
        }
    }
    // 產生 AccountDomain 實例，並注入必要的相依物件
    private static AccountDomain CreateDomain()
    {
        var configuration = CreateConfiguration();
        var repository = new MSSQLAccountRepository(configuration);
        var securityService = new SecurityService();

        // ValidateMemberAsync 不會用到 Blob，但建構子需要此相依物件
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var azureBlobStorageService = new AzureBlobStorageService(blobServiceClient);

        return new AccountDomain(repository, azureBlobStorageService, securityService, configuration);
    }

    //// 以下皆為輔助方法，用於建立測試所需的相依物件或操作資料庫 ////
   
    // 產生 MSSQLAccountRepository 實例，並注入必要的相依物件
    private static MSSQLAccountRepository CreateRepository()
    {
        var configuration = CreateConfiguration();
        return new MSSQLAccountRepository(configuration);
    }
    // 產生 IConfigurationRoot 實例，從環境變數或 appsettings.json 讀取資料庫連線字串
    private static IConfigurationRoot CreateConfiguration()
    {
        var envConnectionString = Environment.GetEnvironmentVariable("KFROOM_SQL_CONNECTION");

        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:sqldb"] = envConnectionString
                })
                .Build();
        }

        var apiServiceProjectPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "KFRoom.ApiService"));

        return new ConfigurationBuilder()
            .SetBasePath(apiServiceProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();
    }
    // 取得資料庫連線字串，若找不到則拋出例外
    private static string GetConnectionString()
    {
        var configuration = CreateConfiguration();
        var connectionString = configuration["ConnectionStrings:sqldb"];

        Assert.False(string.IsNullOrWhiteSpace(connectionString), "找不到整合測試所需的資料庫連線字串。");
        return connectionString!;
    }
    // 在資料庫中新增一筆測試會員資料，使用指定的Email和密碼
    private static async Task SeedMemberAsync(
        IAccountRepository repository,
        SecurityService securityService,
        string email,
        string plainPassword)
    {
        var hashedPassword = securityService.HashPassword(plainPassword, out var salt);

        var model = new RegistrationModel
        {
            MemberName = "整合測試會員",
            MemberNickName = "整合測試暱稱",
            MemberPhone = "0912345678",
            MemberLineId = $"line_{Guid.NewGuid():N}",
            MemberEmail = email,
            MemberSex = "M",
            JobTypeId = 1,
            JobDescription = "Integration Test",
            InterestedInLiveYes = true,
            CityCode = 1,
            Address = "台北市整合測試路1號",
            MemberPassword = hashedPassword,
            MemberPasswordAgain = hashedPassword,
            MemberBirthday = new DateTime(2000, 1, 1)
        };

        await repository.AddMemberAsync(model, salt, "default_avatar.png");
    }
    // 從資料庫中刪除指定Email的會員資料，避免測試資料累積
    private static async Task DeleteMemberAsync(string email)
    {
        await using var connection = new SqlConnection(GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Member] WHERE [MemberEmail] = @MemberEmail";
        command.Parameters.AddWithValue("@MemberEmail", email);

        await command.ExecuteNonQueryAsync();
    }
    // 產生唯一的Email地址，避免測試中使用重複的Email
    private static string CreateUniqueEmail()
        => $"validate-member-{Guid.NewGuid():N}@example.com";
}