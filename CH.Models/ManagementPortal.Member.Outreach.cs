using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CH.Models.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CH.Models.ManagementPortal.Member.Outreach
{
  public class ProgramSummary
  {
    public int Id { get; set; }
    public string Carrier { get; set; }
    public DateTime? EffectiveEligibleDate { get; set; }
    public string CoverageType { get; set; }
    public string Status { get; set; }
    public int? ProgramTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public string BenefitPlan { get; set; }
    public int? AssignedToId { get; set; }
    public bool? ConsentSentMem { get; set; }
//    public DateTime? ConsentSentMemDate { get; set; }
    public DateTime? ConsentDate { get; set; }
    public int? ConsentReceivedTypeId { get; set; }
    public string AuthorizedRepEmailAddress { get; set; }
    public string AuthorizedRepPhoneNumber { get; set; }
    public bool? AuthorizedRep { get; set; }
    public string AuthorizedRepName { get; set; }
    public bool? PhsSentMember { get; set; }
    public DateTime? PhsSentMemberDate { get; set; }
    public bool? PhsReviewedMem { get; set; }
    public DateTime? PhsReviewedMemDate { get; set; }
    public bool? PhsReportPcp { get; set; }
    public DateTime? PhsReportToPcpDate { get; set; }
    public bool? PhsReportToSpecialist { get; set; }
    public DateTime? PhsReportSpecialistDate { get; set; }
  }


  public class Demographic
  {
    public int Id { get; set; }
    public string MemberId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public DateTime? Dob { get; set; }
    public string Gender { get; set; }
    public string Ethnicity { get; set; }
    public int? PrefLanguageId { get; set; }
    public string Ssn { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string EmailAddress { get; set; }
    public string CellPhone { get; set; }
    public string WorkPhone { get; set; }
    public string HomePhone { get; set; }

    public string UpdatedPhone { get; set; } // Updated_PHONE (length: 100)
    public string SelfReportedPhone { get; set; }

    public int? PrefContactTimeId { get; set; }
    public int? PrefContactMethodId { get; set; }
    public string SecondaryContact { get; set; }
    public string MaraRisk { get; set; }
    public int? ClinicalRisk { get; set; }
    public string RelationClass { get; set; }
    public string Client { get; set; }
    public string Group { get; set; }
    public int? LearningStyleId { get; set; }
    public bool HasMyPhaAccount { get; set; }
  }

  public class CommunicationNote 
  {
    [JsonProperty("id")]
    public int? Id { get; set; }
    [JsonProperty("method")]
    public string Method { get; set; }
    [JsonProperty("noteDate")]
    public DateTime NoteDate { get; set; }
    [JsonProperty("noteText")]
    public string NoteText { get; set; }
    [JsonProperty("visitType")]
    public string VisitType { get; set; }
    [JsonProperty("visitStatus")]
    public string VisitStatus { get; set; }

    [JsonProperty("uploadedDocs")]
    public string uploadedDocs { set; get; }
    [JsonProperty("userInitial")]
    public string UserInitial { get; set; }
    [JsonProperty("outreachId")]
    public string outreachId { get; set; }
  }
  public class EmailBlast
  {
    public string uploadedDocs { set; get; }
    public string selectionType { set; get; }
  }

  public class ReferralManagementNote
  {
    public int? Id { get; set; }
    [JsonProperty("referralDate")]
    public DateTime? ReferralDate { get; set; }
    [JsonProperty("referredTo")]
    public string ReferredTo { get; set; }
  }

  public class OutreachReport
  {
    [JsonProperty("id")]
    public int? Id { get; set; }
    [JsonProperty("outreachMemResponse")]
    public string OutreachMemResponse { get; set; }
    [JsonProperty("outreachMethod")]
    public string OutreachMethod { get; set; }
    [JsonProperty("outreachDate")]
    public DateTime? OutreachDate { get; set; }
    [JsonProperty("outreachFrequency")]
    public string OutreachFrequency { get; set; }
    [JsonProperty("outreachPurpose")]
    public string OutreachPurpose { get; set; }
    [JsonProperty("outreachNote")]
    public string OutreachNote { get; set; }
    [JsonProperty("userInitial")]
    public string UserInitial { get; set; }
    [JsonProperty("notesType")]
    public string NotesType { get; set; }
  }
  public class CareCommunicationNote
  {
    [JsonProperty("id")]
    public int? Id { get; set; }
    [JsonProperty("chMemberId")]
    public int ChMemberId { get; set; } // CH_MEMBER_ID
    [JsonProperty("method")]
    public string Method { get; set; } // Method (length: 20)
    [JsonProperty("noteDate")]
    public DateTime? NoteDate { get; set; } // NoteDate
    [JsonProperty("visitType")]
    public string VisitType { get; set; } // VisitType (length: 40)
    [JsonProperty("noteText")]
    public string NoteText { get; set; } // NoteText (length: 4000)
    [JsonProperty("visitStatus")]
    public string VisitStatus { get; set; } // VisitStatus (length: 40)
    [JsonProperty("eid")]
    public string Eid { get; set; } // EID (length: 50)
    [JsonProperty("groupId")]
    public string GroupId { get; set; } // GroupId (length: 50)
    [JsonProperty("groupName")]
    public string GroupName { get; set; } // GroupName (length: 50)
    [JsonProperty("memberId")]
    public string MemberId { get; set; } // MemberId (length: 50)
    [JsonProperty("purpose")]
    public string Purpose { get; set; } // Purpose
    [JsonProperty("fileType")]
    public string? FileType { get; set; } // FileType (length: 100)
    [JsonProperty("date")]
    public DateTime Date { get; set; } // Date
    [JsonProperty("sphnotes")]
    public string SphNotes { get; set; } // SPHNotes

    [JsonProperty("uploadedDocs")]
    public string uploadedDocs { set; get; }
    [JsonProperty("userInitial")]
    public string UserInitial { get; set; }
    [JsonProperty("outreachId")]
    public string outreachId { get; set; }

  }


  public class MemberNotes
  {
    [JsonProperty("id")]
    public int? Id { get; set; }
    [JsonProperty("chMemberId")]
    public int ChMemberId { get; set; } // CH_MEMBER_ID
    [JsonProperty("method")]
    public string Method { get; set; } // Method (length: 20)
    [JsonProperty("noteDate")]
    public DateTime? NoteDate { get; set; } // NoteDate
    [JsonProperty("visitType")]
    public string VisitType { get; set; } // VisitType (length: 40)
    [JsonProperty("noteText")]
    public string NoteText { get; set; } // NoteText (length: 4000)
    [JsonProperty("visitStatus")]
    public string VisitStatus { get; set; } // VisitStatus (length: 40)
    [JsonProperty("eid")]
    public string Eid { get; set; } // EID (length: 50)
    [JsonProperty("groupId")]
    public string GroupId { get; set; } // GroupId (length: 50)
    [JsonProperty("groupName")]
    public string GroupName { get; set; } // GroupName (length: 50)
    [JsonProperty("memberId")]
    public string MemberId { get; set; } // MemberId (length: 50)
    [JsonProperty("purpose")]
    public string Purpose { get; set; } // Purpose
    [JsonProperty("fileType")]
    public string? FileType { get; set; } // FileType (length: 100)
    [JsonProperty("date")]
    public DateTime Date { get; set; } // Date
    [JsonProperty("sphnotes")]
    public string SphNotes { get; set; } // SPHNotes

    [JsonProperty("uploadedDocs")]
    public string uploadedDocs { set; get; }

    [JsonProperty("outreachMemResponse")]
    public string OutreachMemResponse { get; set; }
    [JsonProperty("outreachMethod")]
    public string OutreachMethod { get; set; }
    [JsonProperty("outreachDate")]
    public DateTime? OutreachDate { get; set; }
    [JsonProperty("outreachFrequency")]
    public string OutreachFrequency { get; set; }
    [JsonProperty("outreachPurpose")]
    public string OutreachPurpose { get; set; }
    [JsonProperty("outreachNote")]
    public string OutreachNote { get; set; }
    [JsonProperty("referralDate")]
    public DateTime? ReferralDate { get; set; }
    [JsonProperty("referredTo")]
    public string ReferredTo { get; set; }
  }

}
