using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KFRoom.ApiService.Infra;
public class SecurityService
{
    // 產生密碼雜湊值與salt值
    public string HashPassword(string password, out byte[] salt)
    {
        // 產生亂數的Salt值
        salt = RandomNumberGenerator.GetBytes(128 / 8);
        // 產生密碼雜湊值
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return hashed;
    }
    // 傳入User輸入的密碼與加入會員/忘記密碼產生的Salt值，回傳密碼雜湊值
    public string HashPassword(string password, byte[] salt)
    {
        // 產生密碼雜湊值
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return hashed;
    }
    // 依傳入IssuerSigningKey與會員資料產生JWT並回傳。
    public string GenerateJWTAsync(string JwtKey, string JwtIssuer, string JwtAudience,string MemberEmail,int StatusId,int MemberId,int Expires)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, MemberId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("MemberEmail", MemberEmail),
            new Claim("StatusId", StatusId.ToString()),
            new Claim("MemberId", MemberId.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.Now.AddHours(Expires),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
