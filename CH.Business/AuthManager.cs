using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using AutoMapper;
using CH.Business.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using CH.Data;
using CH.Models.Auth;
using CH.Models.Enums;
using Google.Authenticator;
using System.Runtime.Intrinsics.X86;

namespace CH.Business
{
  public interface IAuthManager
  {
    //Task<LoginResult> LoginAsync(Login loginModel, ApplicationRoleId roleId);
    Task<LoginResult> LoginAsync(Login loginModel, ApplicationRoleId roleId, string UserUniqueKey, string SetupCode, string BarcodeImageUrl);
    Task<ForgotPasswordResult> SendResetPasswordLinkAsync(ForgotPassword detail);
    Task<ResetPasswordResult> ResetPasswordAsync(ResetPassword detail);
    Task<ChangePasswordResult> ChangePasswordAsync(ChangePassword detail);

    Task<ApplicationUser> GetApplicationUserDetailAsync(int userId);
    Task<ApplicationUserSaveResult> SaveApplicationUserDetailAsync(ApplicationUser detail,
      ApplicationRoleId roleId, IAppDbContext dbContext = null);
    Task<bool> RemoveApplicationUserRoleAsyn(int Id);

    Task<MFAResult> MFAAsync(MFA mfaAuthModel);


    #region PHA User
    Task<PhaUser> GetPhaUserDetailAsync(int userId);
    Task<PhaUser> GetCurrentPhaUserDetailAsync();
    Task<PhaUser> GetCurrentPhaAdminUserDetailAsync();
    Task<IEnumerable<PhaUser>> GetPhaUsersAsync(PhaUserFilter filter);
    Task<IEnumerable<PhaUser>> GetPhaPortalUsersAsync(PhaUserFilter filter);
    Task<IEnumerable<PhaUser>> GetPhaAdminUsersAsync(PhaUserFilter filter);
    Task<PhaUserSaveResult> SavePhaUserDetailAsync(PhaUser model);
    
    Task<PhaUserSaveResult> UpdateTwoFactorEnabledAsync(PhaUser model);
    Task<PhaUserSaveResult> UpdateUserStatusAsync(PhaUser model);
    #endregion
    Task<ApplicationUserSaveResult> UpdateTwoFactorEnabledStatusAsync(ApplicationUser detail, IAppDbContext dbContext = null);

    #region Member User
    Task<IEnumerable<MemberUser>> GetMemberUsersAsync(MemberUserFilter filter);
    Task<MemberUser> GetMemberUserDetailAsync(int userId);
    Task<SaveMemberUserResult> SaveMemberUserDetailAsync(MemberUser model);
    Task<ValidateMemberRegistrationProfileResult> ValidateMemberRegistrationProfile(
      MemberRegistrationProfile model);
    Task<SaveMemberRegistrationResult> SaveMemberRegistration(MemberRegistration model);
    #endregion


    // TODO: Add these when needed
    //Task<bool> AddUserToRoleAsync(int userId, ApplicationRoleId roleId);
    //Task<bool> RemoveUserFromRoleAsync(int userId, ApplicationRoleId roleId);
  }


  public class AuthManager : BaseManager, IAuthManager
  {
    private readonly UserManager<Entities.ApplicationUser> _userManager;
    private readonly SignInManager<Entities.ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthManager> _logger;

    // There are several scenarios we try to respond to below
    // In case someone might be trying to brute-force...
    // We don't want to indicate exactly why something fails, which might indicate
    // that they've made progress trying to guess a real account email.
    private const string _noAccountErrorMsg = "No account found for the email address provided";
    private const string _cannotAuthErrorMsg = "Unable to authenticate with the email and password provided";
    private const string _invalidPasswordErrorMsg = "Password does not meet the requirements";


    public AuthManager(
      AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IMapper mapper,
      ILogger<AuthManager> logger,
      UserManager<Entities.ApplicationUser> userManager,
      SignInManager<Entities.ApplicationUser> signInManager,
      IEmailService emailService)
      : base(context, identityService, cacheService, config, mapper)
    {
      _signInManager = signInManager;
      _emailService = emailService;
      _userManager = userManager;
      _logger = logger;
    }


    private Entities.ApplicationUser _currentUser;
    private object _currentUserLock = new object();
    private Entities.ApplicationUser CurrentUser
    {
      get
      {
        if (_currentUser == null)
        {
          lock (_currentUserLock)
          {
            if (_currentUser == null)
            {
              _currentUser = _userManager.Users
                .FirstOrDefault(o => o.Id == IdentityService.UserId);
            }
          }
        }

        return _currentUser;
      }
    }

    private string ConvertApplicationRole(ApplicationRoleId roleId)
    {
      switch (roleId)
      {
        case ApplicationRoleId.ManagementPortalUser:
          return ApplicationRole.ManagementPortalUser;

        case ApplicationRoleId.MyPhaMemberUser:
          return ApplicationRole.MyPhaMemberUser;

        case ApplicationRoleId.ManagementPortalAdmin:
          return ApplicationRole.ManagementPortalAdmin;
      }

      throw new InvalidOperationException("Invalid role provided");
    }

    private async Task<bool> CurrentUserHasRole(ApplicationRoleId role)
    {
      if (CurrentUser != null)
        return await _userManager.IsInRoleAsync(CurrentUser,
          ConvertApplicationRole(role));
      return false;
    }

    #region MFA

    public async Task<MFAResult> MFAAsync(MFA mfaAuthModel)
    {
      Action<MFAResult, string> addError = (res, msg) =>
      {
        res.Succeeded = false;
        res.Message = msg;
        res.IsValidTwoFactorAuthentication = false;
      };

      var result = new MFAResult()
      {

        Succeeded = false,
        Message = string.Empty,
        IsValidTwoFactorAuthentication = false
      };


      TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
      string UserUniqueKey = (mfaAuthModel.UserUniqueKey).ToString();
      bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, mfaAuthModel.CodeDigit, false);

      if (isValid)
      {
        //HttpCookie TwoFCookie = new HttpCookie("TwoFCookie");
        //string UserCode = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(UserUniqueKey)));

        result.Succeeded = true;
        result.Message = string.Empty;
        result.IsValidTwoFactorAuthentication = true;
      }
      else
      {
        addError(result, "An unexpected error occurred while authenticating");
      }

      return result;

    }

    #endregion

    #region Login & Password Management

    //public async Task<LoginResult> LoginAsync(Login model, ApplicationRoleId roleId)
    //{
    //  // TODO: Remove the dependency on this delegate by only working with the result's ModelErrors
    //  Action<LoginResult, string> addError = (res, msg) =>
    //  {
    //    res.Succeeded = false;
    //    res.Token = string.Empty;
    //    res.ValidTo = null;
    //    res.Message = msg;
    //    res.ModelErrors.Add(o => o.Email, msg);
    //  };

    //  var result = new LoginResult()
    //  {
    //    Detail = model,

