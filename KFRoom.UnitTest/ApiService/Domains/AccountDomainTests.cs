using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using KFRoom.ApiService.Domains;
using KFRoom.ApiService.Infra;
using KFRoom.ApiService.Repository;
using KFRoom.Model.RequestModel;

namespace KFRoom.UnitTest.ApiService.Domains
{
    public class AccountDomainTests
    {
        // 加入Repository層的Fake實作，模擬AccountRepository，並記錄傳入的參數以供測試驗證
        // 內容包括AddMemberAsync方法的實作，模擬回傳值，以及GetMemberSalt和GetMemberByEmailPassword方法的簡單實作
        private class FakeAccountRepository : IAccountRepository
        {
            public RegistrationModel? LastModel { get; private set; }
            public byte[]? LastSalt { get; private set; }
            public string? LastAvatar { get; private set; }
            private readonly int _returnValue;

            public FakeAccountRepository(int returnValue) => _returnValue = returnValue;

            public Task<int> AddMemberAsync(RegistrationModel registrationModel, byte[] salt, string memberAvatar)
            {
                LastModel = registrationModel;
                LastSalt = salt;
                LastAvatar = memberAvatar;
                return Task.FromResult(_returnValue);
            }

            public Task<byte[]> GetMemberSalt(string MemberEmail) => Task.FromResult(Array.Empty<byte>());
            public Task<KFRoom.Model.DTO.MemberDTO?> GetMemberByEmailPassword(string MemberEmail, string MemberPassword) => Task.FromResult<KFRoom.Model.DTO.MemberDTO?>(null);
        }

        // 測試AddMemberAsync方法在沒有上傳頭像的情況下，當Repository回傳非0時，是否使用預設的檔名並正確處理密碼，以及返回1的結果
        [Fact]
        public async Task AddMemberAsync_NoAvatar_UsesDefaultFilename_AndReturnsOne_WhenRepositoryReturnsNonZero()                          
        {
            // Arrange: 模擬AccountRepository回傳1，並驗證AddMemberAsync方法在這種情況下的行為，包括使用預設的avatar檔名和正確的密碼處理，以及返回1的結果
            var repo = new FakeAccountRepository(returnValue: 1);
            var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true");
            var azureService = new AzureBlobStorageService(blobServiceClient);
            var securityService = new SecurityService();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new[] {
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultContainer","container"),
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultFilename","default_avatar.png")
            }).Build();

            var domain = new AccountDomain(repo, azureService, securityService, config);

            var model = new RegistrationModel
            {
                MemberName = "name",
                MemberNickName = "nick",
                MemberPhone = "0123456789",
                MemberLineId = "line",
                MemberEmail = "a@b.com",
                MemberSex = "M",
                JobDescription = "job",
                Address = "addr",
                MemberPassword = "Password123",
                MemberPasswordAgain = "Password123",
                MemberBirthday = DateTime.UtcNow
            };

            // Act: 呼叫AddMemberAsync方法，並驗證回傳值以及傳入Repository的參數
            var result = await domain.AddMemberAsync(model);

            // Assert: 驗證回傳值為1，並確認Repository接收到的參數正確，包括使用預設的avatar檔名和正確的密碼處理
            Assert.Equal(1, result);
            Assert.NotNull(repo.LastSalt);
            Assert.True(repo.LastSalt!.Length > 0);
            Assert.Equal("default_avatar.png", repo.LastAvatar);
            Assert.NotEqual("Password123", repo.LastModel!.MemberPassword);
        }
        // 測試AddMemberAsync方法在沒有上傳頭像的情況下，當Repository回傳0時，是否仍然使用預設的檔名並正確處理密碼，以及返回0的結果
        [Fact]
        public async Task AddMemberAsync_NoAvatar_UsesDefaultFilename_AndReturnsZero_WhenRepositoryReturnsZero()
        {
            // Arrange: 模擬AccountRepository回傳0，並驗證AddMemberAsync方法在這種情況下的行為
            var repo = new FakeAccountRepository(returnValue: 0);
            var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true");
            var azureService = new AzureBlobStorageService(blobServiceClient);
            var securityService = new SecurityService();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new[] {
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultContainer","container"),
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultFilename","default_avatar.png")
            }).Build();

            var domain = new AccountDomain(repo, azureService, securityService, config);

            var model = new RegistrationModel
            {
                MemberName = "name",
                MemberNickName = "nick",
                MemberPhone = "0123456789",
                MemberLineId = "line",
                MemberEmail = "a@b.com",
                MemberSex = "M",
                JobDescription = "job",
                Address = "addr",
                MemberPassword = "Password123",
                MemberPasswordAgain = "Password123",
                MemberBirthday = DateTime.UtcNow
            };

            // Act: 呼叫AddMemberAsync方法，並驗證回傳值以及傳入Repository的參數
            var result = await domain.AddMemberAsync(model);

            // Assert: 驗證回傳值為0，並確認Repository接收到的參數正確，包括使用預設的avatar檔名和正確的密碼處理
            Assert.Equal(0, result);
            Assert.NotNull(repo.LastSalt);
            Assert.True(repo.LastSalt!.Length > 0);
            Assert.Equal("default_avatar.png", repo.LastAvatar);
            Assert.NotEqual("Password123", repo.LastModel!.MemberPassword);
        }

        // 測試AddMemberAsync方法在有上傳頭像的情況下，當Repository回傳非0時，是否正確上傳頭像，是否正確處理密碼，以及返回1的結果
        [Fact]
        public async Task AddMemberAsync_WithAvatar_UsesProvidedFilename_AndReturnsOne_WhenRepositoryReturnsNonZero()
        {
            // Arrange: 模擬AccountRepository回傳1，並傳入 MemberAvatar，驗證使用提供的檔名
            var repo = new FakeAccountRepository(returnValue: 1);
            var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true");
            var azureService = new AzureBlobStorageService(blobServiceClient);
            var securityService = new SecurityService();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new[] {
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultContainer","container"),
                new System.Collections.Generic.KeyValuePair<string,string>("MemberAvatar:DefaultFilename","default_avatar.png")
            }).Build();

            var domain = new AccountDomain(repo, azureService, securityService, config);

            var content = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("dummy"));
            content.Position = 0;
            var formFile = new Microsoft.AspNetCore.Http.FormFile(content, 0, content.Length, "MemberAvatar", "custom_avatar.jpg");

            var model = new RegistrationModel
            {
                MemberName = "name",
                MemberNickName = "nick",
                MemberPhone = "0123456789",
                MemberLineId = "line",
                MemberEmail = "a@b.com",
                MemberSex = "M",
                JobDescription = "job",
                Address = "addr",
                MemberPassword = "Password123",
                MemberPasswordAgain = "Password123",
                MemberBirthday = DateTime.UtcNow,
                MemberAvatar = formFile
            };

            // Act
            var result = await domain.AddMemberAsync(model);

            // Assert
            Assert.Equal(1, result);
            Assert.NotNull(repo.LastSalt);
            Assert.True(repo.LastSalt!.Length > 0);
            Assert.Equal("custom_avatar.jpg", repo.LastAvatar);
            Assert.NotEqual("Password123", repo.LastModel!.MemberPassword);
        }
    }
}