using CH.Business.MyPhaApp;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CH.Business.Services.MyPhaApp
{
  public interface IApplicationContext
  {
  }

  public class ApplicationContext : IApplicationContext, IHostedService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private Timer _closeAbandonedSessionsTimer;
    private Timer _checkForDelayedSessionsTimer;


    public ApplicationContext(IServiceProvider serviceProvider, IConfiguration config)
    {
      _serviceProvider = serviceProvider;
      _config = config;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
      _closeAbandonedSessionsTimer = new Timer(ProcessSessionCloseAsync, null, TimeSpan.Zero,
        _config.GetChatAbandonedSessionInterval());
      _checkForDelayedSessionsTimer = new Timer(ProcessDelayedSessionCheckAsync, null, TimeSpan.Zero,
        _config.GetChatSessionAlertInterval());

      return Task.CompletedTask;
    }


    private async void ProcessSessionCloseAsync(object state)
    {
      try
      {
        // Create a new scope to retrieve scoped services
        using (var scope = _serviceProvider.CreateScope())
        {
          var chatManager = scope.ServiceProvider.GetRequiredService<IChatManager>();
          await chatManager.CloseAbandonedSessionsAsync();
        }
      }
      catch (Exception ex)
      {
        // TODO: Log error
      }
    }

    private async void ProcessDelayedSessionCheckAsync(object state)
    {
      try
      {
        // Create a new scope to retrieve scoped services
        using (var scope = _serviceProvider.CreateScope())
        {
          var chatManager = scope.ServiceProvider.GetRequiredService<IChatManager>();
          await chatManager.CheckForOtherWaitingSessionsAsync();
        }
      }
      catch (Exception ex)
      {
        // TODO: log error
      }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}
