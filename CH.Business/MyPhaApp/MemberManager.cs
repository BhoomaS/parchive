using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using CH.Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;

using CH.Data;
using CH.Models.Common;
using CH.Models.MasterEmployer;
using CH.Models.Enums;
using CH.Models.MyPhaApp.Member;

namespace CH.Business.MyPhaApp
{
  public partial interface IMemberManager
  {
    Task<MemberSummary> GetMemberSummaryAsync();
    Task<MemberDetail> GetMemberDetailAsync();
    Task<SaveMemberResult> SaveMemberDetailAsync(MemberDetail model);
    Task<FileDownloadResult> GetMemberPhsPdfAsync();
    Task<Pha> GetPhaAsync();
    Task<IEnumerable<EmployerBenefit>> GetEmployerBenefitsAsync();
  }

  public partial class MemberManager : BaseManager, IMemberManager
  {
    private readonly IAuthManager _authManager;
    private readonly ITableauService _tableauService;

    public MemberManager(
      AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IMapper mapper,
      IAuthManager authManager,
      ITableauService tableauService)
      : base(context, identityService, cacheService, config, mapper)
    {
      _authManager = authManager;
      _tableauService = tableauService;
    }


    #region Member
    public async Task<MemberSummary> GetMemberSummaryAsync()
    {
      return await Mapper.ProjectTo<MemberSummary>(
        GetSessionSnowflakeMemberAsync()).FirstOrDefaultAsync();
    }

    public async Task<MemberDetail> GetMemberDetailAsync()
    {
      return await Mapper.ProjectTo<MemberDetail>(
        GetSessionSnowflakeMemberAsync()).FirstOrDefaultAsync();
    }

    private IQueryable<Entities.SnowflakeMember> GetSessionSnowflakeMemberAsync()
    {
      int chMemberId = this.IdentityService.ChMemberId.Value;
      return Context.SnowflakeMembers
          .Where(o => o.ChMemberId == chMemberId);
    }

    public async Task<SaveMemberResult> SaveMemberDetailAsync(MemberDetail model)
    {
      var result = new SaveMemberResult()
      {
        Detail = model,
      };

      // Force the model's ID to the current user's ChMemberId
      model.ChMemberId = IdentityService.ChMemberId.Value;

      using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
      using (var dbContext = Context.Clone())
      {
        var chMember = await dbContext.SnowflakeMembers
          .Include(o => o.ApplicationUsers)
          .FirstOrDefaultAsync(o => o.ChMemberId == model.ChMemberId);

        if (chMember == null)
          result.ModelErrors.Add(o => o.ChMemberId, "Invalid Member");

        if (result.ModelErrors.Count == 0)
        {
          var userDetail = await _authManager.GetApplicationUserDetailAsync(
            chMember.ApplicationUsers.SingleOrDefault().Id);

          if (!string.Equals(userDetail.Email, model.EmailAddress))
          {
            userDetail.Email = model.EmailAddress;
            var userSaveResult = await _authManager.SaveApplicationUserDetailAsync(
              userDetail, ApplicationRoleId.MyPhaMemberUser, dbContext);
            result.ModelErrors.AddRange(userSaveResult.ModelErrors);
          }

          if (result.ModelErrors.Count == 0)
          {
            chMember.FirstNameEdited = model.FirstName;
            chMember.MiddleNameEdited = model.MiddleName;
            chMember.LastNameEdited = model.LastName;

            //chMember.Dob = model.Dob; // READ-ONLY
            //chMember.Gender = model.Gender; // Not on this model

            chMember.Address1Edited = model.Address1;
            chMember.Address2Edited = model.Address2;
            chMember.CityEdited = model.City;
            //chMember.County = ? // NOT DISPLAYED
            chMember.StateEdited = model.State;
            chMember.ZipCodeEdited = model.ZipCode;

            // Update the profile email here, already updated the MyPHA account email above
            chMember.EmailAddressEdited = model.EmailAddress;
            chMember.CellPhoneEdited = model.CellPhone;
            chMember.WorkPhoneEdited = model.WorkPhone;
            chMember.HomePhoneEdited = model.HomePhone;
            //chMember.EthnicGroup = model.Ethnicity; // Not on this model
            //chMember.MaraRisk = model.MaraRisk; // READ-ONLY
            //chMember.ClinicalRisk = model.ClinicalRisk; // READ-ONLY
            //chMember.Relationship = model.RelationClass; // READ-ONLY
            chMember.LastEditedTimestamp = DateTime.Now;
            chMember.UserLastEditedById = IdentityService.UserId;
            chMember.IsModified = true;

            // Not in this model, not something users can edit at this time
            //var memberDetail = await dbContext.MemberDetails
            //	.FirstOrDefaultAsync(o => o.ChMemberId == model.ChMemberId);
            //if (memberDetail == null)
            //{
            //	// TODO: Create a new one if none currently
            //}
            //memberDetail.CodePreferredLanguageId = model.PrefLanguageId;
            //memberDetail.CodePreferredContactTimeId = model.PrefContactTimeId;
            //memberDetail.CodePreferredContactMethodId = model.PrefContactMethodId;
            //memberDetail.SecondaryContact = model.SecondaryContact;
            //memberDetail.CodeLearningStyleId = model.LearningStyleId;

            // TODO: Add these back if we ever allow users to save anything on their MemberDetail record
            //memberDetail.LastEditedTimestamp = DateTime.Now;
            //memberDetail.UserLastEditedById = IdentityService.UserId;

            await dbContext.SaveChangesAsync();

            // Complete this scope if no errors
            // This will also commit underlying call to SaveApplicationUserDetailAsync
            scope.Complete();
          }
        }
      }

      if (result.ModelErrors.Count == 0)
        result.Detail = await GetMemberDetailAsync();

      return result;
    }

