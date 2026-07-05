using System.ComponentModel.DataAnnotations;
namespace KFRoom.Model.RequestModel;

public class LoginModel
{
    [Required(ErrorMessage = "eMail(帳號)必填")]
    [EmailAddress(ErrorMessage = "eMail格式不符")]
    public string MemberEmail { get; set; }
    [Required(ErrorMessage = "密碼必填")]
    [StringLength(12, MinimumLength = 8, ErrorMessage = "密碼長度為8~12個文字")]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "密碼只能是英數字")]
    public string MemberPassword { get; set; }
}
