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

namespace CH.Business.ManagementPortal
{
  public interface IChatManager
  {
    Task<MgmtSessionList> GetSessionListAsync(Models.Chat.MgmtSessionFilter filter);
    Task<MgmtSessionDetail> GetSessionDetailAsync(int sessionId);
    Task<MgmtUserSessionState> GetUserSessionState();
  }

  public class ChatManager : BaseManager, IChatManager
  {
    public ChatManager(AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IMapper mapper)
      : base(context, identityService, cacheService, config, mapper)
    {
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

    public async Task<MgmtSessionList> GetSessionListAsync(Models.Chat.MgmtSessionFilter filter)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      var query = Context.ChatSessions.AsQueryable();
      if (!filter.IncludeInactive)
        query = query.Where(o => !o.EndDate.HasValue);
      if (!filter.IncludeMine)
        query = query.Where(o => (o.ActualManagementPortalUserId ?? o.IntendedManagementPortalUserId) != mgmtUserId);
      if (!filter.IncludeOther)
        query = query.Where(o => (o.ActualManagementPortalUserId ?? o.IntendedManagementPortalUserId) == mgmtUserId);

      var sessions = await query
        .Select(o => new
        {
          LastMessage = o.ChatMessages
            .OrderByDescending(m => m.SentDate ?? m.CreatedTimestamp)
            .Select(o => new
            {
              MessageDate = o.SentDate ?? o.CreatedTimestamp,
              UserSenderId = o.UserSenderId,
            })
            .FirstOrDefault(),
          MemberUserId = o.MyPhaMemberUserId,
          Summary = new MgmtSessionSummary()
          {
            Id = o.Id,
            IntendedUserId = o.IntendedManagementPortalUserId,
            IntendedUserName = o.IntendedManagementPortalUser.FullName,
            ActualUserId = o.ActualManagementPortalUserId ?? 0,
            ActualUserName = o.ActualManagementPortalUser.FullName,
            MemberName = o.MyPhaMemberUser.FullName,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            MessageCount = o.ChatMessages.Sum(m => 1),
            UnreadCount = o.ChatMessages.Sum(m => m.ViewedDate.HasValue ? 0 : 1),
          },
        })
        .ToListAsync();

      foreach (var session in sessions)
      {
        session.Summary.LastMessageDate = session.LastMessage.MessageDate;
        session.Summary.WaitingForResponse = !session.Summary.EndDate.HasValue &&
          session.LastMessage.UserSenderId == session.MemberUserId;
      }

      var result = new MgmtSessionList()
      {
        Sessions = sessions.Select(o => o.Summary).ToList(),
      };
      return result;
    }

    public async Task<MgmtSessionDetail> GetSessionDetailAsync(int sessionId)
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      var result = await Context.ChatSessions
        .Where(o => o.Id == sessionId)
        .Select(o => new MgmtSessionDetail()
        {
          Id = o.Id,
          MemberName = o.MyPhaMemberUser.FullName,
          IntendedUserId = o.IntendedManagementPortalUserId,
          IntendedUserName = o.IntendedManagementPortalUser.FullName,
          ActualUserId = o.ActualManagementPortalUserId,
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
          CurrentUserId = mgmtUserId,
        })
        .FirstOrDefaultAsync();

      using (var saveContext = Context.Clone())
      {
        var now = DateTimeOffset.Now;

        var unreadMessages = await saveContext.ChatMessages
          .Where(o => o.ChatSessionId == sessionId && !o.ViewedDate.HasValue)
          .ToListAsync();
        foreach (var unreadMessage in unreadMessages)
        {
          unreadMessage.ViewedDate = now;
          unreadMessage.UpdatedTimestamp = now;
        }

        await saveContext.SaveChangesAsync();
      }

      return result;
    }

    public async Task<MgmtUserSessionState> GetUserSessionState()
    {
      int mgmtUserId = this.IdentityService.UserId.Value;
      await VerifyManagementUserAsync(mgmtUserId);

      var lastMessageQuery = Context.ChatMessages
        .GroupBy(o => o.ChatSessionId)
        .Select(g => new
        {
          SessionId = g.Key,
          LastMessageId = g.Max(o => o.Id),
        });

      var delayLimTime = DateTimeOffset.Now.Subtract(Config.GetChatSessionAlertDelay());

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

      var result = await Context.ChatSessions
        .Where(o => !o.EndDate.HasValue)
        .Join(lastMessageQuery, s => s.Id, m => m.SessionId, (s, m) => new { Session = s, m.LastMessageId })
        .Join(Context.ChatMessages, s => s.LastMessageId, m => m.Id, (s, m) => new { Session = s.Session, Message = m })
        .Select(o => new
        {
          IsMine = (o.Session.ActualManagementPortalUserId ?? o.Session.IntendedManagementPortalUserId) == mgmtUserId,
          LastMessage = new
          {
            FromMember = o.Message.UserSenderId == o.Session.MyPhaMemberUserId,
            SentDate = o.Message.SentDate ?? o.Message.CreatedTimestamp,
          },
        })
        .Where(o => o.IsMine || (o.LastMessage.FromMember && o.LastMessage.SentDate < delayLimTime))
        .GroupBy(o => new { IsMine = o.IsMine, IsWaiting = o.LastMessage.FromMember, })
        .Select(g => new
        {
          IsMine = g.Key.IsMine,
          IsWaiting = g.Key.IsWaiting,
          SessionCount = g.Count(),
        })
        .GroupBy(o => 1)
        .Select(g => new MgmtUserSessionState()
        {
          MyOpenChatCount = g.Sum(o => o.IsMine ? o.SessionCount : 0),
          MyWaitingChatCount = g.Sum(o => o.IsMine && o.IsWaiting ? o.SessionCount : 0),
          OtherOpenChatCount = g.Sum(o => !o.IsMine ? o.SessionCount : 0),
          OtherWaitingChatCount = g.Sum(o => !o.IsMine && o.IsWaiting ? o.SessionCount : 0),
        })
        .FirstOrDefaultAsync();
      return result;
    }
  }
}
