using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CH.Models.Common;

namespace CH.Models.MyPhaApp.Member
{
  [MyPhaTypescriptInclude]
  public class MemberSummary
  {
    public int ChMemberId { get; set; }
    public string MemberId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string EmailAddress { get; set; }
    public string CellPhone { get; set; }
    public string WorkPhone { get; set; }
    public string HomePhone { get; set; }
    public string PlanName { get; set; }
  }


  [MyPhaTypescriptInclude]
  public class MemberDetail
  {
    public int ChMemberId { get; set; }
    public string MemberId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    //public DateTime? Dob { get; set; }
    //public string Gender { get; set; }
    //public string Ethnicity { get; set; }
    //public int? PrefLanguageId { get; set; }
    //public string Ssn { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string EmailAddress { get; set; }
    public string CellPhone { get; set; }
    public string WorkPhone { get; set; }
    public string HomePhone { get; set; }
    //public int? PrefContactTimeId { get; set; }
    //public int? PrefContactMethodId { get; set; }
    //public string SecondaryContact { get; set; }
    //public string MaraRisk { get; set; }
    //public int? ClinicalRisk { get; set; }
    //public string RelationClass { get; set; }
    //public string Client { get; set; }
    //public int? LearningStyleId { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class SaveMemberResult : Common.SaveResult<MemberDetail>
  {
    public MemberDetail Detail { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class Pha
  {
    // From AspNetUsers
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string FullName { get; set; }

    // From PhaDetails
    public string Title { get; set; } // Bio (length: 4000)
    public string Bio { get; set; } // Bio (length: 4000)
    public string HeadshotUrl { get; set; } // HeadshotUrl (length: 4000)
  }

}