    //    Succeeded = false,
    //    Token = string.Empty,
    //    Message = string.Empty,
    //    ValidTo = null,
    //  };
    //  string password = result.Detail.Password;
    //  result.Detail.Password = string.Empty; // Do not send the password back down to the client

    //  Entities.ApplicationUser user = null;

    //  // TODO: Allow fetching by email OR username for now
    //  // MyPHA must be mail per client-side validation
    //  // Management Portal can use either username or email
    //  if (!string.IsNullOrWhiteSpace(model.Email))
    //    user = await _userManager.FindByEmailAsync(model.Email);
    //  if (!string.IsNullOrWhiteSpace(model.UserName))
    //    user = await _userManager.FindByNameAsync(model.UserName);

    //  if (user == null || !user.IsEnabled)
    //    addError(result, _cannotAuthErrorMsg);

    //  if (result.ModelErrors.Count == 0)
    //  {
    //    try
    //    {
    //      var signInResult = await _signInManager.PasswordSignInAsync(user, password, true, true);

    //      if (signInResult.Succeeded)
    //      {
    //        if (await _userManager.IsInRoleAsync(user,
    //          ConvertApplicationRole(roleId)))
    //        {
    //          // If the user is in the role provided by the calling method...

    //          // Create the token
    //          var claims = new List<Claim>()
    //          {
    //            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
    //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //            new Claim(JwtRegisteredClaimNames.UniqueName, user.NormalizedEmail),
    //            new Claim(IdentityClaimKeys.UserId, user.Id.ToString()),
    //          };

    //          if (roleId == ApplicationRoleId.MyPhaMemberUser)
    //          {
    //            claims.Add(new Claim(IdentityClaimKeys.ChMemberId,
    //              user.ChMemberId.ToString()));

    //            var snowflakeMember = await Context.SnowflakeMembers.FirstOrDefaultAsync(o => o.ChMemberId == user.ChMemberId);
    //            if (SnowflakeMemberIsTermed(snowflakeMember))
    //              addError(result, "The account provided is no longer authorized to use the MyPHA app");
    //          }
    //          else if (roleId == ApplicationRoleId.ManagementPortalUser)
    //          {
    //            // Nothing to do for this role
    //          }
    //          else if (roleId == ApplicationRoleId.ManagementPortalAdmin)
    //          {
    //            // Nothing to do for this role
    //          }

    //          if (result.ModelErrors.Count == 0)
    //          {
    //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.GetJwtTokenKey()));
    //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //            var token = new JwtSecurityToken(
    //              Config.GetJwtTokenIssuer(),
    //              Config.GetJwtTokenAudience(),
    //              claims,
    //              expires: DateTime.UtcNow.AddMinutes(Config.GetJwtTokenExpirationMinutes()),
    //              signingCredentials: creds);

    //            result.Succeeded = true;
    //            result.Token = new JwtSecurityTokenHandler().WriteToken(token);
    //            result.Message = string.Empty;
    //            result.ValidTo = token.ValidTo;

    //            await AddAuditLogAsync((int)AuditType.SuccessfulLogin, userId: user.Id);
    //          }
    //        }
    //        else
    //          addError(result, "The specified account is not authorized for this app.");
    //      }
    //      else if (signInResult.IsLockedOut)
    //        addError(result, "The account provided is locked out. Please try again later.");
    //      else
    //        addError(result, _cannotAuthErrorMsg);
    //    }
    //    catch (Exception ex)
    //    {
    //      // Unexpected error occurred, e.g. empty/null SecurityStamp
    //      addError(result, "An unexpected error occurred while authenticating");
    //    }
    //  }

    //  if (result.ModelErrors.Count > 0)
    //    await AddAuditLogAsync((int)AuditType.FailedLogin, model.Email ?? model.UserName);

    //  return result;
    //}


