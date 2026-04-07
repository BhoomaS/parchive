using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace CH.Business.Services
{
  public interface IChatClient
  {
    // The following messages are received by member users.
    Task SessionStarted(int sessionId, DateTimeOffset startDate, string fromName,
      string intendedUserName, DateTimeOffset messageDate, string messageText);
    Task MessageReceived(string fromName, bool fromMember, DateTimeOffset sentDate,
      string messageText);
    Task SessionClosed(int sessionId, DateTimeOffset endDate);

    // The following messages are received by management users.
    Task MgmtSessionAvailable(int sessionId, DateTimeOffset startDate, int intendedUserId);
    Task MgmtMessageReceived(int sessionId, string fromName, bool fromMember,
      DateTimeOffset sentDate, string messageText, int? actualManagementPortalUserId);
    Task MgmtSessionClosed(int sessionId, DateTimeOffset endDate);
    Task MgmtDelayedSessions();
  }


  public class ChatHub : Hub<IChatClient>
  {
    private readonly Business.MyPhaApp.IChatManager _chatMgr;
    private readonly IConfiguration _config;

    public ChatHub(Business.MyPhaApp.IChatManager chatMgr, IConfiguration config)
    {
      _chatMgr = chatMgr;
      _config = config;
    }

    #region Member

    public async Task MemberNewSession(string connectionId, string messageText)
    {
      var session = await _chatMgr.CreateNewMemberSessionAsync(connectionId);
      var message = await _chatMgr.SaveMemberMessageAsync(connectionId, session.SessionId, messageText);

      // Send message to the member.
      await Clients.Group(session.SessionKey).SessionStarted(session.SessionId,
        session.StartDate, message.Message.FromName, session.IntendedUserName,
        message.Message.SentDate, message.Message.Message);

      // Send message to management users.
      await Clients.Group(_config.GetChatPhaGroupName()).MgmtSessionAvailable(session.SessionId,
          session.StartDate, session.IntendedUserId);
    }

    public async Task MemberJoinSession(string connectionId, int sessionId)
    {
      await _chatMgr.JoinSessionAsync(connectionId, sessionId);
    }

    public async Task MemberNewMessage(string connectionId, int sessionId, string messageText)
    {
      var message = await _chatMgr.SaveMemberMessageAsync(connectionId, sessionId, messageText);

      // Send message to the member.
      await Clients.Group(message.SessionKey).MessageReceived(message.Message.FromName,
        message.Message.FromMember, message.Message.SentDate, message.Message.Message);

      // Send message to management users.
      await Clients.Group(_config.GetChatPhaGroupName()).MgmtMessageReceived(sessionId,
        message.Message.FromName, message.Message.FromMember,
        message.Message.SentDate, message.Message.Message,
        message.Message.ActualManagementPortalUserId);
    }

    #endregion // Member


    #region Management

    public async Task MgmtNewSession(string connectionId)
    {
      await _chatMgr.MgmtJoinGroupAsync(connectionId);
    }

    public async Task MgmtJoinSession(string connectionId, int sessionId)
    {
      await _chatMgr.MgmtJoinSessionAsync(connectionId, sessionId);
    }

    public async Task MgmtLeaveSession(string connectionId, int sessionId)
    {
      await _chatMgr.MgmtLeaveSessionAsync(connectionId, sessionId);
    }

    public async Task MgmtCloseSession(string connectionId, int sessionId)
    {
      var result = await _chatMgr.MgmtCloseSessionAsync(connectionId, sessionId);

      // Send message to the member.
      await Clients.Group(result.SessionKey).SessionClosed(sessionId, result.EndDate);

      // Send message to management users.
      await Clients.Group(_config.GetChatPhaGroupName()).MgmtSessionClosed(sessionId,
        result.EndDate);
    }

    public async Task MgmtNewMessage(string connectionId, int sessionId, string messageText)
    {
      var message = await _chatMgr.MgmtSaveMessageAsync(connectionId, sessionId, messageText);

      // Send message to the member.
      await Clients.Group(message.SessionKey).MessageReceived(message.Message.FromName,
        message.Message.FromMember, message.Message.SentDate, message.Message.Message);

      // Send message to management users.
      await Clients.Group(_config.GetChatPhaGroupName()).MgmtMessageReceived(sessionId,
        message.Message.FromName, message.Message.FromMember, message.Message.SentDate,
        message.Message.Message, message.Message.ActualManagementPortalUserId);
    }

    #endregion // Management
  }
}
