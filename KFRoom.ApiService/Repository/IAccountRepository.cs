using KFRoom.Model.DTO;
using KFRoom.Model.RequestModel;
namespace KFRoom.ApiService.Repository;
// 帳號相關資料存取介面
public interface IAccountRepository
{
    // 新增一筆會員資料
    Task<int> AddMemberAsync(RegistrationModel registrationModel, byte[] salt, string memberAvatar);
    Task<byte[]> GetMemberSalt(string MemberEmail);
    // 以MemberEmail/MemberPassword讀取會員資料表"Member"之一筆記錄
    Task<MemberDTO?> GetMemberByEmailPassword(string MemberEmail, string MemberPassword);

}