    public async Task<FileDownloadResult> GetMemberPhsPdfAsync()
    {
      string contentType = "application/pdf";
      string fileName = "My Personal Health Summary.pdf";

      var sheets = new Dictionary<int, string>()
      {
        { 1, Config.GetTableauPhsViewSheet1() },
        { 2, Config.GetTableauPhsViewSheet2() },
        { 3, Config.GetTableauPhsViewSheet3() },
      };

      var sheetFileBytes = new ConcurrentBag<KeyValuePair<int, byte[]>>();
      //var sheetFileBytes = new ConcurrencyBag() new Dictionary<string, byte[]>();

      var parameters = new List<KeyValuePair<string, object>>()
      {
        new KeyValuePair<string, object>(
          Config.GetChMemberIdParamName(), IdentityService.ChMemberId),
      };

      foreach (var sheet in sheets)
      {
        sheetFileBytes.Add(new KeyValuePair<int, byte[]>(
          sheet.Key, await _tableauService.GetSheetPdfBytes(
            Config.GetTableauPhsViewWorkbook(),
            sheet.Value,
            forceRefresh: true,
            parameters)));
      }

      var fileBytes = sheetFileBytes.OrderBy(o => o.Key)
        .Select(o => o.Value);

      var resultFileBytes = iTextSharpUtils.MergePdfFileBytes(fileBytes);

      if (resultFileBytes != null && resultFileBytes.Length > 0)
        return new FileDownloadResult()
        {
          Succeeded = true,
          ContentType = contentType,
          FileContents = resultFileBytes,
          FileName = fileName,
        };

      return new FileDownloadResult()
      {
        Succeeded = false,
      };
    }



    #endregion


    #region Pha

    public async Task<Pha> GetPhaAsync()
    {
      int chMemberId = this.IdentityService.ChMemberId.Value;
      return await GetPhaAsync(chMemberId);
    }

    private async Task<Pha> GetPhaAsync(int chMemberId)
    {
      return await Mapper.ProjectTo<Pha>(Context.SnowflakeMembers
          .Where(o => o.ChMemberId == chMemberId)
          .Select(o => o.MemberDetail.UserAssigned))
        .FirstOrDefaultAsync();
    }

    #endregion


    #region Employer

    public async Task<IEnumerable<EmployerBenefit>> GetEmployerBenefitsAsync()
    {
      int chMemberId = this.IdentityService.ChMemberId.Value;

      return await Mapper.ProjectTo<EmployerBenefit>(Context.SnowflakeMembers
        .Where(o => o.ChMemberId == chMemberId)
        .Select(o => o.SnowflakeMasterEmployer.EmployerDetail)
        .SelectMany(o => o.EmployerBenefits)
        .Where(o => !o.IsDeleted && o.IsEnabled)
        .OrderByDescending(o => o.DisplayOrder.HasValue)
        .ThenBy(o => o.DisplayOrder))
        .ToListAsync();
    }

    #endregion

  }
}
