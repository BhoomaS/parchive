using System;
using System.ComponentModel.DataAnnotations;
using CH.Models.Common;

namespace CH.Models.Auth
{
  #region Login

  // TODO: Implement DataAnnotations for required, etc.
  [TypescriptInclude]
  public class Login
  {
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
  }

  // TODO: Make this inherit Common.SaveResult<Login> and make the client handle the ModelErrors
  [TypescriptInclude]
  public class LoginResult : SaveResult<Login>
  {
    public bool Succeeded { get; set; }
    public string Token { get; set; }
    public string Message { get; set; }
    public DateTime? ValidTo { get; set; }
    public string SetupCode { get; set; }
    public string UserUniqueKey { get; set; }
    public string BarcodeImageUrl { get; set; }
    public int Id { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public Login Detail { get; set; }
  }

  #endregion

  #region MFA
  [TypescriptInclude]
  public class MFA
  {
    public string CodeDigit { get; set; }
    public string UserUniqueKey { get; set; }

  }

  [TypescriptInclude]
  public class MFAResult : SaveResult<MFA>
  {
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public bool IsValidTwoFactorAuthentication { get; set; }
  }
  #endregion


  #region TwoFactorEnabled
  [TypescriptInclude]
  public class TwoFactorEnabled
  {
    public bool IsTwoFactorEnabled { get; set; }
    public int Id { get; set; }
  }
  [TypescriptInclude]
  public class TwoFactorEnabledResult : SaveResult<TwoFactorEnabled>
  {
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
  }
  #endregion



  #region Password Management

  [TypescriptInclude]
  public class ForgotPassword
  {
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; }
  }

  [TypescriptInclude]
  public class ForgotPasswordResult : Common.SaveResult<ForgotPassword>
  {
    public ForgotPassword Detail { get; set; }
  }

  [TypescriptInclude]
  public class ResetPassword
  {
    [Required(ErrorMessage = "Password reset token is required")]
    public string Token { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public string ConfirmPassword { get; set; }
  }

  [TypescriptInclude]
  public class ResetPasswordResult : Common.SaveResult<ResetPassword>
  {
    public ResetPassword Detail { get; set; }
  }

  [TypescriptInclude]
  public class ChangePassword
  {
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm new password is required")]
    public string ConfirmNewPassword { get; set; }
  }

  [TypescriptInclude]
  public class ChangePasswordResult : SaveResult<ChangePassword>
  {
    public ChangePassword Detail { get; set; }
  }

  #endregion


  #region Application User

  // Common AspNetUser / ApplicationUser properties
  [TypescriptInclude]
  public class ApplicationUser
  {
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int? MemberDetailId { get; set; } // MemberDetailId
    public int? PhaDetailId { get; set; } // PhaDetailId
    public bool? IsEnabled { get; set; } // IsEnabled
    public bool? IsLocked { get; set; } // 
    public bool? createAdmin { get; set; }
  }

  [TypescriptInclude]
  public class ApplicationUserSaveResult : SaveResult<ApplicationUser>
  {
    public ApplicationUser Detail { get; set; }
  }

  [TypescriptInclude]
  public class ApplicationUserFilter
  {
    public int? UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int? RoleId { get; set; }

    public bool? IncludeDisabled { get; set; }
  }

  #endregion


  #region Pha

  [ManagementPortalTypescriptInclude]
  public class PhaUser : ApplicationUser
  {
    public string Title { get; set; }
    public string Bio { get; set; }
    public string HeadshotUrl { get; set; }
  }

  [ManagementPortalTypescriptInclude]
  public class PhaUserSaveResult : SaveResult<PhaUser>
  {
    public PhaUser Detail { get; set; }
  }

  [ManagementPortalTypescriptInclude]
  public class PhaUserFilter : ApplicationUserFilter
  { }

  #endregion


  #region Member

  [TypescriptInclude]
  public class MemberUser : ApplicationUser
  {
    public DateTime? Dob { get; set; }
    public int? UserAssignedId { get; set; }
    public string UserAssignedFullName { get; set; }
    public bool? Dnc { get; set; }
    public bool? Vip { get; set; }
    public bool? Minor { get; set; }
    public int? ChMemberId { get; set; }
  }

  [TypescriptInclude]
  public class SaveMemberUserResult : SaveResult<MemberUser>
  {
    public MemberUser Detail { get; set; }
  }

  [TypescriptInclude]
  public class MemberUserFilter : ApplicationUserFilter
  {
    public int? ChMemberId { get; set; }
    public string Dob { get; set; }
    public int? UserAssignedId { get; set; }
    public int? ChEmployerId { get; set; }
    public string CarrierName { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class MemberRegistrationProfile
  {
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime? Dob { get; set; }

    [Required(ErrorMessage = "Last 4 of SSN is required")]
    public string LastFourOfSsn { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [DataType(DataType.PhoneNumber)]
    public string CellPhone { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class ValidateMemberRegistrationProfileResult : SaveResult<MemberRegistrationProfile>
  {
    public MemberRegistrationProfile Detail { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class MemberRegistrationAddress
  {
    // Street address or P.O. Box
    [Required(ErrorMessage = "Street address or P.O. Box is required")]
    public string Address1 { get; set; }

    // Apartment, unit, etc.
    public string Address2 { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; }

    [Required(ErrorMessage = "State is required")]
    public string State { get; set; }

    [Required(ErrorMessage = "Zip code is required")]
    [DataType(DataType.PostalCode)]
    public string ZipCode { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class MemberRegistrationConsent
  {
    public bool ConsentTerms { get; set; }
    public bool ConsentPrivacy { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class MemberRegistration
  {
    public MemberRegistrationProfile Profile { get; set; }
    public MemberRegistrationAddress Address { get; set; }
    public MemberRegistrationConsent Consent { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class SaveMemberRegistrationResult : SaveResult<MemberRegistration>
  {
    public MemberRegistration Detail { get; set; }
  }

  #endregion

}
