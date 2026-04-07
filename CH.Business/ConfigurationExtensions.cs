using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using CH.Models.Common;

namespace CH.Business
{
  public static class ConfigurationExtensions
  {
    #region ApplicationSettings

    public static string GetBaseUrl(this IConfiguration config)
    {
      return config["ApplicationSettings:BaseUrl"];
    }


    public static int GetRequiredPasswordSets(this IConfiguration config)
    {
      return int.Parse(config["ApplicationSettings:RequiredPasswordSets"]);
    }

    public static TimeSpan GetDeleteMaximumAge(this IConfiguration config)
    {
      return TimeSpan.Parse(config["ApplicationSettings:DeleteMaximumAge"]);
    }

    public static TimeSpan GetCacheExpirationTimeSpan(this IConfiguration config)
    {
      return TimeSpan.Parse(config["ApplicationSettings:CacheExpirationTimeSpan"]);
    }

    #endregion


    #region Connection Strings

    public static string GetAppConnection(this IConfiguration config)
    {
      return config["ConnectionStrings:AppConnection"];
    }

    public static string GetSnowflakeConnection(this IConfiguration config)
    {
      return config["ConnectionStrings:SnowflakeConnection"];
    }

    #endregion


    #region Google AuthKey
    public static string GetGoogleAuthKey(this IConfiguration config)
    {
      return config["GoogleAuthKey:Key"];
    }
    #endregion


    #region Lockout Options

    public static bool GetAllowedForNewUsers(this IConfiguration config)
    {
      return bool.Parse(config["LockoutOptions:AllowedForNewUsers"]);
    }

    public static TimeSpan GetDefaultLockoutTimeSpan(this IConfiguration config)
    {
      return TimeSpan.Parse(config["LockoutOptions:DefaultLockoutTimeSpan"]);
    }

    public static int GetMaxFailedAccessAttempts(this IConfiguration config)
    {
      return int.Parse(config["LockoutOptions:MaxFailedAccessAttempts"]);
    }

    #endregion


    #region JWT Token

    public static string GetJwtTokenKey(this IConfiguration config)
    {
      return config["JwtToken:Key"];
    }

    public static string GetJwtTokenIssuer(this IConfiguration config)
    {
      return config["JwtToken:Issuer"];
    }

    public static string GetJwtTokenAudience(this IConfiguration config)
    {
      return config["JwtToken:Audience"];
    }

    public static int GetJwtTokenExpirationMinutes(this IConfiguration config)
    {
      return int.Parse(config["JwtToken:ExpirationMinutes"]);
    }

    #endregion


    #region Email

    public static string GetEmailSmtpHost(this IConfiguration config)
    {
      return config["Email:Smtp:Host"];
    }

    public static int GetEmailSmtpPort(this IConfiguration config)
    {
      return int.Parse(config["Email:Smtp:Port"]);
    }

    public static string GetEmailSmtpUsername(this IConfiguration config)
    {
      return config["Email:Smtp:Username"];
    }

    public static string GetEmailSmtpPassword(this IConfiguration config)
    {
      return config["Email:Smtp:Password"];
    }

    public static string GetEmailSmtpFrom(this IConfiguration config)
    {
      return config["Email:Smtp:From"];
    }

    public static bool GetEmailSmtpEnableSsl(this IConfiguration config)
    {
      return bool.Parse(config["Email:Smtp:EnableSsl"]);
    }

    #endregion


    #region Tableau

    public static string GetTableauServerUrl(this IConfiguration config)
    {
      return config["Tableau:ServerUrl"];
    }

    public static string GetTableauSiteId(this IConfiguration config)
    {
      return config["Tableau:SiteId"];
    }

    public static string GetTableauUsername(this IConfiguration config)
    {
      return config["Tableau:Username"];
    }

    // TODO: Make these database-configurable?
    public static string GetTableauPhsViewWorkbook(this IConfiguration config)
    {
      return config["Tableau:PhsView:WorkbookUri"];
    }

    public static string GetTableauPhsViewSheet1(this IConfiguration config)
    {
      return config["Tableau:PhsView:Sheet1Uri"];
    }

    public static string GetTableauPhsViewSheet2(this IConfiguration config)
    {
      return config["Tableau:PhsView:Sheet2Uri"];
    }

    public static string GetTableauPhsViewSheet3(this IConfiguration config)
    {
      return config["Tableau:PhsView:Sheet3Uri"];
    }

    public static string GetChMemberIdParamName(this IConfiguration config)
    {
      return config["Tableau:PhsView:ChMemberIdParamName"];
    }

    #endregion


    #region Chat

    public static string GetChatPhaGroupName(this IConfiguration config)
    {
      return config["Chat:PhaGroupName"];
    }

    public static string GetChatHubUrl(this IConfiguration config)
    {
      return config["Chat:HubUrl"];
    }

    /// <summary>
    /// The domain to add to CORS configuration.
    /// </summary>
    public static string GetChatManagementDomain(this IConfiguration config)
    {
      return config["Chat:ManagementDomain"];
    }

    /// <summary>
    /// How long a member has to respond after the PHA's last message before
    /// the session is automatically closed.
    /// </summary>
    public static TimeSpan GetChatAbandonedSessionTimeout(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:AbandonedSessionTimeout"]);
    }

