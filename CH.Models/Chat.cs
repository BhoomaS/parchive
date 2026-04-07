using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CH.Models.Common;

namespace CH.Models.Chat
{
  #region MyPha

  [MyPhaTypescriptInclude]
  public class SessionSummary
  {
    public int Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public bool IsActive { get; set; }
    public int MessageCount { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class SessionList
  {
    public bool IsChatAvailable { get; set; }
    public List<SessionSummary> Sessions { get; set; }


    public SessionList()
    {
      this.Sessions = new List<SessionSummary>();
    }
  }

  [TypescriptInclude]
  public class MessageSummary
  {
    public string FromName { get; set; }
    public bool FromMember { get; set; }
    public DateTimeOffset SentDate { get; set; }
    public string Message { get; set; }

    [TypescriptIgnore]
    public int? ActualManagementPortalUserId { get; set; }
  }

  public class NewSession
  {
    public int SessionId { get; set; }
    public string SessionKey { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public int IntendedUserId { get; set; }
    public string IntendedUserName { get; set; }
  }

  public class NewMessage
  {
    public string SessionKey { get; set; }
    public MessageSummary Message { get; set; }
  }

  public class ClosedSession
  {
    public string SessionKey { get; set; }
    public DateTimeOffset EndDate { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class SessionDetail
  {
    public int Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string IntendedUserName { get; set; }
    public string ActualUserName { get; set; }
    public List<MessageSummary> Messages { get; set; }


    public SessionDetail()
    {
      this.Messages = new List<MessageSummary>();
    }
  }

  #endregion // MyPha


  #region Management

  [ManagementPortalTypescriptInclude]
  public class MgmtSessionFilter
  {
    public bool IncludeInactive { get; set; }
    public bool IncludeMine { get; set; }
    public bool IncludeOther { get; set; }
  }

  [ManagementPortalTypescriptInclude]
  public class MgmtUserSessionState
  {
    public int MyOpenChatCount { get; set; }
    public int MyWaitingChatCount { get; set; }
    public int OtherOpenChatCount { get; set; }
    public int OtherWaitingChatCount { get; set; }
  }

  [ManagementPortalTypescriptInclude]
  public class MgmtSessionSummary
  {
    public int Id { get; set; }
    public int IntendedUserId { get; set; }
    public string IntendedUserName { get; set; }
    public int ActualUserId { get; set; }
    public string ActualUserName { get; set; }
    public string MemberName { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset LastMessageDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool WaitingForResponse { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
  }

  [ManagementPortalTypescriptInclude]
  public class MgmtSessionList
  {
    public List<MgmtSessionSummary> Sessions { get; set; }


    public MgmtSessionList()
    {
      this.Sessions = new List<MgmtSessionSummary>();
    }
  }

  [ManagementPortalTypescriptInclude]
  public class MgmtSessionDetail
  {
    public int Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string MemberName { get; set; }

    public int IntendedUserId { get; set; }
    public string IntendedUserName { get; set; }

    public int? ActualUserId { get; set; }
    public string ActualUserName { get; set; }

    public int CurrentUserId { get; set; }

    public List<MessageSummary> Messages { get; set; }


    public MgmtSessionDetail()
    {
      this.Messages = new List<MessageSummary>();
    }
  }

  #endregion // Management
}
