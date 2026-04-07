using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using CH.Models.Common;
using CH.Models.Enums;
using CH.Models.ManagementPortal.Member.Outreach;
using Microsoft.Extensions.Logging;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace CH.Business.ManagementPortal
{
  public partial interface IMemberManager
  {
    Task<IEnumerable<Entities.CareFiles>> GetCareFilesDetails(string chMemberId);
    Task<IEnumerable<Entities.CareFiles>> DownloadCareDetails();
    Task<IEnumerable<Entities.CareFiles>> DownloadSearches();
    Task<IEnumerable<Entities.SearchesModel>> DownloadCareSearches();

    Task<IEnumerable<Entities.Care_AuditLog>> GetCareAudithistory();

    Task<IEnumerable<Entities.CareCommunicationNote>> GetCareCommunicationNotesAsync(int chMemberId);

    Task<ProgramSummary> GetProgramSummaryAsync(int chMemberId);
    Task<bool> UpdateProgramSummaryAsync(ProgramSummary summary);

    Task<Demographic> GetDemographicAsync(int chMemberId);
    Task<bool> UpdateDemographicAsync(Demographic model);
    Task<bool> UpdateCareFilesAsync(string fn, string ln, string at);
    Task<IEnumerable<Entities.CommunicationNote>> GetCommunicationNotesAsync(int chMemberId);
    Task<Entities.CommunicationNote> SaveCommunicationNoteAsync(int chMemberId, CommunicationNote note);
    Task<bool> DeleteNoteAsync(int chMemberId, int noteId);
    Task<bool> DeleteCareNoteAsync(int chMemberId, int noteId);

    Task<IEnumerable<Entities.ReferralMgmtNote>> GetReferralMgmtNotesAsync(int chMemberId);
    Task<Entities.ReferralMgmtNote> SaveReferralMgmtNoteAsync(int chMemberId, ReferralManagementNote note);
    Task<bool> DeleteReferralMgmtNoteAsync(int chMemberId, int MgmtNoteId);

    Task<IEnumerable<Entities.OutreachReporting>> GetOutreachReportsAsync(int chMemberId);
    Task<Entities.OutreachReporting> SaveOutreachEmailBlastAsync(EmailBlast mail);
    Task<Entities.OutreachReporting> SaveOutreachReportAsync(int chMemberId, string name, OutreachReport outreachReport);
    Task<bool> DeleteOutreachReportAsync(int chMemberId, int outreachReportId);

    Task<IEnumerable<KeyValue>> GetMemberVendorsAsync(int chMemberId);
    List<Entities.AppointmentsListReturnModel> GetAppointmentsList();
    Task<IEnumerable<Entities.CareFileTasksModel>> GetCareFileTaskDetails();
    Task<IEnumerable<Entities.CareFiles>> GetAutoAssignDetails();
    Task<IEnumerable<Entities.CareFiles>> GetCareFileDistributionDetails();
    Task<IEnumerable<Entities.SearchesModel>> GetCareSearches();
    Task<IEnumerable<Entities.HigRiskMembersTasksModel>> GetHighRiskMembersDetails();
    Task<IEnumerable<Entities.CareFiles>> TaskDistributionGeneric(string[] clients, string[] state, string[] pha, string date);
    Task<IEnumerable<Entities.CareFiles>> TaskDistributionEssilor(string[] client, string[] state, string[] pha, string cdate);
    Task<IEnumerable<Entities.BillingReportModel>> GetBillingReportDetails(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.WeeklyReportModel>> GetWeeklyReportDetails(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.ChalkMountainPHSreportModel>> GetChalkMountainPHSreports(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.ChalkMountainProviderReportModel>> GetChalkMountainProviderReports(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.SevenElevenMonthlyReportModel>> GetSevenElevenMonthlyReports(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.SevenElevenWeeklyReportModel>> GetSevenElevenWeeklyReports(DateTime fromdate, DateTime todate);
    Task<IEnumerable<Entities.SevenElevenWellRightReportModel>> GetSevenElevenWellRightReports(DateTime fromdate, DateTime todate);
    Task<Entities.CareCommunicationNote> SaveCareCommunicationNoteAsync(int chMemberId, CareCommunicationNote note);

    Task<bool> DeleteMaintenanceProgramAsync(int chMemberId, int programId);
    //Task<EmailServiceResult> SendEmailAsync(EmailService service);
    Task<IEnumerable<Entities.MemberNotesModel>> GetNotesDetails(int id);
    Task<IEnumerable<Entities.CommunicationOutreachNotesModel>> GetOutreachWithCommunicationNotesDetails(int id);
    Task<IEnumerable<Entities.CareCommunicationOutreachNotesModel>> GetOutreachWithCareCommunicationNotesDetails(int id);
  }

  public partial class MemberManager
  {
    #region Program

    public async Task<ProgramSummary> GetProgramSummaryAsync(int chMemberId)
    {
      if (chMemberId == 0)
      {
        throw new ArgumentNullException(paramName: nameof(chMemberId));
      }

      var summary = await Context.SnowflakeMembers
        .Where(o => o.ChMemberId == chMemberId)
        .Select(o => new ProgramSummary()
        {
          Id = o.ChMemberId,
          Carrier = o.CarrierName,
          EffectiveEligibleDate = o.EffectiveDate,
          CoverageType = o.CoverageType,
          Status = o.CurrentStatus,
          ProgramTypeId = o.MemberDetail.CodeProgramTypeId,
          StartDate = o.MemberDetail.StartDate,
          BenefitPlan = o.PlanName,
          AssignedToId = o.MemberDetail.UserAssignedId,
          ConsentSentMem = o.MemberDetail.ConsentSentMem,
          //ConsentSentMemDate = o.MemberDetail.ConsentSentMemDate,
          ConsentDate = o.MemberDetail.ConsentDate,
          ConsentReceivedTypeId = o.MemberDetail.CodeConsentReceivedTypeId,
          AuthorizedRepEmailAddress = o.MemberDetail.AuthorizedRepEmailAddress,
          AuthorizedRepPhoneNumber = o.MemberDetail.AuthorizedRepPhoneNumber,
          AuthorizedRep = o.MemberDetail.AuthorizedRep,
          AuthorizedRepName = o.MemberDetail.AuthorizedRepName,
          PhsSentMember = o.MemberDetail.PhsSentMem,
          PhsSentMemberDate = o.MemberDetail.PhsSentMemDate,
          PhsReviewedMem = o.MemberDetail.PhsReviewedMem,
          PhsReviewedMemDate = o.MemberDetail.PhsReviewedMemDate,
          PhsReportPcp = o.MemberDetail.PhsReportPcp,
          PhsReportToPcpDate = o.MemberDetail.PhsReportPcpDate,
          PhsReportToSpecialist = o.MemberDetail.PhsReportSpecialist,
          PhsReportSpecialistDate = o.MemberDetail.PhsReportSpecialistDate,
        })
        .FirstOrDefaultAsync();
      return summary;
    }
    public async Task<IEnumerable<Entities.CareCommunicationNote>> GetCareCommunicationNotesAsync(int chMemberId)
    {
      // Get Note by memberId
      var communicationNoteMethods = await Context.CareCommunicationNotes
          .Where(x => x.ChMemberId == chMemberId)
          .OrderByDescending(x => x.CreatedTimestamp).ToListAsync();
      return communicationNoteMethods;
    }
    public async Task<Entities.CareCommunicationNote> SaveCareCommunicationNoteAsync(int chMemberId,
           CareCommunicationNote model)
    {
      Entities.CareCommunicationNote note = null;
      var phauser = await Context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == IdentityService.UserId);
      var fullName = phauser.FullName.TrimEnd();
      var names = fullName.Split(' ');
      string userInitial = " User(";
      string fN_initial = fullName.Substring(0, 1) != "J" ? String.Concat(userInitial, fullName.Substring(0, 1)) : String.Concat(userInitial, fullName.Substring(0, 2));
      string lN_initial = fN_initial == " User(Ja" ? names[1].Substring(0, 2)
      : fN_initial == " User(Jo" && fullName != "Jorge Gonzalez" ? names[1].Substring(0, 3)
      : fN_initial == " User(M" && fullName == "Melissa Orosco" ? names[1].Substring(0, 3) : names[1].Substring(0, 1);

      userInitial = String.Concat(fN_initial, String.Concat(lN_initial, ") "));


      if (model.Id.HasValue)
      {
        note = await Context.CareCommunicationNotes
            .FirstOrDefaultAsync(x => x.ChMemberId == chMemberId && x.Id == model.Id);
      }

      if (note == null)
      {
        note = new Entities.CareCommunicationNote()
        {
          ChMemberId = chMemberId,
          RecordDate = DateTime.Now,
          CreatedTimestamp = DateTime.Now,
          IgnoreFlag = false,
        };
        await Context.CareCommunicationNotes.AddAsync(note);
      }

      note.Method = model.Method;
      note.NoteDate = model.NoteDate;
      note.VisitType = model.VisitType;
      note.NoteText = model.NoteText;
      //note.NoteText = String.Concat(model.NoteText, userInitial);
      note.UserInitial = userInitial;
      note.VisitStatus = model.VisitStatus;
      note.UserLastEditedById = IdentityService.UserId;
      note.LastEditedTimestamp = DateTime.Now;
      note.Eid = model.Eid;
      note.GroupId = model.GroupId;
      note.GroupName = model.GroupName;
      note.MemberId = model.MemberId;
      note.Purpose = model.Purpose;
      note.FileType = model.FileType;
      note.SphNotes = model.SphNotes;
      note.Date = model.Date;
      note.uploadedDocs = note.uploadedDocs + model.uploadedDocs;
      if (model.outreachId != null)
      {
        note.outreachId = Convert.ToInt32(model.outreachId);
      }

      // Context.CareCommunicationNotes.AddAsync(note);
      await Context.SaveChangesAsync();
      return note;
    }

    public async Task<bool> UpdateProgramSummaryAsync(ProgramSummary dto)
    {
      var chMember = await Context.SnowflakeMembers
      .FirstOrDefaultAsync(o => o.ChMemberId == dto.Id);
      if (chMember == null)
        throw new ArgumentException("Invalid member");

      var memberDetail = await Context.MemberDetails
      .FirstOrDefaultAsync(o => o.ChMemberId == dto.Id);
      if (memberDetail == null)
      {
        memberDetail = new Entities.MemberDetail()
        {
          ChMemberId = dto.Id,
        };
        await Context.MemberDetails.AddAsync(memberDetail);
      }

      //chMember.CarrierName = dto.Carrier; // READ-ONLY
      //chMember.PlanName = dto.BenefitPlan; // READ-ONLY
      //chMember.EffectiveDate = dto.EffectiveEligibleDate; // READ-ONLY
      //chMember.CoverageType = dto.CoverageType; // READ-ONLY
      //chMember.CurrentStatus = dto.Status; // READ-ONLY
      chMember.LastEditedTimestamp = DateTime.Now;
      chMember.UserLastEditedById = IdentityService.UserId;
      chMember.IsModified = true;

      memberDetail.CodeProgramTypeId = dto.ProgramTypeId;
      memberDetail.StartDate = dto.StartDate;
      memberDetail.UserAssignedId = dto.AssignedToId;
      memberDetail.ConsentSentMem = dto.ConsentSentMem;
     // memberDetail.ConsentSentMemDate = dto.ConsentSentMemDate;
      memberDetail.ConsentDate = dto.ConsentDate;
      memberDetail.CodeConsentReceivedTypeId = dto.ConsentReceivedTypeId;
      memberDetail.AuthorizedRepEmailAddress = dto.AuthorizedRepEmailAddress;
      memberDetail.AuthorizedRepPhoneNumber = dto.AuthorizedRepPhoneNumber;
      memberDetail.AuthorizedRep = dto.AuthorizedRep;
      memberDetail.AuthorizedRepName = dto.AuthorizedRepName;
      memberDetail.PhsSentMem = dto.PhsSentMember;
      memberDetail.PhsSentMemDate = dto.PhsSentMemberDate;
      memberDetail.PhsReviewedMem = dto.PhsReviewedMem;
      memberDetail.PhsReviewedMemDate = dto.PhsReviewedMemDate;
      memberDetail.PhsReportPcp = dto.PhsReportPcp;
      memberDetail.PhsReportPcpDate = dto.PhsReportToPcpDate;
      memberDetail.PhsReportSpecialist = dto.PhsReportToSpecialist;
      memberDetail.PhsReportSpecialistDate = dto.PhsReportSpecialistDate;
      memberDetail.LastEditedTimestamp = DateTime.Now;
      memberDetail.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return true;
    }

    #endregion


    #region Demographic

    public async Task<Demographic> GetDemographicAsync(int chMemberId)
    {
      var result = await Mapper.ProjectTo<Demographic>(Context.SnowflakeMembers
        .Where(o => o.ChMemberId == chMemberId))
        .FirstOrDefaultAsync();
      DateTime? currentdate = DateTime.Now;
      DateTime? DateofBirth = Convert.ToDateTime(result.Dob);
      int ageInYears = currentdate.Value.Year - DateofBirth.Value.Year;
      _logger.LogInformation("current year: " + currentdate.Value.Year + "\n DOB year: " + DateofBirth.Value.Year);
      

      try
      {
        int count = Context.CareFiles.Count();
        //var getphonebyCarefile = await Context.CareFiles.FirstOrDefaultAsync(x => x.ClaimantFirstName.ToLower() == result.FirstName.ToLower() && x.ClaimantLastName.ToLower() == result.LastName.ToLower());
        var getphonebyCarefile = await Context.CareFiles
           .FirstOrDefaultAsync(x => x.ClaimantFirstName.ToLower().Equals(result.FirstName.ToLower()) 
           && x.ClaimantLastName.ToLower().Equals(result.LastName.ToLower())
           && x.DateOfBirth == result.Dob
           );

        if (getphonebyCarefile != null)
        {
          if (getphonebyCarefile.Phone != "-")
          {
            if (getphonebyCarefile.Phone != result.HomePhone || getphonebyCarefile.Phone != result.WorkPhone)
            {
              result.UpdatedPhone = getphonebyCarefile.Phone;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }


      if (result != null)
      {
        var al = new Entities.AuditLog()
        {
          CodeAuditTypeId = (int)AuditType.MemberView,
          AdditionalInfo = result.Id.ToString(),
          CreatedBy = IdentityService.UserId,
          DateCreated = DateTime.Now,
        };

        bool skipAudit = await Context.AuditLogs
            .AnyAsync(o => o.CodeAuditTypeId == al.CodeAuditTypeId &&
                o.AdditionalInfo == al.AdditionalInfo && o.CreatedBy == al.CreatedBy &&
                o.DateCreated >= DateTime.Now.AddMinutes(-15));
        if (!skipAudit)
        {
          await Context.AuditLogs.AddAsync(al);
          await Context.SaveChangesAsync();
        }
      }

      return result;
    }

    private bool CalculateAge(DateTime? CurrentDate, DateTime? dateOfBirth, out int ageInYears)
    {
      if (!CurrentDate.HasValue || !dateOfBirth.HasValue)
      {
        ageInYears = default;
        return false;
      }

      ageInYears = CurrentDate.Value.Year - dateOfBirth.Value.Year;

      if (dateOfBirth > CurrentDate.Value.AddYears(-ageInYears))
      {
        ageInYears--;
      }
      return true;
    }

    public async Task<bool> UpdateDemographicAsync(Demographic model)
    {
      var chMember = await Context.SnowflakeMembers
          .Include(o => o.ApplicationUsers)
          .FirstOrDefaultAsync(o => o.ChMemberId == model.Id);
      if (chMember == null)
        throw new ArgumentException("Invalid member");

      var memberDetail = await Context.MemberDetails
          .FirstOrDefaultAsync(o => o.ChMemberId == model.Id);
      if (memberDetail == null)
      {
        memberDetail = new Entities.MemberDetail()
        {
          ChMemberId = model.Id,
        };
        await Context.MemberDetails.AddAsync(memberDetail);
      }

      //snowflakeMember.FirstName = model.FirstName; // READ-ONLY
      //snowflakeMember.MiddleName = model.MiddleName; // READ-ONLY
      //snowflakeMember.LastName = model.LastName; // READ-ONLY
      //snowflakeMember.Dob = model.Dob; // READ-ONLY

      chMember.Gender = model.Gender;
      chMember.Address1 = model.Address1;
      chMember.Address2 = model.Address2;
      chMember.City = model.City;
      //snowflakeMember.County = ? // NOT DISPLAYED
      chMember.State = model.State;
      chMember.ZipCode = model.ZipCode;

      // Ignore case for EmailAddress
      // We're not going to allow PHA's to change Member's login email.
      // Whatever is entered here will be overwritten when the user updates their profile

      // Only allow mgmt. portal user to update Member email
      // if they don't have an account
      if (!chMember.ApplicationUsers.Any())
        chMember.EmailAddress = model.EmailAddress;

      chMember.CellPhone = model.CellPhone;
      chMember.WorkPhone = model.WorkPhone;
      chMember.HomePhone = model.HomePhone;
      chMember.Updated_Phone = model.UpdatedPhone;
      chMember.SelfReported_Phone = model.SelfReportedPhone;
      chMember.EthnicGroup = model.Ethnicity;
      chMember.MaraRisk = model.MaraRisk; 

      //snowflakeMember.MaraRisk = model.MaraRisk; // READ-ONLY
      //snowflakeMember.ClinicalRisk = model.ClinicalRisk; // READ-ONLY
      //snowflakeMember.Relationship = model.RelationClass; // READ-ONLY

      chMember.LastEditedTimestamp = DateTime.Now;
      chMember.UserLastEditedById = IdentityService.UserId;
      chMember.IsModified = true;

      memberDetail.CodePreferredLanguageId = model.PrefLanguageId;
      memberDetail.CodePreferredContactTimeId = model.PrefContactTimeId;
      memberDetail.CodePreferredContactMethodId = model.PrefContactMethodId;
      memberDetail.SecondaryContact = model.SecondaryContact;
      memberDetail.CodeLearningStyleId = model.LearningStyleId;
      memberDetail.LastEditedTimestamp = DateTime.Now;
      memberDetail.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> UpdateCareFilesAsync(string fn, string ln, string at)
    {

      var caremember = await Context.CareFiles.FirstOrDefaultAsync
     (x => x.ClaimantFirstName == fn.ToString() && x.ClaimantLastName == ln.ToString() && x.File != "Searches");
      if (caremember == null)
      {
        throw new ArgumentException("Invalid member");
      }

      caremember.AssignedTo = at;
      Context.SaveChangesNoAudit();
      return true;

    }
    #endregion


    #region region Communication

    public async Task<IEnumerable<Entities.CommunicationNote>> GetCommunicationNotesAsync(int chMemberId)
    {
      // Get Note by memberId
      var communicationNoteMethods = await Context.CommunicationNotes
        .Where(x => x.ChMemberId == chMemberId)
        .OrderByDescending(x => x.CreatedTimestamp).ToListAsync();
      return communicationNoteMethods;
    }


    public async Task<bool> DeleteNoteAsync(int chMemberId, int noteId)
    {
      if (noteId <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(noteId));
      }
      // Get Note by noteId
      var note = await Context.CommunicationNotes.FirstOrDefaultAsync(x => x.Id == noteId && x.ChMemberId == chMemberId);

      // Validate if Note exists
      if (note == null)
      {
        return false;
      }

      // Remove the Note from repository
      Context.Remove(note);
      //if (note.CreatedTimestamp >= DateTime.Now.Subtract(
      //  Config.GetDeleteMaximumAge()))
      //{
      //  Context.Remove(note);
      //}
      //else
      //{
      //  return false;
      //}

      // Delete Note in database
      await Context.SaveChangesAsync();
      return true;

    }

    public async Task<bool> DeleteCareNoteAsync(int chMemberId, int noteId)
    {
      if (noteId <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(noteId));
      }
      // Get Note by noteId
      var note = await Context.CareCommunicationNotes.FirstOrDefaultAsync(x => x.Id == noteId && x.ChMemberId == chMemberId);

      // Validate if Note exists
      if (note == null)
      {
        return false;
      }

      // Remove the Note from repository
      Context.Remove(note);
      //if (note.CreatedTimestamp >= DateTime.Now.Subtract(
      //  Config.GetDeleteMaximumAge()))
      //{
      //  Context.Remove(note);
      //}
      //else
      //{
      //  return false;
      //}

      // Delete Note in database
      await Context.SaveChangesAsync();
      return true;

    }
    public async Task<Entities.CommunicationNote> SaveCommunicationNoteAsync(int chMemberId,
      CommunicationNote model)
    {
      Entities.CommunicationNote note = null;

      var phauser = await Context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == IdentityService.UserId);
      var fullName = phauser.FullName.TrimEnd();
      var names = fullName.Split(' ');
      string userInitial = " User(";
      string fN_initial = fullName.Substring(0, 1) != "J" ? String.Concat(userInitial, fullName.Substring(0, 1)) : String.Concat(userInitial, fullName.Substring(0, 2));
      string lN_initial = fN_initial == " User(Ja" ? names[1].Substring(0, 2)
        : fN_initial == " User(Jo" && fullName != "Jorge Gonzalez" ? names[1].Substring(0, 3)
        : fN_initial == " User(M" && fullName == "Melissa Orosco" ? names[1].Substring(0, 3) : names[1].Substring(0, 1);
      userInitial = String.Concat(fN_initial, String.Concat(lN_initial, ") "));


      if (model.Id.HasValue)
      {
        note = await Context.CommunicationNotes
          .FirstOrDefaultAsync(x => x.ChMemberId == chMemberId && x.Id == model.Id);
      }

      if (note == null)
      {
        note = new Entities.CommunicationNote()
        {
          ChMemberId = chMemberId,
          RecordDate = DateTime.Now,
          CreatedTimestamp = DateTime.Now,
          IgnoreFlag = false,
        };
        await Context.CommunicationNotes.AddAsync(note);
      }

      note.Method = model.Method;
      note.NoteDate = model.NoteDate;
      note.VisitType = model.VisitType;
      note.NoteText = model.NoteText;
      note.UserInitial = userInitial;
      //note.NoteText = String.Concat(model.NoteText, userInitial);
      note.VisitStatus = model.VisitStatus;
      note.UserLastEditedById = IdentityService.UserId;
      note.LastEditedTimestamp = DateTime.Now;
      note.uploadedDocs = note.uploadedDocs + model.uploadedDocs;
      if (model.outreachId != null)
      {
        note.outreachId = Convert.ToInt32(model.outreachId);
      }
      

      await Context.SaveChangesAsync();

      return note;
    }

    #endregion


    #region Referral Management

    public async Task<bool> DeleteReferralMgmtNoteAsync(int chMemberId, int mgmtNoteId)
    {
      if (mgmtNoteId <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(mgmtNoteId));
      }
      // Get Referral by mgmtNoteId
      var referralMgmt = await Context.ReferralMgmtNotes.FirstOrDefaultAsync(x => x.Id == mgmtNoteId && x.ChMemberId == chMemberId);

      // Validate if Referral exists
      if (referralMgmt == null)
      {
        return false;
      }

      // Remove the Referral from repository
      Context.Remove(referralMgmt);
      //if (referralMgmt.CreatedTimestamp >= DateTime.Now.Subtract(
      //  Config.GetDeleteMaximumAge()))
      //{
      //  Context.Remove(referralMgmt);
      //}
      //else
      //{
      //  return false;
      //}
      // Delete Note in database
      await Context.SaveChangesAsync();
      return true;
    }

    public async Task<IEnumerable<Entities.ReferralMgmtNote>> GetReferralMgmtNotesAsync(int chMemberId)
    {
      // Get Referral Mgmt Notes
      var referralMgmtNotes = await Context.ReferralMgmtNotes
        .Where(x => x.ChMemberId == chMemberId).ToListAsync();
      return referralMgmtNotes;
    }

    public async Task<Entities.ReferralMgmtNote> SaveReferralMgmtNoteAsync(int chMemberId,
      ReferralManagementNote model)
    {
      Entities.ReferralMgmtNote note = null;
      var vendors = await Context.Vendors
         .Where(o => o.IsEnabled)
         .Where(o => o.VendorName == model.ReferredTo)
         .Select(o => new KeyValue()
         {
           Id = o.Id,
           Value = o.VendorName,
         })
         .OrderBy(o => o.Value)
         .ToListAsync();
      if (model.Id.HasValue)
      {
        note = await Context.ReferralMgmtNotes
          .FirstOrDefaultAsync(x => x.ChMemberId == chMemberId && x.Id == model.Id.Value);
      }

      if (note == null)
      {

        note = new Entities.ReferralMgmtNote()
        {
          ChMemberId = chMemberId,
          RecordDate = DateTime.Now,
          CreatedTimestamp = DateTime.Now,
        };
        await Context.ReferralMgmtNotes.AddAsync(note);
      }

      note.ReferralDate = model.ReferralDate;
      note.ReferredTo = vendors[0].Value;
      note.UserLastEditedById = IdentityService.UserId;
      // note.LastEditedTimestamp = DateTime.Now;

      await Context.SaveChangesAsync();

      return note;
    }

    #endregion


    #region Outreach Reporting

    public async Task<IEnumerable<Entities.OutreachReporting>> GetOutreachReportsAsync(int chMemberId)
    {
      // Get Outreach Report by memberId
      var reports = await Context.OutreachReportings
        .Where(x => x.ChMemberId == chMemberId)
        .OrderByDescending(x => x.OutreachDate).ToListAsync();
      return reports;
    }

    public async Task<bool> DeleteOutreachReportAsync(int chMemberId, int outreachReportId)
    {
      if (outreachReportId <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(outreachReportId));
      }
      // Get Report by reportId
      var outreachReport = await Context.OutreachReportings
        .FirstOrDefaultAsync(x => x.Id == outreachReportId && x.ChMemberId == chMemberId);

      // Validate if Report exists
      if (outreachReport == null)
      {
        throw new ArgumentNullException(paramName: nameof(outreachReport));
      }

      // Remove the Report from the repository
      Context.Remove(outreachReport);
      //if (outreachReport.CreatedTimestamp >= DateTime.Now.Subtract(
      //  Config.GetDeleteMaximumAge()))
      //{
      //  Context.Remove(outreachReport);
      //}
      //else
      //{
      //  return false;
      //}
      // Delete Report in database
      await Context.SaveChangesAsync();
      return true;
    }

    public async Task<Entities.OutreachReporting> SaveOutreachEmailBlastAsync(EmailBlast mail)
    {

      _logger.LogInformation("type: " + mail.selectionType + ", file:" + mail.uploadedDocs);

      var phauser = await Context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == IdentityService.UserId);
      var fullName = phauser.FullName.TrimEnd();
      var names = fullName.Split(' ');
      string userInitial = " User(";
      string fN_initial = fullName.Substring(0, 1) != "J" ? String.Concat(userInitial, fullName.Substring(0, 1)) : String.Concat(userInitial, fullName.Substring(0, 2));
      //string lN_initial = names.Length == 2 ? names[1].Substring(0, 1) : ":";
      string lN_initial = String.Concat(userInitial, fullName.Substring(0, 2)) == " User(Ja" ? names[1].Substring(0, 2) : names[1].Substring(0, 1);

      lN_initial = String.Concat(userInitial, fullName.Substring(0, 2)) == " User(Jo" && fullName != "Jorge Gonzalez" ? names[1].Substring(0, 3) : names[1].Substring(0, 1);


      //userInitial = lN_initial.Length == 1 ? String.Concat(fN_initial, String.Concat(lN_initial, ") ")) : String.Concat(fN_initial, ") ");
      userInitial = String.Concat(fN_initial, String.Concat(lN_initial, ") "));


      Entities.OutreachReporting note = null;
      Entities.CareFiles careFilesDetails = null;
      //getting full file path of Uploaded file  
      string CSVFilePath = @"C:\EmailBlast\" + mail.uploadedDocs;

      //string CSVFilePath = System.IO.Path.GetFullPath(mail.uploadedDocs);
      _logger.LogInformation("file:" + CSVFilePath);
      //Reading All text  
      string ReadCSV = File.ReadAllText(CSVFilePath);

      try
      {
        if (mail.selectionType.ToLower().Equals("carecoordination"))
        {
          if (ReadCSV.Contains("cH_MEMBER_ID,firstName,lastName,client,groupName,carrier,state,phone,eMail,outReachReason,file,outReachRequest,sphNotes,engaged_Non_Enagaged,startDate,ispcp,clinicaL_RISK,attempts,assignedTo,relationship,OutreachDate,OutreachNote,OutreachPurpose,OutreachFrequency,OutreachMethod,OutreachMemResponse"))
          {
            _logger.LogInformation("Success");

            //Creating object of datatable  
            DataTable tblcsv = new DataTable();
            //creating columns  
            tblcsv.Columns.Add("cH_MEMBER_ID");
            tblcsv.Columns.Add("firstName");
            tblcsv.Columns.Add("lastName");
            tblcsv.Columns.Add("client");
            tblcsv.Columns.Add("groupName");
            tblcsv.Columns.Add("carrier");
            tblcsv.Columns.Add("state");
            tblcsv.Columns.Add("phone");
            tblcsv.Columns.Add("eMail");
            tblcsv.Columns.Add("outReachReason");
            tblcsv.Columns.Add("file");
            tblcsv.Columns.Add("outReachRequest");
            tblcsv.Columns.Add("sphNotes");
            tblcsv.Columns.Add("engaged_Non_Enagaged");
            tblcsv.Columns.Add("startDate");
            tblcsv.Columns.Add("ispcp");
            tblcsv.Columns.Add("clinicaL_RISK");
            tblcsv.Columns.Add("attempts");
            tblcsv.Columns.Add("assignedTo");
            tblcsv.Columns.Add("relationship");
            tblcsv.Columns.Add("OutreachDate");
            tblcsv.Columns.Add("OutreachNote");
            tblcsv.Columns.Add("OutreachPurpose");
            tblcsv.Columns.Add("OutreachFrequency");
            tblcsv.Columns.Add("OutreachMethod");
            tblcsv.Columns.Add("OutreachMemResponse");

            //spliting row after new line  
            foreach (string csvRow in ReadCSV.Split('\n'))
            {
              if (!string.IsNullOrEmpty(csvRow))
              {
                if (csvRow[0] != ',')
                {
                  //Adding each row into datatable  
                  tblcsv.Rows.Add();
                  int count = 0;
                  string temp = null;
                  string test = null;
                  //_logger.LogInformation("csvRow[0]:" + csvRow.Split(','));
                  foreach (string FileRec in csvRow.Split(','))
                  {
                    _logger.LogInformation("FileRec:" + FileRec);
                    if (!String.IsNullOrEmpty(FileRec))
                    {
                      if (FileRec[0] == '"' || FileRec[0] == ' ' || FileRec.EndsWith('"'))
                      {

                        if (FileRec.EndsWith('"'))
                        {
                          if (String.IsNullOrEmpty(temp))
                          {
                            tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                            count++;
                            temp = null;
                          }
                          else
                          {
                            temp = temp + FileRec;
                            tblcsv.Rows[tblcsv.Rows.Count - 1][count] = temp.Substring(1, temp.Length - 2);
                            count++;
                            temp = null;
                          }

                        }
                        else if (FileRec[0] == '"' || FileRec[0] == ' ')
                        {

                          if (String.IsNullOrEmpty(temp))
                          {
                            temp = FileRec + ",";
                          }
                          else
                          {
                            temp = temp + FileRec + ",";
                          }
                        }
                      }
                      else
                      {

                        if (String.IsNullOrEmpty(temp))
                        {
                          tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                          count++;
                        }
                        else
                        {
                          temp = temp + FileRec + ",";
                        }

                      }
                    }
                    else
                    {
                      //tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                      //count++;
                      note = null;
                      return note;
                    }
                  }
                }
              }
            }

            foreach (System.Data.DataRow row in tblcsv.Rows)
            {
              note = null;
              //_logger.LogInformation("length:" + Convert.ToString(row[0]).Length);
              if ((Convert.ToString(row[0]).Length != 0 && Convert.ToString(row[0]) != "cH_MEMBER_ID") && Convert.ToString(row[20]).Length != 0 && Convert.ToString(row[21]).Length != 0 && Convert.ToString(row[22]).Length != 0 && Convert.ToString(row[23]).Length != 0 && Convert.ToString(row[24]).Length != 0 && Convert.ToString(row[25]).Length != 0)
              {

                if (note == null)
                {
                  note = new Entities.OutreachReporting()
                  {
                    ChMemberId = Convert.ToInt32(row[0]),
                    RecordDate = DateTime.Now,
                    CreatedTimestamp = DateTime.Now,
                  };
                  await Context.OutreachReportings.AddAsync(note);
                }
                note.OutreachDate = Convert.ToDateTime(row[20], new System.Globalization.CultureInfo("en-US"));
                note.OutreachNote = row[21].ToString();
                note.OutreachPurpose = row[22].ToString();
                note.OutreachFrequency = row[23].ToString();
                note.OutreachMethod = row[24].ToString();
                note.OutreachMemResponse = row[25].ToString();
                note.UserInitial = userInitial;
                note.NotesType = "CCN";
                if (note != null)
                {
                  note.UserLastEditedById = IdentityService.UserId;
                  note.LastEditedTimestamp = DateTime.Now;
                  await Context.SaveChangesAsync();

                  //if (Convert.ToString(row[0]) != "cH_MEMBER_ID")
                  //{
                  var query = Context.SnowflakeMembers.Where(x => x.ChMemberId == Convert.ToInt32(row[0])).FirstOrDefault();
                  try
                  {
                    careFilesDetails = await Context.CareFiles.FirstOrDefaultAsync(x => x.ClaimantFirstName.ToLower().Equals(query.FirstName.ToLower()) && x.ClaimantLastName.ToLower().Equals(query.LastName.ToLower()) && x.DateOfBirth == query.Dob && x.Status != "Closed" && x.File == "Searches");
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                      note.IsSuppressed = true;
                      note.UpdatedDate = DateTime.Now.Date;
                    }
                  }
                  catch (Exception ex)
                  {
                    _logger.LogInformation("message: " + ex.Message);
                  }
                  Context.SaveChangesNoAudit();
                  // }
                }
              }
              else if (Convert.ToString(row[0]) != "cH_MEMBER_ID")
              {
                return note;
              }
            }
          }







        }
        else if (mail.selectionType.ToLower().Equals("highrisk"))
        {

          if (ReadCSV.Contains("cH_MEMBER_ID,firsT_NAME,lasT_NAME,client,groupName,state,emaiL_ADDRESS,celL_PHONE,carrieR_NAME,assignedTo,relationship,engaged_Non_Enagaged,ispcp,clinicaL_RISK,attempts,OutreachDate,OutreachNote,OutreachPurpose,OutreachFrequency,OutreachMethod,OutreachMemberResponse"))
          {
            //Creating object of datatable  
            DataTable tblcsv = new DataTable();
            //creating columns  
            tblcsv.Columns.Add("cH_MEMBER_ID");
            tblcsv.Columns.Add("firsT_NAME");
            tblcsv.Columns.Add("lasT_NAME");
            tblcsv.Columns.Add("client");
            tblcsv.Columns.Add("groupName");
            tblcsv.Columns.Add("state");
            tblcsv.Columns.Add("emaiL_ADDRESS");
            tblcsv.Columns.Add("celL_PHONE");
            tblcsv.Columns.Add("carrieR_NAME");
            tblcsv.Columns.Add("assignedTo");
            tblcsv.Columns.Add("relationship");
            tblcsv.Columns.Add("engaged_Non_Enagaged");
            tblcsv.Columns.Add("ispcp");
            tblcsv.Columns.Add("clinicaL_RISK");
            tblcsv.Columns.Add("attempts");
            tblcsv.Columns.Add("OutreachDate");
            tblcsv.Columns.Add("OutreachNote");
            tblcsv.Columns.Add("OutreachPurpose");
            tblcsv.Columns.Add("OutreachFrequency");
            tblcsv.Columns.Add("OutreachMethod");
            tblcsv.Columns.Add("OutreachMemberResponse");

            //spliting row after new line  
            foreach (string csvRow in ReadCSV.Split('\n'))
            {
              if (!string.IsNullOrEmpty(csvRow))
              {
                if (csvRow[0] != ',')
                {
                  //Adding each row into datatable  
                  tblcsv.Rows.Add();
                  int count = 0;
                  string temp = null;
                  _logger.LogInformation("csvRow[0]:" + csvRow[0]);
                  foreach (string FileRec in csvRow.Split(','))
                  {
                    _logger.LogInformation("FileRec:" + FileRec);
                    if (!String.IsNullOrEmpty(FileRec))
                    {
                      if (FileRec[0] == '"' || FileRec[0] == ' ' || FileRec.EndsWith('"'))
                      {

                        if (FileRec.EndsWith('"'))
                        {
                          if (String.IsNullOrEmpty(temp))
                          {
                            tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                            count++;
                          }
                          else
                          {
                            temp = temp + FileRec;
                            tblcsv.Rows[tblcsv.Rows.Count - 1][count] = temp.Substring(1, temp.Length - 2);
                            count++;
                          }

                        }
                        else if (FileRec[0] == '"' || FileRec[0] == ' ')
                        {
                          if (String.IsNullOrEmpty(temp))
                          {
                            temp = FileRec + ",";
                          }
                          else
                          {
                            temp = temp + FileRec + ",";
                          }
                        }
                      }
                      else
                      {
                        tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                        count++;
                      }
                    }
                    else
                    {
                      //tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                      //count++;
                      note = null;
                      return note;
                    }
                  }
                }
              }
            }
            foreach (System.Data.DataRow row in tblcsv.Rows)
            {
              note = null;
              _logger.LogInformation("row[0]: " + row[0]);
              if ((Convert.ToString(row[0]).Length != 0 && Convert.ToString(row[0]) != "cH_MEMBER_ID") && Convert.ToString(row[15]).Length != 0 && Convert.ToString(row[16]).Length != 0 && Convert.ToString(row[17]).Length != 0 && Convert.ToString(row[18]).Length != 0 && Convert.ToString(row[19]).Length != 0 && Convert.ToString(row[20]).Length != 0)
              {

                var emailExists = false;
                var phonenumberExists = false;
                var chMember = await Context.SnowflakeMembers.FirstOrDefaultAsync(o => o.ChMemberId == Convert.ToInt32(row[0]));
                var member_details = await Context.MemberDetails.FirstOrDefaultAsync(o => o.ChMemberId == Convert.ToInt32(row[0]));
                if (chMember.EmailAddress == "N/A" && member_details.SecondaryContact == null)
                {
                  emailExists = false;
                }
                else
                {
                  emailExists = true;
                }
                if (chMember.CellPhone == "N/A" && chMember.WorkPhone == "N/A" && chMember.HomePhone == "N/A" && (chMember.SelfReported_Phone == null || chMember.SelfReported_Phone == "N/A") && (chMember.Updated_Phone == null || chMember.Updated_Phone == "N/A" || chMember.Updated_Phone == "0"))
                {
                  phonenumberExists = false;
                }
                else
                {
                  phonenumberExists = true;
                }

                if (note == null)
                {
                  note = new Entities.OutreachReporting()
                  {
                    ChMemberId = Convert.ToInt32(row[0]),
                    RecordDate = DateTime.Now,
                    CreatedTimestamp = DateTime.Now,
                  };
                  await Context.OutreachReportings.AddAsync(note);
                }

                note.OutreachDate = Convert.ToDateTime(row[15], new System.Globalization.CultureInfo("en-US"));
                note.OutreachNote = row[16].ToString();
                note.OutreachPurpose = row[17].ToString();
                note.OutreachFrequency = row[18].ToString();
                note.OutreachMethod = row[19].ToString();
                note.OutreachMemResponse = row[20].ToString();
                note.UserInitial = userInitial;
                note.NotesType = "CN";
                if (note != null)
                {
                  //note.UserLastEditedById = IdentityService.UserId;
                  //note.LastEditedTimestamp = DateTime.Now;
                  //await Context.SaveChangesAsync();

                  var query = Context.SnowflakeMembers.Where(x => x.ChMemberId == Convert.ToInt32(row[0])).FirstOrDefault();
                  try
                  {
                    var OutreachMemResponse = row[19].ToString();
                    var OutreachFrequency = row[17].ToString();

                    if (OutreachFrequency == "Third Attempt")  //OutreachFrequency
                    {
                      note.IsSuppressed = true;
                      note.UpdatedDate = DateTime.Now.Date;
                    }
                    if (OutreachMemResponse != null)  //OutreachMemResponse
                    {
                      if ((OutreachMemResponse.Trim() == "Agreed to communication") || (OutreachMemResponse.Trim() == "Appointment Scheduled") || (OutreachMemResponse.Trim() == "Declined Communication"))
                      {
                        note.IsSuppressed = true;
                        note.UpdatedDate = DateTime.Now.Date;
                      }
                    }

                    if (emailExists == false && phonenumberExists == true)
                    {
                      if (OutreachFrequency == "Second Attempt")
                      {
                        note.IsSuppressed = true;
                        note.UpdatedDate = DateTime.Now.Date;
                      }
                    }

                    if (phonenumberExists == false && emailExists == true)
                    {
                      if (OutreachFrequency == "Second Attempt")
                      {
                        note.IsSuppressed = true;
                        note.UpdatedDate = DateTime.Now.Date;
                      }
                    }

                    if (OutreachMemResponse.Contains("Bad Email"))
                    {
                      if ((chMember.HomePhone != "N/A") || (chMember.WorkPhone != "N/A") || (chMember.CellPhone != "N/A") || (chMember.SelfReported_Phone != null) || (chMember.SelfReported_Phone != "N/A") || chMember.Updated_Phone != null || chMember.Updated_Phone != "N/A" || chMember.Updated_Phone != "0")
                      {

                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                      else
                      {
                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                    }

                    if (OutreachMemResponse.Contains("Bad Home Number") || OutreachMemResponse.Contains("Bad Mobile Number") || OutreachMemResponse.Contains("Bad Work Number"))
                    {
                      if (chMember.EmailAddress != "N/A" || member_details.SecondaryContact != null)
                      {

                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                      else
                      {
                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                    }

                    var outReachWithBadNumber = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(row[0]) && ((x.OutreachMemResponse.Equals("Bad Home Number")) || (x.OutreachMemResponse.Equals("Bad Mobile Number")) || (x.OutreachMemResponse.Equals("Bad Work Number"))));
                    if (outReachWithBadNumber != null)
                    {
                      if (emailExists == true)
                      {
                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                    }

                    var outReachWithBadEmail = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(row[0]) && x.OutreachMemResponse.Equals("Bad Email"));
                    if (outReachWithBadEmail != null)
                    {
                      if (phonenumberExists == true)
                      {
                        if (OutreachFrequency == "Second Attempt")
                        {
                          note.IsSuppressed = true;
                          note.UpdatedDate = DateTime.Now.Date;
                        }
                      }
                    }

                    note.UserLastEditedById = IdentityService.UserId;
                    note.LastEditedTimestamp = DateTime.Now;
                    await Context.SaveChangesAsync();
                  }
                  catch (Exception ex)
                  {
                    _logger.LogInformation("message: " + ex.Message);
                  }
                  Context.SaveChangesNoAudit();

                }
              }
              else if (Convert.ToString(row[0]) != "cH_MEMBER_ID")
              {
                return note;
              }

            }
          }

        }
      }
      catch (Exception ex)
      {
        _logger.LogInformation("error: " + ex.Message);
      }
      return note;
    }


    //public async Task<Entities.OutreachReporting> SaveOutreachEmailBlastAsync(EmailBlast mail)
    //{
    //  Entities.OutreachReporting note = null;
    //  Entities.CareFiles careFilesDetails = null;
    //  string ccSheetName = "CareDetals";
    //  string hRSheetName = "HighRiskMembers";
    //  long TotalRecords;
    //  _logger.LogInformation("type: " + mail.selectionType + ", file:" + mail.uploadedDocs);
    //  try
    //  {
    //    //Create Excel Connection
    //    string path = @"C:\EmailBlast\" + mail.uploadedDocs;
    //    //string path = @"C:\EmailBlast\CareCoordinationEailBlast 11.08.2022.xlsx";  
    //    string connectionStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0 xml;";
    //    OleDbConnection cnn = new OleDbConnection(connectionStr);

    //    //Get Sheet Name
    //    cnn.Open();
    //    DataTable dtSheet = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
    //    string sheetname;
    //    if (mail.selectionType.ToLower().Equals("carecoordination"))
    //    {
    //      try
    //      {
    //        foreach (DataRow drSheet in dtSheet.Rows)
    //        {
    //          if (drSheet["TABLE_NAME"].ToString().Contains("$"))
    //          {
    //            sheetname = drSheet["TABLE_NAME"].ToString();
    //            if (sheetname != null)
    //            {
    //              if (sheetname.Contains(ccSheetName) == true && sheetname.Contains("FilterDatabase") == false)
    //              {
    //                OleDbCommand Econ = new OleDbCommand("select * from [" + sheetname + "]", cnn);
    //                OleDbDataAdapter oda = new OleDbDataAdapter(Econ);
    //                DataTable dt = new DataTable();
    //                oda.Fill(dt);

    //                //Prepare Header columns list 
    //                string ExcelHeaderColumn = "";

    //                for (int i = 0; i < dt.Columns.Count; i++)
    //                {
    //                  if (i != dt.Columns.Count - 1)
    //                    ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'" + ",";
    //                  else
    //                    ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'";
    //                }

    //                TotalRecords = dt.Rows.Count;
    //                if (ExcelHeaderColumn.Contains("cH_MEMBER_ID") && ExcelHeaderColumn.Contains("OutreachMethod") && ExcelHeaderColumn.Contains("OutreachDate") && ExcelHeaderColumn.Contains("OutreachFrequency") && ExcelHeaderColumn.Contains("OutreachPurpose") && ExcelHeaderColumn.Contains("OutreachNote"))
    //                {
    //                  foreach (DataRow row in dt.Rows)
    //                  {
    //                    note = null;
    //                    if (!DBNull.Value.Equals(row["cH_MEMBER_ID"]))
    //                    {
    //                      if (note == null)
    //                      {
    //                        note = new Entities.OutreachReporting()
    //                        {
    //                          ChMemberId = Convert.ToInt32(row["cH_MEMBER_ID"]),
    //                          RecordDate = DateTime.Now,
    //                          CreatedTimestamp = DateTime.Now,
    //                        };
    //                        await Context.OutreachReportings.AddAsync(note);
    //                      }

    //                      note.OutreachMethod = row["OutreachMethod"].ToString();
    //                      note.OutreachDate = Convert.ToDateTime(row["OutreachDate"], new System.Globalization.CultureInfo("en-US"));
    //                      note.OutreachFrequency = row["OutreachFrequency"].ToString();
    //                      note.OutreachPurpose = row["OutreachPurpose"].ToString();
    //                      note.OutreachNote = row["OutreachNote"].ToString();
    //                      note.UserLastEditedById = IdentityService.UserId;
    //                      note.LastEditedTimestamp = DateTime.Now;
    //                      await Context.SaveChangesAsync();

    //                      var query = Context.SnowflakeMembers.Where(x => x.ChMemberId == Convert.ToInt32(row["cH_MEMBER_ID"])).FirstOrDefault();
    //                      try
    //                      {
    //                        careFilesDetails = await Context.CareFiles.FirstOrDefaultAsync(x => x.ClaimantFirstName.ToLower().Equals(query.FirstName.ToLower()) && x.ClaimantLastName.ToLower().Equals(query.LastName.ToLower()) && x.DateOfBirth == query.Dob && x.Status != "Closed" && x.File == "Searches");
    //                        if (careFilesDetails != null)
    //                        {
    //                          careFilesDetails.Status = "Closed";
    //                          note.IsSuppressed = true;
    //                          note.UpdatedDate = DateTime.Now.Date;
    //                        }
    //                      }
    //                      catch (Exception ex)
    //                      {
    //                        //Console.WriteLine(ex.Message);
    //                        _logger.LogInformation("message: " + ex.Message);
    //                      }
    //                      Context.SaveChangesNoAudit();
    //                    }
    //                  }
    //                }
    //                else
    //                {
    //                  return note;
    //                }
    //              }
    //            }
    //          }
    //        }
    //        cnn.Close();
    //      }
    //      catch (Exception ex)
    //      {
    //        //Console.WriteLine(ex.Message);
    //        _logger.LogInformation("message: " + ex.Message);
    //      }
    //    }
    //    else if (mail.selectionType.ToLower().Equals("highrisk"))
    //    {
    //      try
    //      {
    //        foreach (DataRow drSheet in dtSheet.Rows)
    //        {
    //          if (drSheet["TABLE_NAME"].ToString().Contains("$"))
    //          {
    //            sheetname = drSheet["TABLE_NAME"].ToString();
    //            if (sheetname != null)
    //            {
    //              if (sheetname.Contains(hRSheetName) == true && sheetname.Contains("FilterDatabase") == false)
    //              {
    //                OleDbCommand Econ = new OleDbCommand("select * from [" + sheetname + "]", cnn);
    //                OleDbDataAdapter oda = new OleDbDataAdapter(Econ);
    //                DataTable dt = new DataTable();
    //                oda.Fill(dt);

    //                //Prepare Header columns list 
    //                string ExcelHeaderColumn = "";

    //                for (int i = 0; i < dt.Columns.Count; i++)
    //                {
    //                  if (i != dt.Columns.Count - 1)
    //                    ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'" + ",";
    //                  else
    //                    ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'";
    //                }

    //                TotalRecords = dt.Rows.Count;
    //                if (ExcelHeaderColumn.Contains("cH_MEMBER_ID") && ExcelHeaderColumn.Contains("OutreachMethod") && ExcelHeaderColumn.Contains("OutreachDate") && ExcelHeaderColumn.Contains("OutreachFrequency") && ExcelHeaderColumn.Contains("OutreachMemberResponse") && ExcelHeaderColumn.Contains("OutreachPurpose") && ExcelHeaderColumn.Contains("OutreachNote"))
    //                {
    //                  _logger.LogInformation($"{DateTimeOffset.Now} Verified");
    //                  foreach (DataRow row in dt.Rows)
    //                  {
    //                    note = null;
    //                    if (!DBNull.Value.Equals(row["cH_MEMBER_ID"]))
    //                    {
    //                      if (note == null)
    //                      {
    //                        note = new Entities.OutreachReporting()
    //                        {
    //                          ChMemberId = Convert.ToInt32(row["cH_MEMBER_ID"]),
    //                          RecordDate = DateTime.Now,
    //                          CreatedTimestamp = DateTime.Now,
    //                        };
    //                        await Context.OutreachReportings.AddAsync(note);
    //                      }

    //                      note.OutreachMethod = row["OutreachMethod"].ToString();
    //                      note.OutreachDate = Convert.ToDateTime(row["OutreachDate"], new System.Globalization.CultureInfo("en-US"));
    //                      note.OutreachFrequency = row["OutreachFrequency"].ToString();
    //                      note.OutreachMemResponse = row["OutreachMemberResponse"].ToString();
    //                      note.OutreachPurpose = row["OutreachPurpose"].ToString();
    //                      note.OutreachNote = row["OutreachNote"].ToString();
    //                      note.UserLastEditedById = IdentityService.UserId;
    //                      note.LastEditedTimestamp = DateTime.Now;
    //                      await Context.SaveChangesAsync();
    //                    }
    //                  }
    //                }
    //                else
    //                {
    //                  return note;
    //                }
    //              }
    //            }
    //          }
    //        }
    //        cnn.Close();
    //      }
    //      catch (Exception ex)
    //      {
    //        //Console.WriteLine(ex.Message);
    //        _logger.LogInformation("message: " + ex.Message);
    //      }
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    //Console.WriteLine(ex.Message);
    //    _logger.LogInformation("error: " + ex.Message);
    //  }
    //  return note;
    //}

    public async Task<Entities.OutreachReporting> SaveOutreachReportAsync(int chMemberId, string name, OutreachReport model)
    {
      Entities.OutreachReporting note = null;
      Entities.CareFiles careFilesDetail = null;
      var emailExists = false;
      var phonenumberExists = false;
      var chMember = await Context.SnowflakeMembers.FirstOrDefaultAsync(o => o.ChMemberId == chMemberId);

      var phauser = await Context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == IdentityService.UserId);
      var fullName = phauser.FullName.TrimEnd();
      var names = fullName.Split(' ');
      string userInitial = " User(";
      string fN_initial = fullName.Substring(0, 1) != "J" ? String.Concat(userInitial, fullName.Substring(0, 1)) : String.Concat(userInitial, fullName.Substring(0, 2));
      string lN_initial = fN_initial == " User(Ja" ? names[1].Substring(0, 2)
       : fN_initial == " User(Jo" && fullName != "Jorge Gonzalez" ? names[1].Substring(0, 3)
       : fN_initial == " User(M" && fullName == "Melissa Orosco" ? names[1].Substring(0, 3) : names[1].Substring(0, 1);

      userInitial = String.Concat(fN_initial, String.Concat(lN_initial, ") "));

      var member_details = await Context.MemberDetails.FirstOrDefaultAsync(o => o.ChMemberId == chMemberId);
      if (chMember.EmailAddress == "N/A" && member_details.SecondaryContact == null)
      {
        emailExists = false;
      }
      else
      {
        emailExists = true;
      }
      if (chMember.CellPhone == "N/A" && chMember.WorkPhone == "N/A" && chMember.HomePhone == "N/A" && (chMember.SelfReported_Phone == null || chMember.SelfReported_Phone == "N/A") && (chMember.Updated_Phone == null  || chMember.Updated_Phone == "N/A" ||  chMember.Updated_Phone == "0" ))
      {
        phonenumberExists = false;
      }
      else
      {
        phonenumberExists = true;
      }
      try
      {
        if (model.Id.HasValue)
        {
          note = await Context.OutreachReportings
            .FirstOrDefaultAsync(x => x.ChMemberId == chMemberId && x.Id == model.Id);
        }

        if (note == null)
        {
          note = new Entities.OutreachReporting()
          {
            ChMemberId = chMemberId,
            RecordDate = DateTime.Now,
            CreatedTimestamp = DateTime.Now,
          };
          await Context.OutreachReportings.AddAsync(note);
        }

        note.OutreachMethod = model.OutreachMethod;
        note.OutreachDate = model.OutreachDate;
        note.OutreachFrequency = model.OutreachFrequency;
        note.OutreachMemResponse = model.OutreachMemResponse;
        note.OutreachPurpose = model.OutreachPurpose;
        //note.OutreachNote =  String.Concat(model.OutreachNote, userInitial);
        note.OutreachNote = model.OutreachNote;
        note.NotesType = model.NotesType;
        note.UserInitial = userInitial;
        note.UserLastEditedById = IdentityService.UserId;
        note.LastEditedTimestamp = DateTime.Now;
        //note.LastEditedTimestamp = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        
        var query = Context.SnowflakeMembers.Where(x => x.ChMemberId == Convert.ToInt32(chMemberId)).FirstOrDefault();
        try
        {
          //careFilesDetails = await Context.CareFiles.FirstOrDefaultAsync(x => x.ClaimantFirstName.Contains(query.FirstName) && x.ClaimantLastName.Contains(query.LastName) && x.DateOfBirth == query.Dob && x.Status!="Closed");
          careFilesDetail = await Context.CareFiles.FirstOrDefaultAsync(x => x.ClaimantFirstName.ToLower().Equals(query.FirstName.ToLower()) && x.ClaimantLastName.ToLower().Equals(query.LastName.ToLower()) && x.DateOfBirth == query.Dob && x.Status != "Closed");
          List<Entities.CareFiles> careFilesList = null;
          careFilesList = await Context.CareFiles.Where(x => x.ClaimantFirstName.ToLower().Equals(query.FirstName.ToLower()) && x.ClaimantLastName.ToLower().Equals(query.LastName.ToLower()) && x.DateOfBirth == query.Dob && x.Status != "Closed").ToListAsync();

          foreach (Entities.CareFiles careFilesDetails in careFilesList)
          {
            if (model.OutreachMemResponse != null)
            {
              if ((model.OutreachMemResponse == "Agreed to communication") || (model.OutreachMemResponse == "Appointment Scheduled") || (model.OutreachMemResponse == "Declined Communication"))
              {
                note.IsSuppressed = true;
                note.UpdatedDate = DateTime.Now.Date;
                if (careFilesDetails != null)
                {
                  careFilesDetails.Status = "Closed";
                }
              }

              if (model.OutreachPurpose.Contains("Care Coordination"))
              {
                if (model.OutreachFrequency == "Third Attempt")
                {
                  if (careFilesDetails != null)
                  {
                    careFilesDetails.Status = "Closed";
                  }
                }
                if (careFilesDetails != null)
                {
                  if (careFilesDetails.File == "Searches")
                  {
                    careFilesDetails.Status = "Closed";
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }

              if (model.OutreachFrequency == "Third Attempt")
              {
                if (careFilesDetails != null)
                {
                  careFilesDetails.Status = "Closed";
                }
                note.IsSuppressed = true;
                note.UpdatedDate = DateTime.Now.Date;
              }

              if (model.OutreachMemResponse == "No Contact Information" && model.OutreachPurpose.Contains("Care Coordination"))
              {

                if (chMember == null)
                  throw new ArgumentException("Invalid member");
                if (emailExists == false && phonenumberExists == false)
                {
                  if (careFilesDetails != null)
                  {
                    careFilesDetails.Status = "Closed";
                  }
                }

                if (emailExists == false && phonenumberExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }

                if (phonenumberExists == false && emailExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }

              }

              if (emailExists == false && phonenumberExists == true)
              {
                if (model.OutreachFrequency == "Second Attempt")
                {
                  if (careFilesDetails != null)
                  {
                    careFilesDetails.Status = "Closed";
                  }
                  note.IsSuppressed = true;
                  note.UpdatedDate = DateTime.Now.Date;
                }
              }
              if (phonenumberExists == false && emailExists == true)
              {
                if (model.OutreachFrequency == "Second Attempt")
                {
                  if (careFilesDetails != null)
                  {
                    careFilesDetails.Status = "Closed";
                  }
                  note.IsSuppressed = true;
                  note.UpdatedDate = DateTime.Now.Date;
                }
              }

              if (model.OutreachMemResponse.Contains("Bad Email"))
              {
                if ((chMember.HomePhone != "N/A") || (chMember.WorkPhone != "N/A") || (chMember.CellPhone != "N/A") || (chMember.SelfReported_Phone != null) || (chMember.SelfReported_Phone != "N/A") || chMember.Updated_Phone != null || chMember.Updated_Phone != "N/A" || chMember.Updated_Phone != "0")
                {
                  // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
                else
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }

              if (model.OutreachMemResponse.Contains("Bad Home Number") || model.OutreachMemResponse.Contains("Bad Mobile Number") || model.OutreachMemResponse.Contains("Bad Work Number"))
              {
                if (chMember.EmailAddress != "N/A" || member_details.SecondaryContact != null)
                {
                  // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
                else
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }

              var outReachWithBadNumber = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && ((x.OutreachMemResponse.Equals("Bad Home Number")) || (x.OutreachMemResponse.Equals("Bad Mobile Number")) || (x.OutreachMemResponse.Equals("Bad Work Number"))));
              if (outReachWithBadNumber != null)
              {
                if (emailExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }
              var outReachWithBadEmail = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && x.OutreachMemResponse.Equals("Bad Email"));
              if (outReachWithBadEmail != null)
              {
                if (phonenumberExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    if (careFilesDetails != null)
                    {
                      careFilesDetails.Status = "Closed";
                    }
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }
            }

          }

          if(careFilesDetail == null)
          {

            if (model.OutreachMemResponse != null)
            {
              if ((model.OutreachMemResponse == "Agreed to communication") || (model.OutreachMemResponse == "Appointment Scheduled") || (model.OutreachMemResponse == "Declined Communication"))
              {
                note.IsSuppressed = true;
                note.UpdatedDate = DateTime.Now.Date;
  
              }

              if (model.OutreachFrequency == "Third Attempt")
              {
                note.IsSuppressed = true;
                note.UpdatedDate = DateTime.Now.Date;
              }

              if (model.OutreachMemResponse == "No Contact Information" && model.OutreachPurpose.Contains("Care Coordination"))
              {

                if (chMember == null)
                  throw new ArgumentException("Invalid member");

                if (emailExists == false && phonenumberExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }

                if (phonenumberExists == false && emailExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }

              }

              
              if (emailExists == false && phonenumberExists == true)
              {
                if (model.OutreachFrequency == "Second Attempt")
                {
                  note.IsSuppressed = true;
                  note.UpdatedDate = DateTime.Now.Date;
                }
              }
              if (phonenumberExists == false && emailExists == true)
              {
                if (model.OutreachFrequency == "Second Attempt")
                {
                  note.IsSuppressed = true;
                  note.UpdatedDate = DateTime.Now.Date;
                }
              }

              if (model.OutreachMemResponse.Contains("Bad Email"))
              {
                if ((chMember.HomePhone != "N/A") || (chMember.WorkPhone != "N/A") || (chMember.CellPhone != "N/A") || (chMember.SelfReported_Phone != null) || (chMember.SelfReported_Phone != "N/A") || chMember.Updated_Phone != null || chMember.Updated_Phone != "N/A" || chMember.Updated_Phone != "0")
                {
                  // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
                else
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }

              if (model.OutreachMemResponse.Contains("Bad Home Number") || model.OutreachMemResponse.Contains("Bad Mobile Number") || model.OutreachMemResponse.Contains("Bad Work Number"))
              {
                if (chMember.EmailAddress != "N/A" || member_details.SecondaryContact != null)
                {
                  // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
                else
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }

              var outReachWithBadNumber = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && ((x.OutreachMemResponse.Equals("Bad Home Number")) || (x.OutreachMemResponse.Equals("Bad Mobile Number")) || (x.OutreachMemResponse.Equals("Bad Work Number"))));
              if (outReachWithBadNumber != null)
              {
                if (emailExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }
              var outReachWithBadEmail = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && x.OutreachMemResponse.Equals("Bad Email"));
              if (outReachWithBadEmail != null)
              {
                if (phonenumberExists == true)
                {
                  if (model.OutreachFrequency == "Second Attempt")
                  {
                    note.IsSuppressed = true;
                    note.UpdatedDate = DateTime.Now.Date;
                  }
                }
              }
            }
          }


         

          //if (model.OutreachMemResponse != null)
          //{
          //  if ((model.OutreachMemResponse == "Agreed to communication") || (model.OutreachMemResponse == "Appointment Scheduled") || (model.OutreachMemResponse == "Declined Communication"))
          //  {
          //    note.IsSuppressed = true;
          //    note.UpdatedDate = DateTime.Now.Date;
          //    if (careFilesDetails != null)
          //    {
          //      careFilesDetails.Status = "Closed";
          //    }
          //  }

          //  if (model.OutreachPurpose.Contains("Care Coordination"))
          //  {
          //    if (model.OutreachFrequency == "Third Attempt")
          //    {
          //      if (careFilesDetails != null)
          //      {
          //        careFilesDetails.Status = "Closed";
          //      }
          //    }
          //    if (careFilesDetails != null)
          //    {
          //      if (careFilesDetails.File == "Searches")
          //      {
          //        careFilesDetails.Status = "Closed";
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //  }

          //  if (model.OutreachFrequency == "Third Attempt")
          //  {
          //    if (careFilesDetails != null)
          //    {
          //      careFilesDetails.Status = "Closed";
          //    }
          //    note.IsSuppressed = true;
          //    note.UpdatedDate = DateTime.Now.Date;
          //  }

          //  if (model.OutreachMemResponse == "No Contact Information" && model.OutreachPurpose.Contains("Care Coordination"))
          //  {

          //    if (chMember == null)
          //      throw new ArgumentException("Invalid member");
          //    if (emailExists == false && phonenumberExists == false)
          //    {
          //      if (careFilesDetails != null)
          //      {
          //        careFilesDetails.Status = "Closed";
          //      }
          //    }

          //    if (emailExists == false && phonenumberExists == true)
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }

          //    if (phonenumberExists == false && emailExists == true)
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }

          //  }

          //  if (emailExists == false && phonenumberExists == true)
          //  {
          //    if (model.OutreachFrequency == "Second Attempt")
          //    {
          //      if (careFilesDetails != null)
          //      {
          //        careFilesDetails.Status = "Closed";
          //      }
          //      note.IsSuppressed = true;
          //      note.UpdatedDate = DateTime.Now.Date;
          //    }
          //  }
          //  if (phonenumberExists == false && emailExists == true)
          //  {
          //    if (model.OutreachFrequency == "Second Attempt")
          //    {
          //      if (careFilesDetails != null)
          //      {
          //        careFilesDetails.Status = "Closed";
          //      }
          //      note.IsSuppressed = true;
          //      note.UpdatedDate = DateTime.Now.Date;
          //    }
          //  }

          //  if (model.OutreachMemResponse.Contains("Bad Email"))
          //  {
          //    if ((chMember.HomePhone != null) || (chMember.WorkPhone != null) || (chMember.CellPhone != null))
          //    {
          //      // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //    else
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //  }

          //  if (model.OutreachMemResponse.Contains("Bad Home Number") || model.OutreachMemResponse.Contains("Bad Mobile Number") || model.OutreachMemResponse.Contains("Bad Work Number"))
          //  {
          //    if (chMember.EmailAddress != null)
          //    {
          //      // var outreachAttempts = Context.OutreachReportings.Where(x => (x.ChMemberId == chMemberId)).OrderByDescending(x => x.RecordDate).Select(c => new { c.OutreachFrequency }).First();
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //    else
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //  }

          //  var outReachWithBadNumber = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && ( (x.OutreachMemResponse.Equals("Bad Home Number")) || (x.OutreachMemResponse.Equals("Bad Mobile Number")) || (x.OutreachMemResponse.Equals("Bad Work Number")) ));
          //  if (outReachWithBadNumber != null)
          //  {
          //    if ( emailExists == true)
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //  }
          //  var outReachWithBadEmail = await Context.OutreachReportings.FirstOrDefaultAsync(x => x.ChMemberId == Convert.ToInt32(chMemberId) && x.OutreachMemResponse.Equals("Bad Email") );
          //  if (outReachWithBadEmail != null)
          //  {
          //    if (phonenumberExists == true)
          //    {
          //      if (model.OutreachFrequency == "Second Attempt")
          //      {
          //        if (careFilesDetails != null)
          //        {
          //          careFilesDetails.Status = "Closed";
          //        }
          //        note.IsSuppressed = true;
          //        note.UpdatedDate = DateTime.Now.Date;
          //      }
          //    }
          //  }
          //}

        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
        Context.SaveChangesNoAudit();
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return note;
    }

    #endregion

    #region Maintenance Tables

    public async Task<bool> DeleteMaintenanceProgramAsync(int chMemberId, int programId)
    {
      if (programId <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(programId));
      }
      // Get Report by reportId
      var snProgram = await Context.SnowflakeMemberPrograms
        .FirstOrDefaultAsync(x => x.Id.Equals(Convert.ToInt32(programId)) && x.ChMemberId.Equals(Convert.ToInt32(chMemberId)));

      // Validate if Report exists
      if (snProgram == null)
      {
        throw new ArgumentNullException(paramName: nameof(snProgram));
      }

      // Remove the Report from the repository
      //if (snProgram.ProgramDate >= DateTime.Now.Subtract(
      //  Config.GetDeleteMaximumAge()))
      //{
      //  Context.Remove(snProgram);
      //}
      //else
      //{
      //  return false;
      //}
      if (snProgram.Id == programId)
      {
        Context.Remove(snProgram);
      }
      else
      {
        return false;
      }
      // Delete Report in database
      await Context.SaveChangesAsync();
      return true;
    }

    #endregion

    #region CareCoordination
    public async Task<IEnumerable<Entities.CareFiles>> GetCareFilesDetails(string chMemberId)
    {
      List<Entities.CareFiles> careFilesDetails = null;
      if (chMemberId != null)
      {
        var query = Context.SnowflakeMembers.Where(x => x.ChMemberId == Convert.ToInt32(chMemberId)).FirstOrDefault();
        //careFilesDetails = await Context.CareFiles
        //.Where(x => x.ClaimantFirstName.Contains(query.FirstName) && x.ClaimantLastName.Contains(query.LastName) && x.DateOfBirth == query.Dob && (x.UpdatedOutreachReason != null ? x.UpdatedOutreachReason : x.OutreachReason) != null)
        //.OrderByDescending(x => x.Date).ToListAsync();

        //careFilesDetails = await Context.CareFiles.FromSqlRaw($"select * from dbo.CareFiles where [Claimant_First Name]='SPOORTHI'").ToListAsync();

        careFilesDetails = await Context.CareFiles
        .Where(x => x.ClaimantFirstName.Contains(query.FirstName) && x.ClaimantLastName.Contains(query.LastName) && x.DateOfBirth == query.Dob &&  x.OutreachReason != null)
        .OrderByDescending(x => x.Date).ToListAsync();
        return careFilesDetails;
      }

      return careFilesDetails;
    }

    public async Task<IEnumerable<Entities.CareFiles>> DownloadCareDetails()
    {

      List<Entities.CareFiles> careFilesDetails = null;

      careFilesDetails = await Context.CareFiles
 .Where(x => x.File != "Searches" && x.Status != "Closed" && (x.EmailSent == false || x.EmailSent == null) && (x.UpdatedOutreachReason != null ? x.UpdatedOutreachReason : x.OutreachReason) != null)
 .OrderByDescending(x => x.Date).ToListAsync();

      return careFilesDetails;


    }

    public async Task<IEnumerable<Entities.CareFiles>> DownloadSearches()
    {

      List<Entities.CareFiles> careFilesDetails = null;
      careFilesDetails = await Context.CareFiles
  .Where(x => x.File == "Searches" && x.Status != "Closed" && (x.UpdatedOutreachReason != null ? x.UpdatedOutreachReason : x.OutreachReason) != null)
  .OrderByDescending(x => x.Date).ToListAsync();
      return careFilesDetails;


    }
    #endregion

    #region Vendors

    public async Task<IEnumerable<KeyValue>> GetMemberVendorsAsync(int chMemberId)
    {
      int chEmployerId = await Context.SnowflakeMembers
        .Where(o => o.ChMemberId == chMemberId)
        .Select(o => o.ChEmployerId)
        .FirstOrDefaultAsync();

      var vendors = await Context.Vendors
        .Where(o => o.IsEnabled)
        .Where(o => !o.ChEmployerId.HasValue || o.ChEmployerId == chEmployerId)
        .Select(o => new KeyValue()
        {
          Id = o.Id,
          Value = o.VendorName,
        })
        .OrderBy(o => o.Value)
        .ToListAsync();

      return vendors;
    }

    #endregion



    #region CareAuditHistory
    public async Task<IEnumerable<Entities.Care_AuditLog>> GetCareAudithistory()
    {
      List<Entities.Care_AuditLog> GetcarehistoryDetails = null;
      GetcarehistoryDetails = await Context.Care_AuditLog.ToListAsync();
      foreach (Entities.Care_AuditLog filename in GetcarehistoryDetails)
      {
        if (filename.FileName.Contains("#") && filename.FileName.Contains("$"))
        {
          string fname = filename.FileName.Replace("#", "-").Replace("$", "");
          filename.FileName = fname;
        }
      }
      return GetcarehistoryDetails;

    }
    #endregion

    //#region  "CareCoordination"
    //public async Task<IEnumerable<Entities.CareFiles>> GetCareFilesDetails(string chMemberId)
    //{
    //  //Get Member FirstName and LastName
    //  var name = chMemberId.Split(' ');
    //  string fn = null;
    //  string ln = null;
    //  List<Entities.CareFiles> careFilesDetails = null;
    //  if (name.Length > 0)
    //  {
    //    fn = name[0];
    //    ln = name[1];
    //    careFilesDetails = await Context.CareFiles
    //    .Where(x => x.SubscriberFirstName == fn.ToString() && x.SubscriberLastName == ln.ToString() && x.File != "Searches" && (x.UpdatedOutreachReason != null ? x.UpdatedOutreachReason : x.OutreachReason) != null)
    //    .OrderByDescending(x => x.Date).ToListAsync();
    //    return careFilesDetails;
    //  }

    //  return careFilesDetails;
    //}
    //#endregion

    #region TaskviewDashboard
    public async Task<IEnumerable<Entities.CareFileTasksModel>> GetCareFileTaskDetails()
    {
      var careDetails = await Context.CareFileTaksAsync();

      return careDetails;
    }
    public async Task<IEnumerable<Entities.CareFiles>> GetAutoAssignDetails()
    {
      var careDistributionDetails = await Context.AutoTaskDistribution();

      return careDistributionDetails;
    }
    public async Task<IEnumerable<Entities.CareFiles>> GetCareFileDistributionDetails()
    {
      var careDistributionDetails = await Context.CareFiles.Where(x => x.AssignedTo == "22" || x.AssignedTo == "23").ToListAsync();

      return careDistributionDetails;
    }
    public async Task<IEnumerable<Entities.SearchesModel>> GetCareSearches()
    {
      var searches = await Context.SearchesTaskAsymc();
      return searches;
    }
    public async Task<IEnumerable<Entities.HigRiskMembersTasksModel>> GetHighRiskMembersDetails()
    {
      var riskMembers = await Context.HigRiskMembersTasksAsync();
      return riskMembers;
    }

    public List<Entities.AppointmentsListReturnModel> GetAppointmentsList()
    {
      var result = Context.AppointmentsList();
      return result;
    }
    #endregion

    #region TaskDistribution
    public async Task<IEnumerable<Entities.CareFiles>> TaskDistributionGeneric(string[] clients, string[] state, string[] pha, string date)
    {
      var res = await Context.TaskDistributionGeneric(clients, state, pha, date);
      return res;
    }
    public async Task<IEnumerable<Entities.CareFiles>> TaskDistributionEssilor(string[] client, string[] state, string[] pha, string cdate)
    {
      var res = await Context.ClientDistribution(client, state, pha, cdate);
      return res;

    }
    #endregion

    #region Notes
    public async Task<IEnumerable<Entities.MemberNotesModel>> GetNotesDetails(int id)
    {
      var notes = await Context.GetNotesAsync(id);

      return notes;
    }

    public async Task<IEnumerable<Entities.CommunicationOutreachNotesModel>> GetOutreachWithCommunicationNotesDetails(int id)
    {
      var notes = await Context.GetOutreachWithCommunicationNotesAsync(id);

      return notes;
    }

    public async Task<IEnumerable<Entities.CareCommunicationOutreachNotesModel>> GetOutreachWithCareCommunicationNotesDetails(int id)
    {
      var notes = await Context.GetOutreachWithCareCommunicationNotesAsync(id);

      return notes;
    }
    #endregion

    #region GenerateReports
    public async Task<IEnumerable<Entities.BillingReportModel>> GetBillingReportDetails(DateTime fromdate, DateTime todate)
    {
      var billingDetails = await Context.GenerateMonthlyReportAsync(fromdate, todate);

      return billingDetails;
    }
    public async Task<IEnumerable<Entities.WeeklyReportModel>> GetWeeklyReportDetails(DateTime fromdate, DateTime todate)
    {
      var weeklyReports = await Context.WeeklyReportsAsync(fromdate, todate);

      return weeklyReports;

    }
    public async Task<IEnumerable<Entities.ChalkMountainPHSreportModel>> GetChalkMountainPHSreports(DateTime fromdate, DateTime todate)
    {
      var PHSReports = await Context.ChalkMountainPHSreportsAsync(fromdate, todate);

      return PHSReports;
    }
    public async Task<IEnumerable<Entities.ChalkMountainProviderReportModel>> GetChalkMountainProviderReports(DateTime fromdate, DateTime todate)
    {
      var providerReports = await Context.ChalkMountainProviderReportsAsync(fromdate, todate);

      return providerReports;

    }

    public async Task<IEnumerable<Entities.SevenElevenMonthlyReportModel>> GetSevenElevenMonthlyReports(DateTime fromdate, DateTime todate)
    {
      var SevenElevenMonthlyReports = await Context.GetSevenElevenMonthlyReportsAsync(fromdate, todate);

      return SevenElevenMonthlyReports;
    }
    public async Task<IEnumerable<Entities.SevenElevenWeeklyReportModel>> GetSevenElevenWeeklyReports(DateTime fromdate, DateTime todate)
    {
      var SevenElevenWeeklyReports = await Context.GetSevenElevenWeeklyReportsAsync(fromdate, todate);

      return SevenElevenWeeklyReports;
    }

    public async Task<IEnumerable<Entities.SevenElevenWellRightReportModel>> GetSevenElevenWellRightReports(DateTime fromdate, DateTime todate)
    {
      var SevenElevenWellRightReports = await Context.GetSevenElevenWellRightReportsAsync(fromdate, todate);

      return SevenElevenWellRightReports;
    }

    #endregion

    //#region Email Service
    //public Task<EmailServiceResult> SendEmailAsync(EmailService service)
    //{
    //  Action<EmailServiceResult, string> addError = (res, msg) =>
    //  {
    //    res.Succeeded = false;
    //    res.Message = msg;
    //    res.Status = false;
    //  };

    //  var result = new EmailServiceResult()
    //  {
    //    Succeeded = false,
    //    Message = string.Empty,
    //    Status = false,
    //  };

    //  if (service != null)
    //  {
    //    try
    //    {
    //      if (result.ModelErrors.Count == 0)
    //      {
    //        MailMessage mail = new MailMessage();

    //        mail.From = new MailAddress("no-reply@converginghealth.com");
    //        //mail.Attachments.Add(new Attachment(service.uploadedDocs));
    //        mail.CC.Add("gladis.merlin@converginghealth.com");
    //        //mail.To.Add(service.to);
    //        mail.To.Add("aavinash.manimaran@converginghealth.com");
    //        mail.Subject = service.subject;
    //        mail.Body = service.message + "\n\n" + service.signature;

    //        SmtpClient smtp = new SmtpClient("smtp.office365.com");

    //        smtp.Port = 587;
    //        smtp.Credentials = new NetworkCredential("no-reply@converginghealth.com", "B$y@6U!23&*");
    //        smtp.EnableSsl = true;
    //        smtp.Send(mail);

    //        result.Succeeded = true;
    //        result.Message = string.Empty;
    //        result.Status = true;
    //      }
    //    }
    //    catch (Exception ex)
    //    {
    //      addError(result, "An unexpected error occurred while send mail");
    //    }
    //  }

    //  return result;
    //}

    //#endregion





  }

}