    /// <summary>
    /// The frequency to use when checking for abandoned sessions.
    /// </summary>
    public static TimeSpan GetChatAbandonedSessionInterval(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:AbandonedSessionInterval"]);
    }

    /// <summary>
    /// How long a member should wait after their last message before the session
    /// is shown in the header alert for other PHAs to respond to.
    /// </summary>
    public static TimeSpan GetChatSessionAlertDelay(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:SessionAlertDelay"]);
    }

    /// <summary>
    /// The frequency to use when checking for delayed sessions.
    /// </summary>
    public static TimeSpan GetChatSessionAlertInterval(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:SessionAlertInterval"]);
    }

    /// <summary>
    /// Start time for chat - should be in HH:MM:SS format.
    /// </summary>
    public static TimeSpan GetChatMinTimeOfDay(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:MinTimeOfDay"]);
    }

    /// <summary>
    /// End time for chat - should be in HH:MM:SS format.
    /// </summary>
    public static TimeSpan GetChatMaxTimeOfDay(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:MaxTimeOfDay"]);
    }

    public static int[] GetChatAllowedOnDays(this IConfiguration config)
    {
      return config.GetSection("Chat:AllowedOnDays")?.GetChildren()?
        .Select(o => int.Parse(o.Value)).ToArray();
    }

    public static TimeSpan GetChatEstimatedResponseTime(this IConfiguration config)
    {
      return TimeSpan.Parse(config["Chat:EstimatedResponseTime"]);
    }

    public static ChatConfiguration GetChatConfiguration(this IConfiguration config)
    {
      var minTimeOfDay = config.GetChatMinTimeOfDay();
      var maxTimeOfDay = config.GetChatMaxTimeOfDay();

      var allowedDays = config.GetChatAllowedOnDays();
      string minDayOfWeek = ((DayOfWeek)allowedDays.Min()).ToString();
      string maxDayOfWeek = ((DayOfWeek)allowedDays.Max()).ToString();

      var estResponseTime = config.GetChatEstimatedResponseTime();

      return new ChatConfiguration()
      {
        MinTimeOfDay = minTimeOfDay,
        MinTimeOfDayFormatted = minTimeOfDay.FormatClockTime(),
        MaxTimeOfDay = maxTimeOfDay,
        MaxTimeOfDayFormatted = maxTimeOfDay.FormatClockTime(),

        AllowedOnDays = string.Join(",", allowedDays),
        MinDayOfWeek = minDayOfWeek,
        MaxDayOfWeek = maxDayOfWeek,

        EstimatedResponseTime = estResponseTime,
        EstimatedResponseTimeFormatted = estResponseTime.FormatInterval(),

        //AbandonedSessionTimeout = config.GetChatAbandonedSessionTimeout(),
        //AbandonedSessionInterval = config.GetChatAbandonedSessionInterval(),
        //SessionAlertDelay = config.GetChatSessionAlertDelay(),
        //SessionAlertInterval = config.GetChatSessionAlertInterval(),

        IsChatAvailable = config.IsChatAvailable(DateTime.Now),
      };
    }

    public static bool IsChatAvailable(this IConfiguration config,
      DateTime dateTimeToCheck)
    {
      var minTimeOfDay = config.GetChatMinTimeOfDay();
      var maxTimeOfDay = config.GetChatMaxTimeOfDay();
      var allowedDays = config.GetChatAllowedOnDays();

      var timeToCheck = dateTimeToCheck.TimeOfDay;

      return allowedDays.Contains((int)dateTimeToCheck.DayOfWeek)
        && minTimeOfDay <= timeToCheck && timeToCheck < maxTimeOfDay;
    }

    #endregion // Chat


    #region Support

    public static string GetSupportEmail(this IConfiguration config)
    {
      return config["Support:Email"];
    }

    public static string GetSupportPhone(this IConfiguration config)
    {
      return config["Support:Phone"];
    }

    public static string GetSupportMailTo(this IConfiguration config)
    {
      return config["Support:Mail:To"];
    }

    public static string GetSupportMailAddress1(this IConfiguration config)
    {
      return config["Support:Mail:Address1"];
    }

    public static string GetSupportMailAddress2(this IConfiguration config)
    {
      return config["Support:Mail:Address2"];
    }

    public static string GetSupportMailCity(this IConfiguration config)
    {
      return config["Support:Mail:City"];
    }

    public static string GetSupportMailState(this IConfiguration config)
    {
      return config["Support:Mail:State"];
    }

    public static string GetSupportMailZip(this IConfiguration config)
    {
      return config["Support:Mail:Zip"];
    }

    public static SupportConfiguration GetSupportConfiguration(this IConfiguration config)
    {
      return new SupportConfiguration()
      {
        Email = config.GetSupportEmail(),
        Phone = config.GetSupportPhone(),
        MailTo = config.GetSupportMailTo(),
        MailAddress1 = config.GetSupportMailAddress1(),
        MailAddress2 = config.GetSupportMailAddress2(),
        MailCity = config.GetSupportMailCity(),
        MailState = config.GetSupportMailState(),
        MailZip = config.GetSupportMailZip(),
      };
    }

    #endregion

  }
}
