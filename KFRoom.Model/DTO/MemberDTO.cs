//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace KFRoom.Model.DTO;
// 定義 MemberDTO 類別，用於表示課程資料傳輸物件
public class MemberDTO
{
    public int MemberId { get; set; }
    public string MemberName { get; set; }
    public string MemberNickName { get; set; }
    public string MemberPhone { get; set; }
    public string MemberLineId { get; set; }
    public string MemberEmail { get; set; }
    public string MemberSex { get; set; }
    public int JobTypeId { get; set; }
    public string JobDescription { get; set; }
    public bool InterestedInLiveYes { get; set; }
    public int CityCode { get; set; }
    public string Address { get; set; }
    public string? MemberAvatar { get; set; }
    public DateTime MemberBirthday { get; set; }
    public int LogicScore { get; set; }
    public int StatusId { get; set; }
}
