using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CH.Models.Common;

namespace CH.Models.ManagementPortal.Member.Search
{
  public class MemberSearchRequest
  {
    public int? MemberId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string DOB { get; set; }
    public int? PHA { get; set; }
    public string Risk { get; set; }
    public string Client { get; set; }
    public string GroupName { get; set; }
    public string Carrier { get; set; }
    public string AlternateMemberId { get; set; }
    public string SecondaryContact { get; set; }
    public string SelfReportedPhone { get; set; }
    public string GroupId { get; set; }
  }

  public class MemberSearchResult
  {
    public int Id { get; set; }
    public string MemberId { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public DateTime? Dob { get; set; }
    public int? ClinicalRisk { get; set; }
    public string AssignedPha { get; set; }
    public bool DoNotCall { get; set; }
    public bool HasMyPhaAccount { get; set; }
    public string? AlternateMemberId { get; set; }
    public string? Relationship { get; set; }
    public string GroupId { get; set; }
    public bool VIP { get; set; }
    public bool Minor { get; set; }
    public bool MinorMember { get; set; }
    public string? TruncatedAlternateMemberId { get; set; }
    public string? ClientEmployeeId { get; set; }
    public string? Carrier { get; set; }
    public string SecondaryContact { get; set; }
    public string SelfReportedPhone   { get; set; }
    public int? Engaged { get; set; }
    public string CellPhone { get; set; }
    public string WorkPhone { get; set; }
    public string HomePhone { get; set; }
    public string CurrentStatus { get; set; }
    public string ClientId { get; set; }

  }
  public class AssignRequest
  {
    public string[] client { get; set; }
    public string[] state { get; set; }
    public string[] pha { get; set; }
    public string cdate { get; set; }


  }
  public class GenericAssignRequest
  {

    public string[] clients { get; set; }
    public string[] state { get; set; }
    public string[] gpha { get; set; }
    public string gdate { get; set; }


  }
}