    public async Task<LoginResult> LoginAsync(Login model, ApplicationRoleId roleId, string UserUniqueKey, string SetupCode, string BarcodeImageUrl)
    {
      // TODO: Remove the dependency on this delegate by only working with the result's ModelErrors
      Action<LoginResult, string> addError = (res, msg) =>
      {
        res.Succeeded = false;
        res.Token = string.Empty;
        res.ValidTo = null;
        res.Message = msg;
        res.ModelErrors.Add(o => o.Email, msg);
        res.SetupCode = string.Empty;
        res.UserUniqueKey = string.Empty;
        res.BarcodeImageUrl = null;
        res.Id = 0;
        res.IsTwoFactorEnabled = false;
      };

      var result = new LoginResult()
      {
        Detail = model,

        Succeeded = false,
        Token = string.Empty,
        Message = string.Empty,
        ValidTo = null,
        SetupCode = string.Empty,
        UserUniqueKey = string.Empty,
        BarcodeImageUrl = null,
        Id = 0,
        IsTwoFactorEnabled = false,
      };
      string password = result.Detail.Password;
      result.Detail.Password = string.Empty; // Do not send the password back down to the client

      Entities.ApplicationUser user = null;

      // TODO: Allow fetching by email OR username for now
      // MyPHA must be mail per client-side validation
      // Management Portal can use either username or email
      if (!string.IsNullOrWhiteSpace(model.Email))
        user = await _userManager.FindByEmailAsync(model.Email);
      if (!string.IsNullOrWhiteSpace(model.UserName))
        user = await _userManager.FindByNameAsync(model.UserName);

      if (user == null || !user.IsEnabled)
        addError(result, _cannotAuthErrorMsg);

      if (result.ModelErrors.Count == 0)
      {
        try
        {
          var signInResult = await _signInManager.PasswordSignInAsync(user, password, true, true);

          if (signInResult.Succeeded)
          {
            if (await _userManager.IsInRoleAsync(user,
              ConvertApplicationRole(roleId)))
            {
              // If the user is in the role provided by the calling method...

              // Create the token
              var claims = new List<Claim>()
              {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.NormalizedEmail),
                new Claim(IdentityClaimKeys.UserId, user.Id.ToString()),
              };

              if (roleId == ApplicationRoleId.MyPhaMemberUser)
              {
                claims.Add(new Claim(IdentityClaimKeys.ChMemberId,
                  user.ChMemberId.ToString()));

                var snowflakeMember = await Context.SnowflakeMembers.FirstOrDefaultAsync(o => o.ChMemberId == user.ChMemberId);
                if (SnowflakeMemberIsTermed(snowflakeMember))
                  addError(result, "The account provided is no longer authorized to use the MyPHA app");
              }
              else if (roleId == ApplicationRoleId.ManagementPortalUser)
              {
                // Nothing to do for this role
              }
              else if (roleId == ApplicationRoleId.ManagementPortalAdmin)
              {
                // Nothing to do for this role
              }

              if (result.ModelErrors.Count == 0)
              {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.GetJwtTokenKey()));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                  Config.GetJwtTokenIssuer(),
                  Config.GetJwtTokenAudience(),
                  claims,
                  expires: DateTime.UtcNow.AddMinutes(Config.GetJwtTokenExpirationMinutes()),
                  signingCredentials: creds);

                result.Succeeded = true;
                result.Token = new JwtSecurityTokenHandler().WriteToken(token);
                result.Message = string.Empty;
                result.ValidTo = token.ValidTo;
                result.SetupCode = SetupCode;
                result.UserUniqueKey = UserUniqueKey;
                result.BarcodeImageUrl = BarcodeImageUrl;
                result.Id = user.Id;
                result.IsTwoFactorEnabled = user.TwoFactorEnabled;

                await AddAuditLogAsync((int)AuditType.SuccessfulLogin, userId: user.Id);
              }
            }
            else
              addError(result, "The specified account is not authorized for this app.");
          }
          else if (signInResult.IsLockedOut)
            addError(result, "The account provided is locked out. Please try again later.");
          else
            addError(result, _cannotAuthErrorMsg);
        }
        catch (Exception ex)
        {
          // Unexpected error occurred, e.g. empty/null SecurityStamp
          addError(result, "An unexpected error occurred while authenticating");
        }
      }

      if (result.ModelErrors.Count > 0)
        await AddAuditLogAsync((int)AuditType.FailedLogin, model.Email ?? model.UserName);

      return result;
    }


    public async Task<ForgotPasswordResult> SendResetPasswordLinkAsync(ForgotPassword detail)
    {
      var result = new ForgotPasswordResult()
      {
        Detail = detail,
      };

      // Set up result, check for bad input
      // TODO: This should be covered by model Required attribute
      //if (string.IsNullOrWhiteSpace(detail.Email))
      //{
      //	result.ModelErrors.Add(o => o.Email, "Email must be provided");
      //}

      if (result.ModelErrors.Count == 0)
      {
        // Look up the user by email.
        var user = await _userManager.FindByEmailAsync(detail.Email);

        if (user == null)
          // No user found
          result.ModelErrors.Add(o => o.Email, _noAccountErrorMsg);

        if (result.ModelErrors.Count == 0)
        {
          // Check to see if there's any reason not to send a reset link
          // Locked out? NO, let a user reset password & unlock themselves at the same time

          // No role
          var roles = await _userManager.GetRolesAsync(user);
          if (roles.Count == 0)
            // No usable roles for the specified user
            result.ModelErrors.Add(o => o.Email, _noAccountErrorMsg);

          // Deactivated (by PHA, for example)
          // TODO: Deactivating not yet implemented. Needs to be done soon.

          if (await _userManager.IsInRoleAsync(user, ConvertApplicationRole(ApplicationRoleId.MyPhaMemberUser)))
          {
            // Termed or any other reason the data indicates the user shouldn't have access
            if (!user.ChMemberId.HasValue)
              // Member user, but no snowflake.CH_MEMBER record, checking just in case the underlying data gets corrupted
              result.ModelErrors.Add(o => o.Email, _noAccountErrorMsg);
          }

          if (result.ModelErrors.Count == 0)
          {
            // Get the token
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string url = $"{Config.GetBaseUrl()}/auth/reset-password?t=" + HttpUtility.UrlEncode(token);
            // TODO: Replace the URL with a build-configurable value
            // TODO: Get some copy for this message from CH
            string message = $@"<html><body>
<div>Please click the link below to reset your password.</div><br />
<a href='{url}'>{url}</a><br /><br />
<div>If you did not request this link, you may ignore this email. If you have any questions about MyPHA and the privacy of your personal information, please call {Config.GetSupportPhone()} or reply to this email requesting assistance.</div>
</body>
</html>";

            try
            {
              // Send the reset link
              var emailResult = await _emailService.SendEmailAsync(user.Email, "Reset Your MyPHA Password", message, isBodyHtml: true);

              // If successful, great. Do nothing but exit.
              if (!emailResult)
                result.ModelErrors.Add(o => o.Email, "An error occurred while attempting to send your password reset link. Please try again later.");
            }
            catch (Exception e)
            {
              // If not, let's notify the user something went wrong, but there's nothing they can do
              result.ModelErrors.Add(o => o.Email, "An error occurred while attempting to send your password reset link. Please try again later.");
            }
          }
          else
          {
            // This else happens before we attempt to send the email. Some validation caused us to end up here.
            // I.e. if there's a mistake, give a phone number to call or email to reach out to for help?
            // TODO: Do we let the user know if any of the above happened?
          }
        }
      }

      return result;
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPassword detail)
    {
      var result = new ResetPasswordResult
      {
        Detail = detail,
      };

      var user = await _userManager.FindByEmailAsync(detail.Email);

      if (user == null)
        // No user found
        result.ModelErrors.Add(o => o.Email, _noAccountErrorMsg);

      if (result.ModelErrors.Count == 0)
      {
        bool validPassword = await ValidatePasswordAsync(user, detail.Password);

        if (!validPassword)
          result.ModelErrors.Add(o => o.Password, _invalidPasswordErrorMsg);

        if (result.ModelErrors.Count == 0)
        {
          var idResult = await _userManager.ResetPasswordAsync(user, detail.Token, detail.Password);

          if (!idResult.Succeeded)
            result.ModelErrors.Add(o => o.Email, "Unable to reset the password for the specified account. If you continue to receive this error, please request a new password reset link.");
          else
            // Audit the change
            await AddAuditLogAsync((int)AuditType.ResetPassword, userId: user.Id);
        }
      }

      result.Detail.Password = null;
      result.Detail.ConfirmPassword = null;
      result.Detail.Token = null;

      return result;
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(ChangePassword detail)
    {
      var result = new ChangePasswordResult()
      {
        Detail = detail,
      };

      if (string.IsNullOrWhiteSpace(detail.CurrentPassword))
        result.ModelErrors.Add(o => o.CurrentPassword, "Current password is required");

      if (string.IsNullOrWhiteSpace(detail.NewPassword))
        result.ModelErrors.Add(o => o.NewPassword, "New password is required");

      if (string.IsNullOrWhiteSpace(detail.ConfirmNewPassword))
        result.ModelErrors.Add(o => o.ConfirmNewPassword, "Confirm your new password");

      if (!string.Equals(detail.NewPassword, detail.ConfirmNewPassword, StringComparison.Ordinal))
        result.ModelErrors.Add(o => o.NewPassword, "Passwords do not match");

      var user = await _userManager.FindByIdAsync(
        IdentityService.UserId?.ToString());

      if (user == null)
        // No user found
        result.ModelErrors.Add(o => o.NewPassword, _noAccountErrorMsg);

      if (result.ModelErrors.Count == 0)
      {
        bool validPassword = await ValidatePasswordAsync(user, detail.NewPassword);

        if (!validPassword)
          result.ModelErrors.Add(o => o.NewPassword, _invalidPasswordErrorMsg);

        if (result.ModelErrors.Count == 0
          && !await _userManager.CheckPasswordAsync(user, detail.CurrentPassword))
        {
          result.ModelErrors.Add(o => o.CurrentPassword, "Unable to validate your current password. Please try again.");
        }

        if (result.ModelErrors.Count == 0)
        {
          string token = await _userManager.GeneratePasswordResetTokenAsync(user);
          var idResult = await _userManager.ResetPasswordAsync(user, token, detail.NewPassword);

          if (!idResult.Succeeded)
            result.ModelErrors.Add(o => o.NewPassword, "An unexpected error occurred while attempting to save your changes. If you continue to receive this error, please request a new password reset link.");
          else
            // Audit the change
            await AddAuditLogAsync((int)AuditType.ChangedPassword, userId: user.Id);
        }
      }

      result.Detail.CurrentPassword = null;
      result.Detail.NewPassword = null;
      result.Detail.ConfirmNewPassword = null;

      return result;
    }

    #endregion


    #region Common User

    public async Task<ApplicationUser> GetApplicationUserDetailAsync(int userId)
    {
      return await Mapper.ProjectTo<ApplicationUser>(Context.ApplicationUsers)
        .FirstOrDefaultAsync(o => o.Id == userId);
    }

    /// <summary>
    /// Optionally accepts a new IAppDbContext in case you need to do transaction-scoped work
    /// </summary>
    /// <param name="model"></param>
    /// <param name="roleId"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<ApplicationUserSaveResult> SaveApplicationUserDetailAsync(
      ApplicationUser model, ApplicationRoleId roleId, IAppDbContext context = null)
    {
      context ??= Context;

      // Check model is valid (roles, etc.)
      var result = new ApplicationUserSaveResult()
      {
        Detail = model,
      };

      // Who can save?
      //if (!await CurrentUserHasRole(ApplicationRoleId.ManagementPortalUser) // Management portal user
      //  && IdentityService.UserId != model.Id // User managing themselves
      //  && model.Id != 0) // New user registering
      if (!await CurrentUserHasRole(ApplicationRoleId.ManagementPortalAdmin) // Management portal user
        && IdentityService.UserId != model.Id) // User managing themselves
      {
        result.ModelErrors.Add(o => o.Id, "Current user does not have permission to save the given user");
      }

      // Email already in use?
      if (_userManager.Users.Any(o => o.Email == model.Email && o.Id != model.Id))
      {
        result.ModelErrors.Add(o => o.Email, "Email already in use");
      }

      if (model.Password != model.ConfirmPassword)
      {
        result.ModelErrors.Add(o => o.Password, "Passwords do not match");
      }

      int? userId = null;
      if (result.ModelErrors.Count == 0)
      {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
          // Get the entity if exists
          var entity = await context.ApplicationUsers
            .FirstOrDefaultAsync(o => o.Id == model.Id);

          if (entity == null)
          {
            entity = new Entities.ApplicationUser();
            // TODO: Any time when lockout should be disabled?
            // I.e. not applicable to the user, i.e. user cannot be locked out automatically
            entity.LockoutEnabled = true;
            entity.IsEnabled = true;
            context.ApplicationUsers.Add(entity);
          }

          entity.UserName = model.UserName;
          entity.NormalizedUserName = model.UserName.ToUpper();
          entity.FullName = model.FullName;
          entity.Email = model.Email;
          entity.NormalizedEmail = model.Email.ToUpper();
          entity.PhoneNumber = model.PhoneNumber ?? entity.PhoneNumber;
          entity.IsEnabled = model.IsEnabled ?? entity.IsEnabled;

          if (model.IsLocked.HasValue && model.IsLocked.Value == false)
            // End the lockout if the user explicitly unchecked the box for IsLocked
            entity.LockoutEnd = null;

          // Add the specified role, if missing
          if (entity.Id == 0 && !await _userManager.IsInRoleAsync(entity,
            ConvertApplicationRole(roleId)))
          {
            entity.ApplicationUserRoles.Add(new Entities.ApplicationUserRole
            {
              ApplicationRole = await context.ApplicationRoles.FirstOrDefaultAsync(o => o.Id == (int)roleId),
              ApplicationUser = entity,
            });
          }

          if (entity.Id > 0)
          {
            if (!await _userManager.IsInRoleAsync(entity,
             ConvertApplicationRole(roleId)))
            {
              entity.ApplicationUserRoles.Add(new Entities.ApplicationUserRole
              {
                ApplicationRole = await context.ApplicationRoles.FirstOrDefaultAsync(o => o.Id == (int)roleId),
                ApplicationUser = entity,
              });

              bool userRemove = await RemoveApplicationUserRoleAsyn(entity.Id);  //Remove Existing user role
            }

          }


          if (entity.Id == 0 && string.IsNullOrWhiteSpace(model.Password))
            result.ModelErrors.Add(o => o.Password,
              "Password must be provided when creating a new user");

          bool changedPassword = false;
          if (!string.IsNullOrWhiteSpace(model.Password))
          {
            bool validPassword = await ValidatePasswordAsync(entity, model.Password);
            if (!validPassword)
              result.ModelErrors.Add(o => o.Password, _invalidPasswordErrorMsg);

            if (result.ModelErrors.Count == 0)
            {
              entity.SecurityStamp = Guid.NewGuid().ToString("D");
              entity.PasswordHash = _userManager.PasswordHasher.HashPassword(
                entity, model.Password);

              if (entity.Id > 0)
                changedPassword = true;
            }
          }

          if (result.ModelErrors.Count == 0)
          {
            // Force the DB save cycle, so the user entity gets an ID
            context.SaveChangesAsync().Wait();
            userId = entity.Id;

            if (changedPassword)
              await AddAuditLogAsync((int)AuditType.ChangedPassword, userId: entity.Id, context: context);

            scope.Complete();
          }

          // Completed scope above
        }
      }

      if (result.ModelErrors.Count == 0 && userId.HasValue)
        result.Detail = await GetApplicationUserDetailAsync(userId.Value);

      // Add lookups if needed
      return result;
    }

    private IQueryable<Entities.ApplicationUser> QueryApplicationUsers(ApplicationUserFilter filter)
    {
      var query = this.Context.ApplicationUsers.AsQueryable();

      if (filter != null)
      {
        if (filter.UserId.HasValue)
          query = query.Where(o => o.Id == filter.UserId);

        if (!string.IsNullOrWhiteSpace(filter.Email))
          query = query.Where(o => o.Email.Contains(filter.Email));

        if (!string.IsNullOrWhiteSpace(filter.FullName))
          query = query.Where(o => o.FullName.Contains(filter.FullName));

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
          query = query.Where(o => o.PhoneNumber.Contains(filter.PhoneNumber));

        if (filter.IncludeDisabled.HasValue && filter.IncludeDisabled.Value)
        {
          // Do nothing, include all rows
          query = query.Where(o => o.IsEnabled);
        }
        else
        {
          // Did not specify to include all rows or specified including only Enabled rows
          //query = query.Where(o => o.IsEnabled);
        }


        if (filter.RoleId.HasValue)
          query = query.Where(o => o.ApplicationUserRoles.Any(x => x.RoleId == filter.RoleId.Value));
      }

      return query;
    }

    #endregion


    #region PHA User

    public async Task<PhaUser> GetPhaUserDetailAsync(int userId)
    {
      return await Mapper.ProjectTo<PhaUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            RoleId = (int)ApplicationRoleId.ManagementPortalUser,
            UserId = userId,
          }))
        .FirstOrDefaultAsync();
    }

    public async Task<PhaUser> GetCurrentPhaUserDetailAsync()
    {
      return await GetPhaUserDetailAsync(IdentityService.UserId.Value);
    }

    public async Task<PhaUser> GetCurrentPhaAdminUserDetailAsync()
    {
      return await GetPhaAdminUserDetailAsync(IdentityService.UserId.Value);
    }

    public async Task<PhaUser> GetPhaAdminUserDetailAsync(int userId)
    {
      return await Mapper.ProjectTo<PhaUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            RoleId = (int)ApplicationRoleId.ManagementPortalAdmin,
            UserId = userId,
          }))
        .FirstOrDefaultAsync();
    }


    // List all users( user and admin) for  Employers tab
    public async Task<IEnumerable<PhaUser>> GetPhaUsersAsync(PhaUserFilter filter)
    {
      var result = await Mapper.ProjectTo<PhaUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            Email = filter?.Email,
            FullName = filter?.FullName,
            IncludeDisabled = filter?.IncludeDisabled,
            PhoneNumber = filter?.PhoneNumber,
            UserId = filter?.UserId,
            TwoFactorEnabled = (bool)filter?.TwoFactorEnabled
            //RoleId = (int)ApplicationRoleId.ManagementPortalUser,
          }))
        .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<PhaUser>> GetPhaPortalUsersAsync(PhaUserFilter filter)
    {
      var result = await Mapper.ProjectTo<PhaUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            Email = filter?.Email,
            FullName = filter?.FullName,
            IncludeDisabled = filter?.IncludeDisabled,
            PhoneNumber = filter?.PhoneNumber,
            UserId = filter?.UserId,
            TwoFactorEnabled = (bool)filter?.TwoFactorEnabled,
            RoleId = (int)ApplicationRoleId.ManagementPortalUser,
          }))
        .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<PhaUser>> GetPhaAdminUsersAsync(PhaUserFilter filter)
    {
      var result = await Mapper.ProjectTo<PhaUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            Email = filter?.Email,
            FullName = filter?.FullName,
            IncludeDisabled = filter?.IncludeDisabled,
            PhoneNumber = filter?.PhoneNumber,
            UserId = filter?.UserId,
            TwoFactorEnabled = (bool)filter?.TwoFactorEnabled,
            RoleId = (int)ApplicationRoleId.ManagementPortalAdmin,
          }))
        .ToListAsync();

      return result;
    }

    public async Task<bool> RemoveApplicationUserRoleAsyn(int Id)
    {
      var entity = await Context.ApplicationUsers
           .FirstOrDefaultAsync(o => o.Id == Id);
      IList<String> roles = await _userManager.GetRolesAsync(entity);
      foreach (string role in roles)
      {
        _logger.LogInformation("Role: " + role + ", entity: " + entity + ", Id: " + Id);
        if (role == "Management Portal User")
        {
          var deletionResult = await _userManager.RemoveFromRoleAsync(entity, ConvertApplicationRole(ApplicationRoleId.ManagementPortalUser));
          _logger.LogInformation("deletionResult: " + deletionResult);
          if (deletionResult.Succeeded)
            return true;
        }
        if (role == "Management Portal Admin")
        {
          var deletionResult = await _userManager.RemoveFromRoleAsync(entity, ConvertApplicationRole(ApplicationRoleId.ManagementPortalAdmin));
          _logger.LogInformation("deletionResult: " + deletionResult);
          if (deletionResult.Succeeded)
            return true;
        }

      }
      return false;
    }

    public async Task<PhaUserSaveResult> SavePhaUserDetailAsync(PhaUser model)
    {
      var result = new PhaUserSaveResult()
      {
        Detail = model,
      };

      // TODO: Do any PHA-specific validation here
      // No need for general user validation here, the SaveApplicationUserDetailAsync does that already

      // Creating a separate scope & dbContext to do some work on different tables
      using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
      using (var dbContext = Context.Clone())
      {
        var baseResult = new ApplicationUserSaveResult();
        // Sending the new IAppDbContext into this base method...
        if ((bool)model.createAdmin)
        {
          baseResult = await SaveApplicationUserDetailAsync(model,
          ApplicationRoleId.ManagementPortalAdmin, dbContext);
        }
        else
        {
          baseResult = await SaveApplicationUserDetailAsync(model,
          ApplicationRoleId.ManagementPortalUser, dbContext);
        }

        result.ModelErrors.AddRange(baseResult.ModelErrors);

        if (result.ModelErrors.Count == 0)
        {
          //var phaDetail = await dbContext.PhaDetails
          //	.FirstOrDefaultAsync(o => o.Id == baseResult.Detail.PhaDetailId);
          var appUser = await dbContext.ApplicationUsers
            .Include(o => o.PhaDetail)
            .FirstOrDefaultAsync(o => o.Id == baseResult.Detail.Id);
          var phaDetail = appUser.PhaDetail;

          if (phaDetail == null)
          {
            phaDetail = new Entities.PhaDetail();
            appUser.PhaDetail = phaDetail;
            await dbContext.PhaDetails.AddAsync(phaDetail);
          }

          phaDetail.HeadshotUrl = model.HeadshotUrl;
          phaDetail.Bio = model.Bio;
          //phaDetail.Title = model.Title;
          if (model.Title == null || model.Title == "")
          {
            if (model.Email.ToLower().Contains("pha"))
            {
              phaDetail.Title = "PHA";
            }
            else
            {
              phaDetail.Title = "Converging";
            }
          }
          else
          {
            phaDetail.Title = model.Title;
          }

          if (phaDetail.Id == 0)
            phaDetail.CreatedTimestamp = DateTime.Now;
          phaDetail.LastEditedTimestamp = DateTime.Now;
          phaDetail.UserLastEditedById = IdentityService.UserId;

          await dbContext.SaveChangesAsync();

          // Complete this scope if no errors
          // This will also commit underlying call to SaveApplicationUserDetailAsync
          scope.Complete();
        }

        // The scope.Complete happens above
      }

      if (result.ModelErrors.Count == 0)
        result.Detail = await GetPhaUserDetailAsync(model.Id);

      return result;
    }



    public async Task<PhaUserSaveResult> UpdateTwoFactorEnabledAsync(PhaUser model)
    {

      var result = new PhaUserSaveResult()
      {
        Detail = model,
      };

      using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
      using (var dbContext = Context.Clone())
      {
        // var baseResult = new ApplicationUserSaveResult();
        //  baseResult = await UpdateTwoFactorEnabledStatusAsync(model, dbContext);

        //  result.ModelErrors.AddRange(baseResult.ModelErrors);

        //  var appUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == baseResult.Detail.Id);
        var appUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == model.Id);

        appUser.TwoFactorEnabled = model.TwoFactorEnabled;

        await dbContext.SaveChangesAsync();
        scope.Complete();
      }


      if (result.ModelErrors.Count == 0)
        result.Detail = await GetPhaUserDetailAsync(model.Id);
      if (result.Detail == null)
        result.Detail = await GetPhaAdminUserDetailAsync(model.Id);

      return result;
    }


    public async Task<PhaUserSaveResult> UpdateUserStatusAsync(PhaUser model)
    {

      var result = new PhaUserSaveResult()
      {
        Detail = model,
      };

      using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
      using (var dbContext = Context.Clone())
      {
        // var baseResult = new ApplicationUserSaveResult();
        //  baseResult = await UpdateTwoFactorEnabledStatusAsync(model, dbContext);

        //  result.ModelErrors.AddRange(baseResult.ModelErrors);

        //  var appUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == baseResult.Detail.Id);
        var appUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == model.Id);

      
        appUser.IsEnabled = (bool)model.IsEnabled;
   
        
        await dbContext.SaveChangesAsync();
        scope.Complete();
      }


      if (result.ModelErrors.Count == 0)
        result.Detail = await GetPhaUserDetailAsync(model.Id);
      if (result.Detail == null)
        result.Detail = await GetPhaAdminUserDetailAsync(model.Id);

      return result;
    }

    #endregion


    public async Task<ApplicationUserSaveResult> UpdateTwoFactorEnabledStatusAsync(ApplicationUser model, IAppDbContext context = null)
    {
      context ??= Context;

      var result = new ApplicationUserSaveResult()
      {
        Detail = model,
      };
      int? userId = null;
      if (model.Id > 0 && result.ModelErrors.Count == 0)
      {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
          var entity = await context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == model.Id);

          if (entity != null)
          {
            entity.TwoFactorEnabled = model.TwoFactorEnabled;
          }

          if (result.ModelErrors.Count == 0)
          {
            // Force the DB save cycle, so the user entity gets an ID
            //context.SaveChangesAsync().Wait();
            await context.SaveChangesAsync();

            userId = entity.Id;


            scope.Complete();
          }


        }

      }

      if (result.ModelErrors.Count == 0 && userId.HasValue)
        result.Detail = await GetApplicationUserDetailAsync(userId.Value);

      // Add lookups if needed
      return result;
    }


    #region Member User

    public async Task<IEnumerable<MemberUser>> GetMemberUsersAsync(MemberUserFilter filter)
    {
      var query = QueryApplicationUsers(new ApplicationUserFilter()
      {
        // TODO: Add these back if it turns out we can just look at the user account and not the MemberDetail.SnowflakeMember as below...
        //Email = filter.Email,
        //FullName = filter.FullName,
        IncludeDisabled = filter?.IncludeDisabled,
        //PhoneNumber = filter.PhoneNumber,
        RoleId = (int)Models.Enums.ApplicationRoleId.MyPhaMemberUser,
      });

      if (filter != null)
      {
        if (filter.ChMemberId.HasValue && filter.ChMemberId.Value > 0)
          query = query.Where(o => o.ChMemberId == filter.ChMemberId);

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
          // TODO: Figure out which of these is the right one to do this for
          query = query.Where(o =>
            o.FullName.Contains(filter.FullName)
            || o.SnowflakeMember.FirstName.Contains(filter.FullName)
            || o.SnowflakeMember.MiddleName.Contains(filter.FullName)
            || o.SnowflakeMember.LastName.Contains(filter.FullName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
          // TODO: Figure out which of these is the right one to do this for
          query = query.Where(o =>
            o.Email.Contains(filter.Email)
            || o.SnowflakeMember.EmailAddress.Contains(filter.Email));
        }

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
        {
          // TODO: Figure out which of these is the right one to do this for
          query = query.Where(o =>
            o.PhoneNumber.Contains(filter.PhoneNumber)
            || o.SnowflakeMember.CellPhone.Contains(filter.PhoneNumber)
            || o.SnowflakeMember.HomePhone.Contains(filter.PhoneNumber)
            || o.SnowflakeMember.WorkPhone.Contains(filter.PhoneNumber));
        }

        // Below are specific to Member users only
        if (!string.IsNullOrWhiteSpace(filter.Dob))
        {
          if (DateTime.TryParse(filter.Dob, out DateTime dateValue))
            query = query.Where(o => o.SnowflakeMember.Dob == dateValue);
        }

        if (filter.UserAssignedId.HasValue && filter.UserAssignedId.Value > 0)
          query = query.Where(o => o.SnowflakeMember.MemberDetail.UserAssignedId == filter.UserAssignedId);

        if (filter.ChEmployerId.HasValue && filter.ChEmployerId.Value > 0)
          query = query.Where(o => o.SnowflakeMember.MemberDetail.SnowflakeMember.ChEmployerId == filter.ChEmployerId);

        if (!string.IsNullOrWhiteSpace(filter.CarrierName))
          query = query.Where(o => o.SnowflakeMember.MemberDetail.SnowflakeMember.CarrierName == filter.CarrierName);
      }

      return await Mapper.ProjectTo<MemberUser>(query)
        .ToListAsync();
    }

    public async Task<MemberUser> GetMemberUserDetailAsync(int userId)
    {
      return await Mapper.ProjectTo<MemberUser>(
          QueryApplicationUsers(new ApplicationUserFilter()
          {
            RoleId = (int)ApplicationRoleId.MyPhaMemberUser,
            UserId = userId,
            IncludeDisabled = true,
          }))
        .FirstOrDefaultAsync();
    }

    public async Task<SaveMemberUserResult> SaveMemberUserDetailAsync(MemberUser model)
    {
      var result = new SaveMemberUserResult
      {
        Detail = model,
      };

      using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
      using (var dbContext = Context.Clone())
      {
        // Sending the new IAppDbContext into this base method for transaction scoping...
        var baseResult = await SaveApplicationUserDetailAsync(model,
          ApplicationRoleId.MyPhaMemberUser, dbContext);

        result.ModelErrors.AddRange(baseResult.ModelErrors);

        if (result.ModelErrors.Count == 0)
        {
          var appUser = await dbContext.ApplicationUsers
            .Include(o => o.SnowflakeMember) // .ThenInclude(o => o.MemberDetail)
            .FirstOrDefaultAsync(o => o.Id == baseResult.Detail.Id);

          // *WARNING* Do not save updates to the other properties in the model, which were included for filtering
          // I.e. Dob, UserAssignedId, UserAssignedFullName, etc.
          // There's nothing else we're going to allow changing about a user's profile on this screen.
          // This is primarily for allowing PHA's to unlock and enable/disable users in the mgmt. portal
          // See the registration methods in the Manager or the MemberManager classes for more.

          if (appUser.SnowflakeMember == null)
            // Check for an invalid data scenario, just in case
            result.ModelErrors.Add(o => o.ChMemberId, "Unexpected error: snowflake.CH_MEMBER cannot be null");

          if (result.ModelErrors.Count == 0)
          {
            if (!string.Equals(appUser.SnowflakeMember.EmailAddress, appUser.Email))
            {
              appUser.SnowflakeMember.EmailAddress = appUser.Email;
              await dbContext.SaveChangesAsync();
            }

            scope.Complete();
          }
        }

        // Completed the transaction scope above
      }

      if (result.ModelErrors.Count == 0)
        result.Detail = await GetMemberUserDetailAsync(model.Id);

      return result;
    }

    public async Task<ValidateMemberRegistrationProfileResult> ValidateMemberRegistrationProfile(
      MemberRegistrationProfile model)
    {
      const string genericLookupFailureMsg = "Unable to locate a matching registerable account";

      var result = new ValidateMemberRegistrationProfileResult
      {
        Detail = model,
      };

      // TODO: Error if no matching values
      var emailFilter = new ApplicationUserFilter()
      {
        Email = model.Email,
        IncludeDisabled = true,
        // TODO: No need to bring Role into this for now. Might need to revisit once we have PHA's working in the mobile app.
        //RoleId = (int)Models.Enums.ApplicationRoleId.ManagementPortalUser,
      };

      //model.Email;
      if (await QueryApplicationUsers(emailFilter).AnyAsync())
        result.ModelErrors.Add(o => o.Email, "The email provided is already in use");

      //model.Password
      if (await ValidatePasswordAsync(model.Password) == false)
        result.ModelErrors.Add(o => o.Password, _invalidPasswordErrorMsg);

      if (result.ModelErrors.Count == 0)
      {
        // Get the Snowflake user row, and check those props
        var snowflakeMember = await GetSnowflakeMemberForRegistrationAsync(
          model.Dob.Value, model.FirstName, model.LastName, model.LastFourOfSsn);

        // Whether the member is termed is determined by GetSnowflakeMemberForRegistrationAsync
        if (snowflakeMember == null) // || SnowflakeMemberIsTermed(snowflakeMember))
          result.ModelErrors.Add(o => o.Email, genericLookupFailureMsg);
        else
        {
          if (Context.ApplicationUsers.Any(o => o.ChMemberId == snowflakeMember.ChMemberId))
            // TODO: Add a different failure message?
            // For now, for security reasons, we don't want to indicate anything about whether the registering user matched a valid row
            result.ModelErrors.Add(o => o.Email, genericLookupFailureMsg);
        }

        if (result.ModelErrors.Count == 0)
        {
          if (!await ValidateRegistrationReqs(snowflakeMember))
            result.ModelErrors.Add(o => o.Email, genericLookupFailureMsg);
        }
      }

      return result;
    }

    private static bool SnowflakeMemberIsTermed(Entities.SnowflakeMember snowflakeMember)
    {
      return !string.IsNullOrWhiteSpace(snowflakeMember.CurrentStatus)
        && snowflakeMember.CurrentStatus.ToLower() == "termed";
    }

    private async Task<bool> ValidateRegistrationReqs(Entities.SnowflakeMember snowflakeMember)
    {
      if (snowflakeMember == null) return false;

      var empDetail = await Context.EmployerDetails.SingleOrDefaultAsync(o =>
        o.ChEmployerId == snowflakeMember.ChEmployerId);

      bool isValid = true;

      if (empDetail != null)
      {
        if (empDetail.MyPhaRegistrationMinClinicalRisk.HasValue && snowflakeMember.ClinicalRisk.HasValue
          && snowflakeMember.ClinicalRisk < empDetail.MyPhaRegistrationMinClinicalRisk)
        {
          isValid = false;
        }
      }
      else
      {
        isValid = false;
      }

      return isValid;
    }

    private async Task<Entities.SnowflakeMember> GetSnowflakeMemberForRegistrationAsync(
      DateTime dob,
      string firstName,
      string lastName,
      string lastFourOfSsn,
      AppDbContext context = null)
    {
      context = context ?? Context;

      // TODO: Think about caching here (and elsewhere).
      // Thought this was a good idea, but it's likely a PHA will be on the phone
      // with a member, need to add a phone to proceed with registration,
      // and need this to be refreshed each time.

      //var cleansedMemberPhones = CacheService.GetObjectFromCache(
      //	CacheKeys.ChMemberCleansedPhoneNumbers,
      //	Config.GetCacheExpirationTimeSpan(),
      //	GetChMemberCleansedPhoneNumbers);

      // TODO: Add back phone at some point?
      //var cleansedMemberPhones = await GetChMemberCleansedPhoneNumbersAsync();
      //string phoneNumberCleansed = CleansePhoneNumber(phoneNumber);
      //var matchingPhoneChMemberIds = cleansedMemberPhones.Where(o =>
      //    o.CleansedPhones.Any(x => x.Equals(phoneNumberCleansed)))
      //  .Select(o => o.ChMemberId).ToList();

      var memberLastFourOfSsns = await GetChMemberCleansedLastFourOfSsnsAsync();
      var matchingLastFourOfSsnChMemberIds = memberLastFourOfSsns.Where(o =>
          o.CleansedSsn == lastFourOfSsn)
        .Select(o => o.ChMemberId).ToList();

      var query = context.SnowflakeMembers
        .Where(o => string.IsNullOrWhiteSpace(o.CurrentStatus)
          || o.CurrentStatus.ToLower() != "termed");

      query = query.Where(o =>
          1 == 1
          //&& (matchingPhoneChMemberIds.Contains(o.ChMemberId))
          && matchingLastFourOfSsnChMemberIds.Contains(o.ChMemberId)
          && o.Dob == dob
          && (o.FirstName == firstName || o.FirstNameEdited == firstName)
          && (o.LastName == lastName || o.LastNameEdited == lastName)
      // Zip Code was a requirement, this was relaxed per new req's
      //&& (o.ZipCode == zipCode || o.ZipCodeEdited == zipCode)
      // TODO: Check email here, too?
      // No. Email is not required, so only checking the other requirements
      );

      return await query.FirstOrDefaultAsync();
    }

    internal class ChMemberProfileCleansed
    {
      public int ChMemberId { get; set; }

      public string[] Phones { get; set; }
      public string[] CleansedPhones { get; set; }

      public string Ssn { get; set; }
      public string CleansedSsn { get; set; }
    }

    private string CleansePhoneNumber(string phoneNumber)
    {
      if (string.IsNullOrWhiteSpace(phoneNumber))
        return phoneNumber;

      string pattern = $"[^0-9]";
      var options = RegexOptions.Compiled | RegexOptions.Singleline;
      string phoneNumberCleansed = Regex.Replace(phoneNumber, pattern, string.Empty, options);
      if (phoneNumberCleansed.Length == 11 && phoneNumberCleansed.StartsWith('1'))
        // Remove +1 prefix
        phoneNumberCleansed = phoneNumberCleansed.Substring(1);
      return phoneNumberCleansed;
    }

    private string GetLastFourOfSsn(string ssn)
    {
      if (string.IsNullOrWhiteSpace(ssn))
        return null;

      string pattern = $"[^0-9]";
      var options = RegexOptions.Compiled | RegexOptions.Singleline;
      string ssnCleansed = Regex.Replace(ssn, pattern, string.Empty, options);

      if (!string.IsNullOrWhiteSpace(ssnCleansed) && ssnCleansed.Length >= 4)
        // Is not null or whitespace, is exactly 4 characters
        return ssnCleansed.Substring(ssnCleansed.Length - 4, 4);

      return null;
    }

    private async Task<List<ChMemberProfileCleansed>> GetChMemberCleansedPhoneNumbersAsync()
    {
      var memberPhones = (await Context.SnowflakeMembers
        .Select(o => new
        {
          o.ChMemberId,
          o.CellPhone,
          o.CellPhoneEdited,
          o.HomePhone,
          o.HomePhoneEdited,
          o.WorkPhone,
          o.WorkPhoneEdited
        })
        .ToListAsync())
        .Select(o => new
        {
          o.ChMemberId,
          PhonesAry = new[]
          {
            o.CellPhone,
            o.CellPhoneEdited,
            o.HomePhone,
            o.HomePhoneEdited,
            o.WorkPhone,
            o.WorkPhoneEdited
          }.Where(x => !string.IsNullOrWhiteSpace(x))
        })
        .Where(o => o.PhonesAry.Any())
        .Select(o => new ChMemberProfileCleansed()
        {
          ChMemberId = o.ChMemberId,
          Phones = o.PhonesAry
            .Distinct().ToArray(),
          CleansedPhones = o.PhonesAry.Select(x => CleansePhoneNumber(x))
            .Distinct().ToArray(),
        })
        .ToList();

      return memberPhones;
    }

    private async Task<List<ChMemberProfileCleansed>> GetChMemberCleansedLastFourOfSsnsAsync()
    {
      var memberPhones = (await Context.SnowflakeMembers
          .Select(o => new
          {
            o.ChMemberId,
            o.Ssn,
          })
          .ToListAsync())
        // Exclude any empty before cleansing (for performance)
        .Where(o => !string.IsNullOrWhiteSpace(o.Ssn))
        .Select(o => new ChMemberProfileCleansed()
        {
          ChMemberId = o.ChMemberId,
          Ssn = o.Ssn,
          CleansedSsn = GetLastFourOfSsn(o.Ssn),
        })
        // Exclude any empty, which are invalid (empty or <4 characters)
        .Where(o => !string.IsNullOrWhiteSpace(o.CleansedSsn))
        .ToList();

      return memberPhones;
    }

    public async Task<SaveMemberRegistrationResult> SaveMemberRegistration(MemberRegistration model)
    {
      var result = new SaveMemberRegistrationResult
      {
        Detail = model,
      };

      if (model.Profile == null)
        result.ModelErrors.Add(o => o.Profile, "Personal information must be provided to register");

      // This ensures we have a good snowflakeEntity below when we go looking
      var profileResult = await ValidateMemberRegistrationProfile(model.Profile);
      result.ModelErrors.AddRange(profileResult.ModelErrors);

      if (!model.Consent.ConsentTerms
        || !model.Consent.ConsentPrivacy)
      {
        result.ModelErrors.Add(o => o.Consent, "Consent must be given to all terms and conditions to register");
      }

      if (result.ModelErrors.Count == 0)
      {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        using (var dbContext = Context.Clone())
        {
          var appUserModel = new ApplicationUser()
          {
            Email = model.Profile.Email,
            UserName = model.Profile.Email,
            Password = model.Profile.Password,
            ConfirmPassword = model.Profile.Password,
            FullName = $"{model.Profile.FirstName} {model.Profile.LastName}",
            IsEnabled = true,
            PhoneNumber = model.Profile.CellPhone,
            MemberDetailId = null, // Set/update this from the above
            PhaDetailId = null,
          };

          var baseResult = await SaveApplicationUserDetailAsync(appUserModel,
            ApplicationRoleId.MyPhaMemberUser, dbContext);

          result.ModelErrors.AddRange(baseResult.ModelErrors);

          if (result.ModelErrors.Count == 0)
          {
            // Get the Snowflake Member
            var snowflakeEntity = await GetSnowflakeMemberForRegistrationAsync(
              model.Profile.Dob.Value,
              model.Profile.FirstName,
              model.Profile.LastName,
              model.Profile.LastFourOfSsn,
              dbContext);

            // Update PII
            snowflakeEntity.EmailAddress = model.Profile.Email;
           // snowflakeEntity.FirstNameEdited = model.Profile.FirstName;
            snowflakeEntity.FirstName = model.Profile.FirstName;
            snowflakeEntity.LastName = model.Profile.LastName;
            snowflakeEntity.CellPhone = model.Profile.CellPhone;

            // Update address
            snowflakeEntity.Address1 = model.Address.Address1;
            snowflakeEntity.Address2 = model.Address.Address2;
            snowflakeEntity.ZipCode = model.Address.ZipCode;
            snowflakeEntity.City = model.Address.City;
            snowflakeEntity.State = model.Address.State;

            var memberDetail = await dbContext.MemberDetails
              .FirstOrDefaultAsync(o => o.ChMemberId == snowflakeEntity.ChMemberId);
            // Get the MemberDetail (or create one if none)
            if (memberDetail == null)
            {
              memberDetail = new Entities.MemberDetail()
              {
                ChMemberId = snowflakeEntity.ChMemberId,
              };
              await dbContext.MemberDetails.AddAsync(memberDetail);
            }

            int? phaUserId = await dbContext.SnowflakeMasterEmployers
              .Where(o => o.ChEmployerId == snowflakeEntity.ChEmployerId)
              .Select(o => o.EmployerDetail.DefaultUserAssignedId)
              .SingleOrDefaultAsync();

            memberDetail.UserAssignedId ??= phaUserId;
            memberDetail.MyPhaTermsConsentTimestamp = DateTimeOffset.Now;
            memberDetail.MyPhaPrivacyConsentTimestamp = DateTimeOffset.Now;

            var appUserEntity = await dbContext.ApplicationUsers
              .FirstOrDefaultAsync(o => o.Id == baseResult.Detail.Id);

            appUserEntity.SnowflakeMember = snowflakeEntity;
            await dbContext.SaveChangesAsync();

            // Commit the entire transaction _only_ if there were no errors
            scope.Complete();
          }

          // Scope completed/committed only if the above all goes to plan
        }
      }

      return result;
    }

    #endregion


    private async Task<bool> ValidatePasswordAsync(Entities.ApplicationUser user, string password)
    {
      bool validPassword = true;
      foreach (var validator in _userManager.PasswordValidators)
      {
        var validateResult = await validator.ValidateAsync(_userManager, user, password);
        if (!validateResult.Succeeded)
        {
          validPassword = false;
          break;
        }
      }
      return validPassword;
    }

    private async Task<bool> ValidatePasswordAsync(string password)
    {
      return await ValidatePasswordAsync(new Entities.ApplicationUser(), password);
    }

  }
}
