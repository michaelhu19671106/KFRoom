using KFRoom.ApiService.Infra;
using KFRoom.Model.RequestModel;
using KFRoom.ApiService.Repository;
using KFRoom.Model.DTO;


namespace KFRoom.ApiService.Domains;

public class AccountDomain
{
    private IAccountRepository _accountRepository;
    private AzureBlobStorageService _azureBlobStorageService;
    private SecurityService _securityService;
    private IConfiguration _configuration;
    // 注入Infra/Repository層的服務以及配置服務
    public AccountDomain(IAccountRepository accountRepository, AzureBlobStorageService azureBlobStorageService, SecurityService securityService, IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        _azureBlobStorageService = azureBlobStorageService;
        _securityService = securityService;
        _configuration = configuration;
    }
    // 呼叫Infra層將會員頭像儲存到會員圖像儲存區。呼叫Repository層新增一筆會員記錄。
    public async Task<int> AddMemberAsync(RegistrationModel model)
    {
        int ret = 0;
        var defaultContainer = _configuration["MemberAvatar:DefaultContainer"];
        var defaultFilename = _configuration["MemberAvatar:DefaultFilename"];
        var filePath = "";
        string filename = model.MemberAvatar?.FileName ?? "";
        if (!string.IsNullOrEmpty(filename))
        {
            // 9-01.系統Domain層AddMember方法判斷8有上傳頭像。
            // 9-02.系統設定fileName=" 8上傳頭像之filename"。
            // 9.系統呼叫Infra層儲存上傳頭像。
            await using var fileStream = model.MemberAvatar!.OpenReadStream();
            await _azureBlobStorageService.UploadFileAsync(fileStream, defaultContainer, filename);
        }
        else
        {
            // 9-01.系統判斷8沒有上傳頭像。
            //    9-01a-1.系統設定fileName ="default_avatar.png"。
            //  9 - 01a - 2.回10。
            filename = defaultFilename;
        }
        // 10.系統呼叫Service層加密密碼並產生Salt值。
        string hashed = _securityService.HashPassword(model.MemberPassword, out byte[] salt);
        model.MemberPassword = hashed;
        // 11.系統在Domain層AddMember方法呼叫Repository層新增一筆會員記錄。
        var spRet = await _accountRepository.AddMemberAsync(model, salt, filename);
        // Repository層傳回值為0傳回0，不為0回傳1
        return spRet == 0 ? 0 : 1;
    }
    // 驗證帳/密並回傳驗證結果以及符合帳/密的Member資料，以ValueTuple回傳兩個資料
    public async Task<(int, MemberDTO?)> ValidateMemberAsync(string MemberEmail, string MemberPassword)
    {
        // 9.系統在Domain層ValidateMember方法呼叫Repository層以Email讀取一筆會員salt資料。
        byte[] salt = await _accountRepository.GetMemberSalt(MemberEmail);
        if (salt == Array.Empty<byte>())
        {
            // 10-1a.系統判斷9傳回值salt為空陣列。
            //   10-1a-1.系統回傳
            //(
            //ret: 1(表MemberEmail不正確),
            //MemberDTO：null
            //)
            return (1, null);
        }
        // 10-1.系統在Domain層ValidateMember方法判斷9傳回值salt不為空陣列。
        // 11.系統在Domain層ValidateMember方法呼叫Infra層加密密碼。
        string hashed = _securityService.HashPassword(MemberPassword, salt);
        // 12.系統在Domain層ValidateMember方法呼叫Repository層依帳/密讀取一筆會員資料。
        var member = await _accountRepository.GetMemberByEmailPassword(MemberEmail, hashed);
        if (member == null)
        {
            // 13-1a.系統在Domain層ValidateMember方法判斷12傳回MemberDTO為null。
            //  13 - 1a - 1.系統回傳
              //(
              //ret: 2(表MemberPassword不正確),
              //MemberDTO：null
              //)
            return (2, null);
        }
        // 13-1.系統在Domain層ValidateMember方法判斷12傳回MemberDTO不為null。
        // 8r.系統在Domain層ValidateMember方法回傳下列資料：
        //(
        //out ret: 0(表帳 / 密正確),
        //MemberDTO：12回傳一筆會員資料(MemberDTO型別)        
        //)
        return (0, member);
    }
}
