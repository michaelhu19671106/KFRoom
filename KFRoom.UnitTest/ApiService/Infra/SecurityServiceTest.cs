using System;
using Xunit;
using KFRoom.ApiService.Infra;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace KFRoom.UnitTest.ApiService.Infra
{
    public class SecurityServiceTest
    {
        // 產生密碼雜湊值與salt值 的單元測試
        [Fact]
        public void HashPassword_OutParameter_ProducesSaltAndHash()
        {
            // Arrange：建立SecurityService實例和測試密碼，準備呼叫HashPassword方法。
            var svc = new SecurityService();
            string password = "Password123";
            // Act：呼叫HashPassword方法，傳入密碼並使用out參數接收產生的salt值。
            string hash = svc.HashPassword(password, out byte[] salt);

            // Assert：驗證在hash不會是Null也不會是空字串，salt不為空且長度為16字節（128位）。
            Assert.NotNull(hash);
            Assert.NotNull(salt);
            Assert.Equal(128 / 8, salt.Length); // expect 16 bytes
            Assert.NotEmpty(hash);
        }

        // 傳入User輸入的密碼與加入會員/忘記密碼產生的Salt值，回傳密碼雜湊值 的單元測試
        // 傳入參數時[Fact]改為[Theory]，並使用[InlineData]提供不同的參數值
        [Theory]
        [InlineData("Password123")]
        [InlineData("AnotherPassword456")]
        // 方法加入引數以接收InlineData提供的密碼值
        public void HashPassword_WithSalt_ReproducesSameHash(string password)
        {
            // Arrange：建立SecurityService實例和測試密碼。
            var svc = new SecurityService();
            //string password = "Password123";

            // Act：先呼叫HashPassword方法產生密碼雜湊值和salt值，然後再使用相同的密碼和salt值呼叫HashPassword方法。
            string hash1 = svc.HashPassword(password, out byte[] salt);
            string hash2 = svc.HashPassword(password, salt);

            // Assert：驗證兩次產生的雜湊值是否相同。
            Assert.Equal(hash1, hash2);
        }

        // 依傳入IssuerSigningKey與會員資料產生JWT並回傳。 的單元測試
        [Fact]
        public void GenerateJWTAsync_ReturnsTokenContainingClaims()
        {
            // Arrange：建立SecurityService實例，定義JWT相關參數（密鑰、發行者、受眾、會員資料等）。準備呼叫GenerateJWTAsync方法。
            var svc = new SecurityService();
            string key = "very_secret_test_key_which_is_long_enough_for_hmac";
            string issuer = "test_issuer";
            string audience = "test_audience";
            string email = "user@test.com";
            int status = 1;
            int memberId = 123;
            int expires = 1; // hours

            // Act：呼叫GenerateJWTAsync方法，傳入上述參數並接收回傳的JWT字串。
            string token = svc.GenerateJWTAsync(key, issuer, audience, email, status, memberId, expires);

            // Assert：驗證回傳的JWT字串不為Null或空白。
            Assert.False(string.IsNullOrWhiteSpace(token));
            // 讀取JWT字串。
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            // Assert：驗證JWT的發行者、受眾和聲明是否正確。
            Assert.Equal(issuer, jwt.Issuer);
            Assert.Contains(audience, jwt.Audiences);
            Assert.Contains(jwt.Claims, c => c.Type == "MemberEmail" && c.Value == email);
            Assert.Contains(jwt.Claims, c => c.Type == "StatusId" && c.Value == status.ToString());
            Assert.Contains(jwt.Claims, c => c.Type == "MemberId" && c.Value == memberId.ToString());
        }
    }
}
