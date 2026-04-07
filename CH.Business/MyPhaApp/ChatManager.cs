using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AutoMapper;

using CH.Data;
using CH.Models.Chat;
using CH.Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace CH.Business.MyPhaApp
{
  public interface IChatManager
  {
    Task<SessionList> GetMemberSessionsAsync();
    Task<SessionDetail> GetMemberSessionDetailAsync(int sessionId);
    Task<NewSession> CreateNewMemberSessionAsync(string connectionId);
    Task JoinSessionAsync(string connectionId, int sessionId);
    Task<NewMessage> SaveMemberMessageAsync(string connectionId, int sessionId, string messageText);

    Task MgmtJoinGroupAsync(string connectionId);
    Task MgmtJoinSessionAsync(string connectionId, int sessionId);
    Task MgmtLeaveSessionAsync(string connectionId, int sessionId);
    Task<ClosedSession> MgmtCloseSessionAsync(string connectionId, int sessionId);
    Task<NewMessage> MgmtSaveMessageAsync(string connectionId, int sessionId, string messageText);

    Task CloseAbandonedSessionsAsync();
    Task CheckForOtherWaitingSessionsAsync();
  }

  public class ChatManager : BaseManager, IChatManager
  {
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public ChatManager(AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IHubContext<ChatHub, IChatClient> hubContext,
      IMapper mapper)
      : base(context, identityService, cacheService, config, mapper)
    {
      _hubContext = hubContext;
    }


    public async Task<SessionList> GetMemberSessionsAsync()
    {
      var result = new SessionList()
      {
        IsChatAvailable = Config.IsChatAvailable(DateTime.Now),
      };

      int? memberUserId = this.IdentityService.UserId;
      if (memberUserId.HasValue)
      {
        result.Sessions = await Context.ChatSessions
          .Where(o => o.MyPhaMemberUserId == memberUserId.Value)
          .Select(o => new SessionSummary()
          {
            Id = o.Id,
            StartDate = o.StartDate,
            IsActive = !o.EndDate.HasValue,
            MessageCount = o.ChatMessages.Count(),
          })
          .OrderByDescending(o => o.StartDate)
          .ToListAsync();
      }
      return result;
    }

    public async Task<SessionDetail> GetMemberSessionDetailAsync(int sessionId)
    {
      var result = await Context.ChatSessions
        .Where(o => o.Id == sessionId &&
          o.MyPhaMemberUserId == this.IdentityService.UserId.Value)
        .Select(o => new SessionDetail()
        {
          Id = o.Id,
          IntendedUserName = o.IntendedManagementPortalUser.FullName,
          ActualUserName = o.ActualManagementPortalUser.FullName,
          StartDate = o.StartDate,
          EndDate = o.EndDate,
          Messages = o.ChatMessages
            .Select(m => new MessageSummary()
            {
              FromName = m.UserSender.FullName,
              FromMember = m.UserSenderId == o.MyPhaMemberUserId,
              SentDate = m.SentDate ?? m.CreatedTimestamp,
              Message = m.Message,
            })
            .OrderBy(o => o.SentDate)
            .ToList(),
        })
        .FirstOrDefaultAsync();
      return result;
    }

    public async Task<NewSession> CreateNewMemberSessionAsync(string connectionId)
    {
      using (var saveContext = Context.Clone())
      {
        int memberUserId = this.IdentityService.UserId.Value;
        int? intendedUserId = null;
        string intendedUserName = null;

        int? chMemberId = this.IdentityService.ChMemberId;
        if (chMemberId.HasValue)
        {
          // Lookup the intended user from the member.
          var info = await saveContext.MemberDetails
            .Where(o => o.ChMemberId == chMemberId.Value)
            .Select(o => new
            {
              UserAssignedId = o.UserAssignedId,
              UserAssignedName = o.UserAssigned.FullName,
              ChEmployerId = o.SnowflakeMember.ChEmployerId,
            })
            .FirstOrDefaultAsync();
          if (info != null)
          {
            intendedUserId = info.UserAssignedId;
            intendedUserName = info.UserAssignedName;

            if (!intendedUserId.HasValue)
            {
              // Lookup the default user for the employer.
              var employerInfo = await saveContext.EmployerDetails
                .Where(o => o.ChEmployerId == info.ChEmployerId)
                .Select(o => new
                {
                  UserAssignedId = o.DefaultUserAssignedId,
                  UserAssignedName = o.DefaultUserAssigned.FullName,
                })
                .FirstOrDefaultAsync();
              if (employerInfo != null)
              {
                intendedUserId = employerInfo.UserAssignedId;
                intendedUserName = employerInfo.UserAssignedName;
              }
            }
          }
        }

        var now = DateTimeOffset.Now;

        // Close any prior sessions for this member.
        var oldSessions = await saveContext.ChatSessions
          .Where(o => o.MyPhaMemberUserId == memberUserId &&
            !o.EndDate.HasValue)
          .ToListAsync();
        foreach (var oldSession in oldSessions)
        {
          oldSession.EndDate = now;
          oldSession.UpdatedTimestamp = now;
        }

        var session = new Entities.ChatSession()
        {
          IntendedManagementPortalUserId = intendedUserId.Value,
          MyPhaMemberUserId = memberUserId,
          StartDate = now,
          SessionKey = Guid.NewGuid().ToString(),
          CreatedTimestamp = now,
        };
        saveContext.ChatSessions.Add(session);

        await saveContext.SaveChangesAsync();

        await _hubContext.Groups.AddToGroupAsync(connectionId, session.SessionKey);

        return new NewSession()
        {
          SessionId = session.Id,
          SessionKey = session.SessionKey,
          StartDate = session.StartDate,
          IntendedUserId = intendedUserId.Value,
          IntendedUserName = intendedUserName,
        };
      }
    }

    public async Task JoinSessionAsync(string connectionId, int sessionId)
    {
      int memberUserId = this.IdentityService.UserId.Value;

      string sessionKey = await Context.ChatSessions
        .Where(o => o.Id == sessionId && o.MyPhaMemberUserId == memberUserId)
        .Select(o => o.SessionKey)
        .FirstOrDefaultAsync();
      if (!string.IsNullOrWhiteSpace(sessionKey))
        await _hubContext.Groups.AddToGroupAsync(connectionId, sessionKey);
    }

    public async Task<NewMessage> SaveMemberMessageAsync(string connectionId, int sessionId,
      string messageText)
    {
      // Verify that the user has access to the chat session.
      int? memberUserId = this.IdentityService.UserId;
      var verifiedSession = await Context.ChatSessions
        .Where(o => o.Id == sessionId && o.MyPhaMemberUserId == memberUserId)
        .Select(o => new
        {
          SessionId = o.Id,
          SessionKey = o.SessionKey,
          RecipientUserId = o.ActualManagementPortalUserId ?? o.IntendedManagementPortalUserId,
        })
        .FirstOrDefaultAsync();
      if (verifiedSession == null)
        throw new UnauthorizedAccessException();

      return await SaveMessageAsync(connectionId, verifiedSession.SessionId, verifiedSession.SessionKey,
        true, memberUserId.Value, verifiedSession.RecipientUserId, messageText);
    }

    private async Task<NewMessage> SaveMessageAsync(string connectionId, int sessionId,
      string sessionKey, bool fromMember, int userSenderId, int recipientUserId,
      string messageText)
    {
      var sentDate = DateTimeOffset.Now;

      await _hubContext.Groups.AddToGroupAsync(connectionId, sessionKey);

      using (var saveContext = Context.Clone())
      {
        var now = DateTimeOffset.Now;

        var chatMessage = new Entities.ChatMessage()
        {
          ChatSessionId = sessionId,
          Message = messageText,
          CreatedTimestamp = now,
          SentDate = sentDate,
          DispatchedDate = now,
          UserSenderId = userSenderId,
          UserRecipientId = recipientUserId,
        };
        saveContext.ChatMessages.Add(chatMessage);

        Entities.ChatSession chatSession = null;
        if (!fromMember)
        {
          // Assign the actual user if it hasn't been assigned yet.
          chatSession = await saveContext.ChatSessions
            .Where(o => o.Id == sessionId && !o.ActualManagementPortalUserId.HasValue)
            .FirstOrDefaultAsync();
          if (chatSession != null)
          {
            chatSession.ActualManagementPortalUserId = userSenderId;
            chatSession.UpdatedTimestamp = now;
          }
        }

        await saveContext.SaveChangesAsync();

        var fromUser = await saveContext.Users
          .Where(o => o.Id == userSenderId)
          .Select(o => new { UserId = o.Id, o.FullName })
          .FirstOrDefaultAsync();

        return new NewMessage()
        {
          SessionKey = sessionKey,
          Message = new MessageSummary()
          {
            FromName = fromUser.FullName,
            FromMember = fromMember,
            SentDate = sentDate,
            Message = messageText,
            ActualManagementPortalUserId = !fromMember && chatSession != null ? (int?)fromUser.UserId : null,
          },
        };
      }
    }

    public async Task CloseAbandonedSessionsAsync()
    {
      using (var saveContext = Context.Clone())
      {
        var lastMessageQuery = saveContext.ChatMessages
          .GroupBy(o => o.ChatSessionId)
          .Select(g => new
          {
            SessionId = g.Key,
            LastMessageId = g.Max(o => o.Id),
          });

        var now = DateTimeOffset.Now;
        var abandonedLimTime = now.Subtract(Config.GetChatAbandonedSessionTimeout());

        var abandonedSessions = await saveContext.ChatSessions
          .Where(o => !o.EndDate.HasValue)
          .Join(lastMessageQuery, s => s.Id, m => m.SessionId, (s, m) => new { Session = s, m.LastMessageId })
          .Join(saveContext.ChatMessages, s => s.LastMessageId, m => m.Id, (s, m) => new { Session = s.Session, Message = m })
          .Select(o => new
          {
            IsAnswered = o.Message.UserSenderId != o.Session.MyPhaMemberUserId,
            SentDate = o.Message.SentDate ?? o.Message.CreatedTimestamp,
            Session = o.Session,
          })
          .Where(o => o.IsAnswered && o.SentDate < abandonedLimTime)
          .Select(o => o.Session)
          .ToListAsync();

        string mgmtGroupName = Config.GetChatPhaGroupName();
        foreach (var session in abandonedSessions)
        {
          session.EndDate = now;
          session.UpdatedTimestamp = now;

          // Send the client notification about the closed session.
          await _hubContext.Clients.Group(session.SessionKey).SessionClosed(session.Id, now);

          // Send the internal notification about the closed session.
          await _hubContext.Clients.Group(mgmtGroupName).MgmtSessionClosed(session.Id, now);
        }

        await saveContext.SaveChangesAsync();
      }
    }

    public async Task CheckForOtherWaitingSessionsAsync()
    {
      var lastMessageQuery = Context.ChatMessages
        .GroupBy(o => o.ChatSessionId)
        .Select(g => new
        {
          SessionId = g.Key,
          LastMessageId = g.Max(o => o.Id),
        });

      var now = DateTimeOffset.Now;
      var delayLimTime = now.Subtract(Config.GetChatSessionAlertDelay());

      bool hasDelayedSessions = await Context.ChatSessions
        .Where(o => !o.EndDate.HasValue)
        .Join(lastMessageQuery, s => s.Id, m => m.SessionId, (s, m) => new { Session = s, m.LastMessageId })
        .Join(Context.ChatMessages, s => s.LastMessageId, m => m.Id, (s, m) => new { Session = s.Session, Message = m })
        .Select(o => new
        {
          IsWaiting = o.Message.UserSenderId == o.Session.MyPhaMemberUserId,
          SentDate = o.Message.SentDate ?? o.Message.CreatedTimestamp,
        })
        .Where(o => o.IsWaiting && o.SentDate < delayLimTime)
        .AnyAsync();

      if (hasDelayedSessions)
      {
        string mgmtGroupName = Config.GetChatPhaGroupName();

        // Send the internal notification about the delayed sessions.
        await _hubContext.Clients.Group(mgmtGroupName).MgmtDelayedSessions();
      }
    }


    private async Task VerifyManagementUserAsync(int mgmtUserId)
    {
      // Verify that the user has the ManagementPortalUser role.
      if (!(await Context.ApplicationUserRoles
        .AnyAsync(o => o.UserId == mgmtUserId &&
          o.RoleId == (int)Models.Enums.ApplicationRoleId.ManagementPortalUser)))
      {
        throw new UnauthorizedAccessException();
      }
    }

    public async Task MgmtJoinGroupAsync(string connectionId)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      string groupName = Config.GetChatPhaGroupName();
      await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task MgmtJoinSessionAsync(string connectionId, int sessionId)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      string sessionKey = await Context.ChatSessions
        .Where(o => o.Id == sessionId)
        .Select(o => o.SessionKey)
        .FirstOrDefaultAsync();
      if (!string.IsNullOrWhiteSpace(sessionKey))
        await _hubContext.Groups.AddToGroupAsync(connectionId, sessionKey);
    }

    public async Task MgmtLeaveSessionAsync(string connectionId, int sessionId)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      string sessionKey = await Context.ChatSessions
        .Where(o => o.Id == sessionId)
        .Select(o => o.SessionKey)
        .FirstOrDefaultAsync();
      if (!string.IsNullOrWhiteSpace(sessionKey))
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, sessionKey);
    }

    public async Task<ClosedSession> MgmtCloseSessionAsync(string connectionId, int sessionId)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      using (var saveContext = Context.Clone())
      {
        var session = await saveContext.ChatSessions
          .Where(o => o.Id == sessionId)
          .FirstOrDefaultAsync();
        if (session == null)
          return null;

        if (!session.EndDate.HasValue)
        {
          var now = DateTimeOffset.Now;

          session.EndDate = now;
          session.UpdatedTimestamp = now;

          await saveContext.SaveChangesAsync();
        }

        return new ClosedSession()
        {
          SessionKey = session.SessionKey,
          EndDate = session.EndDate.Value,
        };
      }
    }

    public async Task<NewMessage> MgmtSaveMessageAsync(string connectionId, int sessionId,
      string messageText)
    {
      var sentDate = DateTimeOffset.Now;

      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      var verifiedSession = await Context.ChatSessions
        .Where(o => o.Id == sessionId)
        .Select(o => new
        {
          SessionId = o.Id,
          SessionKey = o.SessionKey,
          MemberUserId = o.MyPhaMemberUserId,
        })
        .FirstOrDefaultAsync();
      if (verifiedSession == null)
        throw new UnauthorizedAccessException();

      return await SaveMessageAsync(connectionId, verifiedSession.SessionId, verifiedSession.SessionKey,
        false, mgmtUserId, verifiedSession.MemberUserId, messageText);
    }
  }
}
