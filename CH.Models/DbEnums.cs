/************************************************************
 * This is an auto-generated source file.                   *
 * DO NOT EDIT MANUALLY.                                    *
 * Edit the T4 template that generated this file to modify. *
 ***********************************************************/

using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CH.Models.Enums
{
  #region CodeDefs

  /****************
   * CodeDefTypes *
   ****************/

  /// <summary>
  /// An enumeration of all the available CodeDefTypes.
  /// </summary>
  public enum CodeDefTypeId
  {
    /// <summary>Audit Change Type</summary>
    AuditChangeType = 21,
    /// <summary>Audit Type</summary>
    AuditType = 1,
    /// <summary>Communication Method</summary>
    CommunicationMethod = 15,
    /// <summary>Consent Received Type</summary>
    ConsentReceived = 5,
    /// <summary>Contact Method</summary>
    ContactMethod = 2,
    /// <summary>Contact Time</summary>
    ContactTime = 16,
    /// <summary>Language</summary>
    Language = 22,
    /// <summary>Learning Style</summary>
    LearningStyle = 19,
    /// <summary>Mode of Completion</summary>
    CompletionMode = 4,
    /// <summary>Outreach Frequency</summary>
    OutreachFrequency = 14,
    /// <summary>Outreach Member Response</summary>
    OutreachMemberResponse = 12,
    /// <summary>Outreach Method</summary>
    OutreachMethod = 11,
    /// <summary>Outreach Purpose</summary>
    OutreachPurpose = 13,
    /// <summary>Program Type</summary>
    ProgramType = 3,
    /// <summary>Status</summary>
    StatusType = 7,
    /// <summary>Task Priority</summary>
    TaskPriority = 9,
    /// <summary>Task Status</summary>
    TaskStatus = 10,
    /// <summary>Task Type</summary>
    TaskType = 18,
    /// <summary>Visit Status</summary>
    VisitStatus = 8,
    /// <summary>Visit Type</summary>
    VisitType = 6,

    FileType = 23,

    Purpose=24,
    /// <summary>Program Name</summary>
    ProgramName = 25,
    /// <summary>Program Completion Status</summary>
    ProgramCompletionStatus = 26,
    OutReachNotesList = 27,
    CommunicationNotesList = 28,
    CareCoordinationNotesList = 29,
    VendorName = 30,
  }


  /************
   * CodeDefs *
   ************/

  public enum AuditChangeType
  {
    /// <summary>Delete</summary>
    Delete = 123,
    /// <summary>Insert</summary>
    Insert = 121,
    /// <summary>Update</summary>
    Update = 122,
  }

  public enum AuditType
  {
    /// <summary>Changed Password</summary>
    ChangedPassword = 3,
    /// <summary>Failed Login</summary>
    FailedLogin = 2,
    /// <summary>Member Search</summary>
    MemberSearch = 124,
    /// <summary>Member View</summary>
    MemberView = 125,
    /// <summary>Reset Password</summary>
    ResetPassword = 5,
    /// <summary>Successful Login</summary>
    SuccessfulLogin = 1,
  }

  public enum CommunicationMethod
  {
    /// <summary>Archived Note</summary>
    ArchivedNote = 78,
    /// <summary>E-Fax</summary>
    EFax = 76,
    /// <summary>Fax</summary>
    Fax = 75,
    /// <summary>Incoming Call</summary>
    IncomingCall = 69,
    /// <summary>Incoming Email</summary>
    IncomingEmail = 71,
    /// <summary>Incoming Text</summary>
    IncomingText = 73,
    /// <summary>Mailing</summary>
    Mailing = 128,
    /// <summary>Onsite Visit</summary>
    OnsiteVisit = 77,
    /// <summary>Outgoing Call</summary>
    OutgoingCall = 70,
    /// <summary>Outgoing Email</summary>
    OutgoingEmail = 72,
    /// <summary>Outgoing Text</summary>
    OutgoingText = 74,
  }

  public enum ConsentReceived
  {
    /// <summary>Declined</summary>
    Declined = 19,
    /// <summary>Not Required</summary>
    NotRequired = 16,
    /// <summary>Signed</summary>
    Signed = 17,
    /// <summary>Verbal</summary>
    Verbal = 18,
  }

  public enum ContactMethod
  {
    /// <summary>Email</summary>
    Email = 6,
    /// <summary>Home Phone</summary>
    HomePhone = 89,
    /// <summary>HS Call</summary>
    HSCall = 10,
    /// <summary>Mail</summary>
    Mail = 8,
    /// <summary>Mobile Phone</summary>
    MobilePhone = 7,
    /// <summary>Text Msg</summary>
    TextMsg = 9,
    /// <summary>Work Phone</summary>
    WorkPhone = 90,
  }

  public enum ContactTime
  {
    /// <summary>10-12pm</summary>
    Morning2 = 81,
    /// <summary>12-3pm</summary>
    Afternoon1 = 82,
    /// <summary>3-5pm</summary>
    Afternoon2 = 83,
    /// <summary>5-7pm</summary>
    Afternoon3 = 84,
    /// <summary>7-10am</summary>
    Morning1 = 80,
  }

  public enum Language
  {
    /// <summary>English</summary>
    English = 126,
    /// <summary>Spanish</summary>
    Spanish = 127,
  }

  public enum LearningStyle
  {
    /// <summary>Auditory</summary>
    Auditory = 111,
    /// <summary>Physical (Kinesthetic)</summary>
    Physicial = 114,
    /// <summary>Verbal</summary>
    Verbal = 113,
    /// <summary>Visual</summary>
    Visual = 112,
  }

  public enum CompletionMode
  {
    /// <summary>Docusign</summary>
    Docusign = 14,
    /// <summary>In-Person</summary>
    InPerson = 13,
    /// <summary>Verbal</summary>
    Verbal = 15,
  }

  public enum OutreachFrequency
  {
    /// <summary>Continued Outreach</summary>
    ContinuedOutreach = 68,
    /// <summary>First Attempt</summary>
    FirstAttempt = 65,
    /// <summary>Second Attempt</summary>
    SecondAttempt = 66,
    /// <summary>Third Attempt</summary>
    ThirdAttempt = 67,
  }

  public enum OutreachMemberResponse
  {
    /// <summary>Agreed to communication</summary>
    AgreedToCommunication = 53,
    /// <summary>Bad Contact Information</summary>
    NoContactInformation = 57,
    /// <summary>Bad Email</summary>
    BadEmail = 61,
    /// <summary>Bad Home Number</summary>
    BadHomeNumber = 58,
    /// <summary>Bad Mobile Number</summary>
    BadMobileNumber = 60,
    /// <summary>Bad Work Number</summary>
    BadWorkNumber = 59,
    /// <summary>Declined Communication</summary>
    DeclinedCommunication = 54,
    /// <summary>Left Voicemail</summary>
    LeftVoicemail = 55,
    /// <summary>Other</summary>
    Other = 62,
    /// <summary>Unable to leave voicemail</summary>
    UnableToLeaveVoicemail = 56,
  }

  public enum OutreachMethod
  {
    /// <summary>Email</summary>
    Email = 50,
    /// <summary>Mail</summary>
    Mail = 52,
    /// <summary>Phone</summary>
    Phone = 49,
    /// <summary>Text Msg</summary>
    TextMsg = 51,
  }

  public enum OutreachPurpose
  {
    /// <summary>To Engage Member</summary>
    ToEngageMember = 64,
    /// <summary>To Review Summary</summary>
    ToReviewSummary = 63,
  }

  public enum ProgramType
  {
    /// <summary>Engaged</summary>
    Engaged = 11,
    /// <summary>Engaged Care Coordination</summary>
    EngagedCareCoordination = 119,
    /// <summary>Non-Engaged</summary>
    NonEngaged = 12,
    /// <summary>Non-Engaged Care Coordination</summary>
    NonEngagedCareCoordination = 120,
  }

  public enum StatusType
  {
    /// <summary>Active</summary>
    Active = 30,
    /// <summary>Cobra</summary>
    Cobra = 32,
    /// <summary>Termed</summary>
    Termed = 31,
  }

  public enum TaskPriority
  {
    /// <summary>High</summary>
    High = 42,
    /// <summary>Low</summary>
    Low = 44,
    /// <summary>Medium</summary>
    Medium = 43,
    /// <summary>Urgent</summary>
    Urgent = 41,
  }

  public enum TaskStatus
  {
    /// <summary>Closed</summary>
    Closed = 45,
    /// <summary>Open</summary>
    Open = 46,
    /// <summary>Overdue</summary>
    Overdue = 47,
    /// <summary>Pending</summary>
    Pending = 48,
  }

  public enum TaskType
  {
    /// <summary>Appointment</summary>
    Appointment = 98,
    /// <summary>Communication</summary>
    Communication = 97,
    /// <summary>Consultant Review</summary>
    ConsultantReview = 110,
    /// <summary>Educational Call</summary>
    EducationalCall = 92,
    /// <summary>Educational Mailing</summary>
    EducationalMailing = 91,
    /// <summary>Follow-up Assessment</summary>
    FollowUpAssessment = 95,
    /// <summary>Follow-up Call</summary>
    FollowUpCall = 94,
    /// <summary>Follow-up Visit</summary>
    FollowUpVisit = 93,
    /// <summary>Initial Assessment</summary>
    InitialAssessment = 109,
    /// <summary>Other</summary>
    Other = 108,
    /// <summary>Outreach</summary>
    Outreach = 96,
    /// <summary>Provider Contact</summary>
    ProviderContact = 101,
    /// <summary>Referral Management</summary>
    ReferralManagement = 107,
    /// <summary>Return Member Call</summary>
    ReturnMemberCall = 106,
    /// <summary>Return Provider Call</summary>
    ReturnProviderCall = 105,
    /// <summary>Review Attachment</summary>
    ReviewAttachment = 104,
    /// <summary>Schedule Meeting</summary>
    ScheduleMeeting = 99,
    /// <summary>Screen For Program</summary>
    ScreenForProgram = 103,
    /// <summary>Send PHS</summary>
    SendPhs = 100,
    /// <summary>Service/Procedure Review</summary>
    ServiceProcedureReview = 102,
  }

  public enum VisitStatus
  {
    /// <summary>Attempt</summary>
    Attempt = 34,
    /// <summary>Bad Contact Information</summary>
    NoContactInformation = 37,
    /// <summary>Bad Email</summary>
    BadEmail = 40,
    /// <summary>Bad Home Number</summary>
    BadHomeNumber = 38,
    /// <summary>Bad Work Number</summary>
    BadWorkNumber = 39,
    /// <summary>Completed</summary>
    Completed = 33,
    /// <summary>Message Left</summary>
    MessageLeft = 35,
    /// <summary>No Show</summary>
    NoShow = 36,
  }

  public enum VisitType
  {
    /// <summary>Appointment Reminder</summary>
    AppointmentReminder = 23,
    /// <summary>Education Sent</summary>
    EducationSent = 28,
    /// <summary>Follow Up</summary>
    FollowUp = 21,
    /// <summary>Initial Visit</summary>
    InitialVisit = 20,
    /// <summary>Initial Visit & PHS Review</summary>
    InitialVisitAndPHSReview = 22,
    /// <summary>Member Check-in</summary>
    MemberCheckIn = 27,
    /// <summary>PHS Review</summary>
    PHSReview = 24,
    /// <summary>PHS to Care Team</summary>
    PHSToCareTeam = 26,
    /// <summary>PHS to PCP</summary>
    PHSToPCP = 25,
    /// <summary>Resources Sent</summary>
    ResourcesSent = 29,
    /// <summary>Vendor Response</summary>
    VendorResponse = 79,
    /// <summary>Testimonial</summary>
    Testimonial = 133,
    /// <summary>Success Story</summary>
    SuccessStory = 135,
    /// <summary>Unplanned Call</summary>
    UnplannedCall = 159,
    /// <summary>Outreach</summary>
    Outreach = 319,
  }

  #endregion // CodeDefs


  #region Identity

  /********************
   * ApplicationRoles *
   ********************/

  /// <summary>
  /// An enumeration of all the available application roles.
  /// </summary>
  public enum ApplicationRoleId
  {
    /// <summary>Management Portal User</summary>
    ManagementPortalUser = 1,
    /// <summary>MyPHA Member User</summary>
    MyPhaMemberUser = 2,
    /// <summary>Management Portal Admin</summary>
    ManagementPortalAdmin = 3,
  }

  /// <summary>
  /// An static class of all the available application roles.
  /// </summary>
  public static class ApplicationRole
  {
    /// <summary>Management Portal User</summary>
    public static readonly string ManagementPortalUser = "ManagementPortalUser";
    /// <summary>MyPHA Member User</summary>
    public static readonly string MyPhaMemberUser = "MyPhaMemberUser";
    /// <summary>Management Portal Admin</summary>
    public static readonly string ManagementPortalAdmin = "ManagementPortalAdmin";
  }

  #endregion
}

